using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
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
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using static HMUI.IconSegmentedControl;

namespace ScoreSaber.Patches {
    internal class LeaderboardPatches : IInitializable, IAffinity {

        private readonly ScoreSaberLeaderboardViewController _scoresaberLeaderboardViewController;
        private PlatformLeaderboardViewController _platformLeaderboardViewController;

        private int _lastScopeIndex = -1;

        public LeaderboardPatches(ScoreSaberLeaderboardViewController scoresaberLeaderboardViewController) {
            _scoresaberLeaderboardViewController = scoresaberLeaderboardViewController;
        }

        public void Initialize() { }

        [AffinityPatch(typeof(PlatformLeaderboardViewController), nameof(PlatformLeaderboardViewController.Refresh))]
        [AffinityPrefix]
        bool PatchPlatformLeaderboardsRefresh(ref IDifficultyBeatmap ____difficultyBeatmap, ref List<LeaderboardTableView.ScoreData> ____scores, ref bool ____hasScoresData, ref LeaderboardTableView ____leaderboardTableView, ref int[] ____playerScorePos, ref PlatformLeaderboardsModel.ScoresScope ____scoresScope, ref LoadingControl ____loadingControl) {
            if (____difficultyBeatmap.level is CustomBeatmapLevel) {
                ____hasScoresData = false;
                ____scores.Clear();
                ____leaderboardTableView.SetScores(____scores, ____playerScorePos[(int)____scoresScope]);
                ____loadingControl.ShowLoading();

                _scoresaberLeaderboardViewController.isOST = false;
                _scoresaberLeaderboardViewController.RefreshLeaderboard(____difficultyBeatmap, ____leaderboardTableView, ____scoresScope, ____loadingControl, Guid.NewGuid().ToString()).RunTask();
                return false;
            } else {
                _scoresaberLeaderboardViewController.isOST = true;
                return true;
            }
        }

        private int ohmylord = 0;
        [AffinityPatch(typeof(LeaderboardTableView), nameof(LeaderboardTableView.CellForIdx))]
        void PatchLeaderboardTableView(ref LeaderboardTableView __instance, TableCell __result) {
            if (__instance.transform.parent.transform.parent.name == "PlatformLeaderboardViewController") {

                LeaderboardTableCell tableCell = (LeaderboardTableCell)__result;

                if (tableCell.gameObject.GetComponent<CellClicker>() == null) {
                    CellClicker cellClicker = tableCell.gameObject.AddComponent<CellClicker>();
                    cellClicker.onClick = _scoresaberLeaderboardViewController._infoButtons.InfoButtonClicked;
                    Plugin.Log.Info($"{cellClicker.index.ToString()} nalls");
                    cellClicker.index = ohmylord;
                    ohmylord++;
                    cellClicker.seperator = tableCell.GetField<Image, LeaderboardTableCell>("_separatorImage") as ImageView;
                }

                TextMeshProUGUI _playerNameText = tableCell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText");

                if (_scoresaberLeaderboardViewController.isOST) {
                    _playerNameText.richText = false;
                } else {
                    _playerNameText.richText = true;
                }
            }
        }

        [AffinityPatch(typeof(PlatformLeaderboardViewController), "DidActivate")]
        [AffinityPrefix]
        bool PatchPlatformLeaderboardDidActivatePrefix(ref PlatformLeaderboardViewController __instance) {
            _platformLeaderboardViewController = __instance;
            return true;
        }

        [AffinityPatch(typeof(PlatformLeaderboardViewController), "DidActivate")]
        [AffinityPostfix]
        void PatchPlatformLeaderboardDidActivatePostfix(ref bool firstActivation, ref Sprite ____friendsLeaderboardIcon, ref Sprite ____globalLeaderboardIcon, ref Sprite ____aroundPlayerLeaderboardIcon, ref IconSegmentedControl ____scopeSegmentedControl) {
            if (firstActivation) {
                _platformLeaderboardViewController?.InvokeMethod<object, PlatformLeaderboardViewController>("Refresh", true, true);

                if (Plugin.Settings.enableCountryLeaderboards) {
                    SetupScopeControl(____friendsLeaderboardIcon, ____globalLeaderboardIcon, ____aroundPlayerLeaderboardIcon, ____scopeSegmentedControl);
                }
            }
            if (Plugin.Settings.enableCountryLeaderboards) {
                ____scopeSegmentedControl.SelectCellWithNumber(_lastScopeIndex);
            }
        }

