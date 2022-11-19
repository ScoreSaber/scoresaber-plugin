#region

using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using ScoreSaber.Extensions;
using ScoreSaber.UI.Leaderboard;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using Zenject;
using static HMUI.IconSegmentedControl;

#endregion

namespace ScoreSaber.Patches {
    internal class LeaderboardPatches : IInitializable, IAffinity {
        private readonly ScoreSaberLeaderboardViewController _scoresaberLeaderboardViewController;

        private int _lastScopeIndex = -1;
        private PlatformLeaderboardViewController _platformLeaderboardViewController;

        public LeaderboardPatches(ScoreSaberLeaderboardViewController scoresaberLeaderboardViewController) {
            _scoresaberLeaderboardViewController = scoresaberLeaderboardViewController;
        }

        public void Initialize() { }

        [AffinityPatch(typeof(PlatformLeaderboardViewController), nameof(PlatformLeaderboardViewController.Refresh))]
        [AffinityPrefix]
        private bool PatchPlatformLeaderboardsRefresh(ref IDifficultyBeatmap ____difficultyBeatmap,
            ref List<LeaderboardTableView.ScoreData> ____scores, ref bool ____hasScoresData,
            ref LeaderboardTableView ____leaderboardTableView, ref int[] ____playerScorePos,
            ref PlatformLeaderboardsModel.ScoresScope ____scoresScope, ref LoadingControl ____loadingControl) {
            switch (____difficultyBeatmap.level) {
                case CustomBeatmapLevel _:
                    ____hasScoresData = false;
                    ____scores.Clear();
                    ____leaderboardTableView.SetScores(____scores, ____playerScorePos[(int)____scoresScope]);
                    ____loadingControl.ShowLoading();

                    _scoresaberLeaderboardViewController.isOST = false;
                    _scoresaberLeaderboardViewController.RefreshLeaderboard(____difficultyBeatmap,
                        ____leaderboardTableView,
                        ____scoresScope, ____loadingControl, Guid.NewGuid().ToString()).RunTask();
                    return false;
                default:
                    _scoresaberLeaderboardViewController.isOST = true;
                    return true;
            }
        }

        [AffinityPatch(typeof(LeaderboardTableView), nameof(LeaderboardTableView.CellForIdx))]
        private void PatchLeaderboardTableView(ref LeaderboardTableView __instance, TableCell __result) {
            switch (__instance.transform.parent.transform.parent.name) {
                case "PlatformLeaderboardViewController": {
                    LeaderboardTableCell tableCell = (LeaderboardTableCell)__result;
                    TextMeshProUGUI _playerNameText =
                        tableCell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText");

                    switch (_scoresaberLeaderboardViewController.isOST) {
                        case true:
                            _playerNameText.richText = false;
                            break;
                        default:
                            _playerNameText.richText = true;
                            break;
                    }

                    break;
                }
            }
        }

        [AffinityPatch(typeof(PlatformLeaderboardViewController), "DidActivate")]
        [AffinityPrefix]
        private bool PatchPlatformLeaderboardDidActivatePrefix(ref PlatformLeaderboardViewController __instance) {
            _platformLeaderboardViewController = __instance;
            return true;
        }

        [AffinityPatch(typeof(PlatformLeaderboardViewController), "DidActivate")]
        [AffinityPostfix]
        private void PatchPlatformLeaderboardDidActivatePostfix(ref bool firstActivation,
            ref Sprite ____friendsLeaderboardIcon, ref Sprite ____globalLeaderboardIcon,
            ref Sprite ____aroundPlayerLeaderboardIcon, ref IconSegmentedControl ____scopeSegmentedControl) {
            switch (firstActivation) {
                case true: {
                    _platformLeaderboardViewController?.InvokeMethod<object, PlatformLeaderboardViewController>(
                        "Refresh",
                        true, true);

                    switch (Plugin.Settings.enableCountryLeaderboards) {
                        case true:
                            SetupScopeControl(____friendsLeaderboardIcon, ____globalLeaderboardIcon,
                                ____aroundPlayerLeaderboardIcon, ____scopeSegmentedControl);
                            break;
                    }

                    break;
                }
            }

            switch (Plugin.Settings.enableCountryLeaderboards) {
                case true:
                    ____scopeSegmentedControl.SelectCellWithNumber(_lastScopeIndex);
                    break;
            }
        }

        private void SetupScopeControl(Sprite ____friendsLeaderboardIcon, Sprite ____globalLeaderboardIcon,
            Sprite ____aroundPlayerLeaderboardIcon, IconSegmentedControl ____scopeSegmentedControl) {
            Texture2D countryTexture = new Texture2D(64, 64);
            countryTexture.LoadImage(Utilities.GetResource(Assembly.GetExecutingAssembly(),
                "ScoreSaber.Resources.country.png"));
            countryTexture.Apply();

            Sprite _countryIcon = Sprite.Create(countryTexture,
                new Rect(0, 0, countryTexture.width, countryTexture.height), Vector2.zero);
            ____scopeSegmentedControl.SetData(new[] {
                new DataItem(____globalLeaderboardIcon, "Global"),
                new DataItem(____aroundPlayerLeaderboardIcon, "Around You"),
                new DataItem(____friendsLeaderboardIcon, "Friends"),
                new DataItem(_countryIcon, "Country")
            });

            ____scopeSegmentedControl.didSelectCellEvent -=
                _platformLeaderboardViewController.HandleScopeSegmentedControlDidSelectCell;
            ____scopeSegmentedControl.didSelectCellEvent += ScopeSegmentedControl_didSelectCellEvent;
        }

        private void ScopeSegmentedControl_didSelectCellEvent(SegmentedControl segmentedControl, int cellNumber) {
            bool filterAroundCountry = false;

            switch (cellNumber) {
                case 0:
                    _platformLeaderboardViewController.SetStaticField("_scoresScope",
                        PlatformLeaderboardsModel.ScoresScope.Global);
                    break;
                case 1:
                    _platformLeaderboardViewController.SetStaticField("_scoresScope",
                        PlatformLeaderboardsModel.ScoresScope.AroundPlayer);
                    break;
                case 2:
                    _platformLeaderboardViewController.SetStaticField("_scoresScope",
                        PlatformLeaderboardsModel.ScoresScope.Friends);
                    break;
                case 3:
                    filterAroundCountry = true;
                    break;
            }

            _lastScopeIndex = cellNumber;
            _scoresaberLeaderboardViewController.ChangeScope(filterAroundCountry);
        }
    }
}