        private void SetupScopeControl(Sprite ____friendsLeaderboardIcon, Sprite ____globalLeaderboardIcon, Sprite ____aroundPlayerLeaderboardIcon, IconSegmentedControl ____scopeSegmentedControl) {

            Texture2D countryTexture = new Texture2D(64, 64);
            countryTexture.LoadImage(Utilities.GetResource(Assembly.GetExecutingAssembly(), "ScoreSaber.Resources.country.png"));
            countryTexture.Apply();

            Sprite _countryIcon = Sprite.Create(countryTexture, new Rect(0, 0, countryTexture.width, countryTexture.height), Vector2.zero);
            ____scopeSegmentedControl.SetData(new DataItem[] {
                    new DataItem(____globalLeaderboardIcon, "Global"),
                    new DataItem(____aroundPlayerLeaderboardIcon, "Around You"),
                    new DataItem(____friendsLeaderboardIcon, "Friends"),
                    new DataItem(_countryIcon, "Country"),
                });

            ____scopeSegmentedControl.didSelectCellEvent -= _platformLeaderboardViewController.HandleScopeSegmentedControlDidSelectCell;
            ____scopeSegmentedControl.didSelectCellEvent += ScopeSegmentedControl_didSelectCellEvent;
        }

        private void ScopeSegmentedControl_didSelectCellEvent(SegmentedControl segmentedControl, int cellNumber) {

            bool filterAroundCountry = false;

            switch (cellNumber) {
                case 0:
                    _platformLeaderboardViewController.SetStaticField("_scoresScope", PlatformLeaderboardsModel.ScoresScope.Global);
                    break;
                case 1:
                    _platformLeaderboardViewController.SetStaticField("_scoresScope", PlatformLeaderboardsModel.ScoresScope.AroundPlayer);
                    break;
                case 2:
                    _platformLeaderboardViewController.SetStaticField("_scoresScope", PlatformLeaderboardsModel.ScoresScope.Friends);
                    break;
                case 3:
                    filterAroundCountry = true;
                    break;
            }

            _lastScopeIndex = cellNumber;
            _scoresaberLeaderboardViewController.ChangeScope(filterAroundCountry);
        }

        // probably a better place to put this
        public class CellClicker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
            public Action<int> onClick;
            public int index;
            public ImageView seperator;
            private Vector3 originalScale;
            private bool isScaled = false;

            private Color origColour;
            private Color origColour0;
            private Color origColour1;

            private void Start() {
                originalScale = seperator.transform.localScale;
            }

            public void OnPointerClick(PointerEventData data) {
                BeatSaberUI.BasicUIAudioManager.HandleButtonClickEvent();
                onClick(index);
            }

            public void OnPointerEnter(PointerEventData eventData) {
                if (!isScaled) {
                    seperator.transform.localScale = originalScale * 1.8f;
                    isScaled = true;
                }

                origColour = seperator.color;
                origColour0 = seperator.color0;
                origColour1 = seperator.color1;


                seperator.color = Color.white;
                seperator.color0 = Color.white;
                seperator.color1 = new Color(1, 1, 1, 0);
            }

            public void OnPointerExit(PointerEventData eventData) {
                if (isScaled) {
                    seperator.transform.localScale = originalScale;
                    isScaled = false;
                }
                seperator.color = origColour;
                seperator.color0 = origColour0;
                seperator.color1 = origColour1;
            }

            private void OnDestroy() {
                onClick = null;
            }
        }

    }
}