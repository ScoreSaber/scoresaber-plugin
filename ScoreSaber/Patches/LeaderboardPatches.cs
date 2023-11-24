using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
using ScoreSaber.Extensions;
using ScoreSaber.UI.Leaderboard;
using SiraUtil.Affinity;
using System;
using System.Collections;
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

        private bool obtainedAnchor = false;
        private Vector2 normalAnchor = Vector2.zero;

        [AffinityPatch(typeof(LeaderboardTableView), nameof(LeaderboardTableView.CellForIdx))]
        void PatchLeaderboardTableView(ref LeaderboardTableView __instance, TableCell __result, int row) {
            if (__instance.transform.parent.transform.parent.name == "PlatformLeaderboardViewController") {
                LeaderboardTableCell tableCell = (LeaderboardTableCell)__result;

                CellClicker existingCellClicker = tableCell.gameObject.GetComponent<CellClicker>();
                if (existingCellClicker == null || existingCellClicker.index != row || _scoresaberLeaderboardViewController.isOST) {
                    if (existingCellClicker != null) {
                        GameObject.Destroy(existingCellClicker);
                    }

                    if (!_scoresaberLeaderboardViewController.isOST) {
                        CellClicker cellClicker = tableCell.gameObject.AddComponent<CellClicker>();
                        cellClicker.onClick = _scoresaberLeaderboardViewController._infoButtons.InfoButtonClicked;
                        cellClicker.index = row;
                        cellClicker.seperator = tableCell.GetField<Image, LeaderboardTableCell>("_separatorImage") as ImageView;
                        if(_scoresaberLeaderboardViewController._ImageHolders[row].profileImage.gameObject.GetComponent<CellClicker>() != null)
                        {
                            GameObject.Destroy(_scoresaberLeaderboardViewController._ImageHolders[row].profileImage.gameObject.GetComponent<CellClicker>());
                        }
                        cellClicker.CopyComponent<CellClicker>(_scoresaberLeaderboardViewController._ImageHolders[row].profileImage.gameObject);
                    }
                }

                TextMeshProUGUI _playerNameText = tableCell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText");

                if (!obtainedAnchor) {
                    normalAnchor = _playerNameText.rectTransform.anchoredPosition;
                    obtainedAnchor = true;
                }

                if (_scoresaberLeaderboardViewController.isOST) {
                    _playerNameText.richText = false;
                    _playerNameText.rectTransform.anchoredPosition = normalAnchor;
                    tableCell.showSeparator = row != __instance._scores.Count - 1;
                    tableCell.interactable = false;
                } else {
                    _playerNameText.richText = true;
                    Vector2 newPosition = new Vector2(normalAnchor.x + 2.5f, 0f);
                    _playerNameText.rectTransform.anchoredPosition = newPosition;
                    tableCell.showSeparator = true;
                    tableCell.interactable = true;
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
            }
            if (Plugin.Settings.enableCountryLeaderboards) {
                UpdateScopeControl(____friendsLeaderboardIcon, ____globalLeaderboardIcon, ____aroundPlayerLeaderboardIcon, ____scopeSegmentedControl);
                ____scopeSegmentedControl.SelectCellWithNumber(_lastScopeIndex);
            }
        }

        internal void UpdateScopeControl(Sprite ____friendsLeaderboardIcon, Sprite ____globalLeaderboardIcon, Sprite ____aroundPlayerLeaderboardIcon, IconSegmentedControl ____scopeSegmentedControl) {

            Texture2D countryTexture = new Texture2D(64, 64);
            countryTexture.LoadImage(Utilities.GetResource(Assembly.GetExecutingAssembly(), "ScoreSaber.Resources.country.png"));
            countryTexture.Apply();

            Sprite _countryIcon = Sprite.Create(countryTexture, new Rect(0, 0, countryTexture.width, countryTexture.height), Vector2.zero);
            ____scopeSegmentedControl.SetData(new DataItem[] {
                    new DataItem(____globalLeaderboardIcon, "Global"),
                    new DataItem(____aroundPlayerLeaderboardIcon, "Around You"),
                    new DataItem(____friendsLeaderboardIcon, "Friends"),
                    Plugin.Settings.locationFilterMode.ToLower() == "country" ? new DataItem(_countryIcon, "Country") : Plugin.Settings.locationFilterMode.ToLower() == "region" ? new DataItem(_countryIcon, "Region") : new DataItem(_countryIcon, "Country")
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

            private Color origColour = new Color(1, 1, 1, 1);
            private Color origColour0 = new Color(1, 1, 1, 0.2509804f);
            private Color origColour1 = new Color(1, 1, 1, 0);

            private void Start() {
                originalScale = seperator.transform.localScale;
            }

            public void OnPointerClick(PointerEventData data) {
                onClick(index);
            }

            public void OnPointerEnter(PointerEventData eventData) {
                if (!isScaled) {
                    seperator.transform.localScale = originalScale * 1.8f;
                    isScaled = true;
                }

                Color targetColor = Color.white;
                Color targetColor0 = Color.white;
                Color targetColor1 = new Color(1, 1, 1, 0);

                float lerpDuration = 0.15f;

                StopAllCoroutines();
                StartCoroutine(LerpColors(seperator, seperator.color, targetColor, seperator.color0, targetColor0, seperator.color1, targetColor1, lerpDuration));
            }

            public void OnPointerExit(PointerEventData eventData) {
                if (isScaled) {
                    seperator.transform.localScale = originalScale;
                    isScaled = false;
                }

                float lerpDuration = 0.05f;

                StopAllCoroutines();
                StartCoroutine(LerpColors(seperator, seperator.color, origColour, seperator.color0, origColour0, seperator.color1, origColour1, lerpDuration));
            }


            private IEnumerator LerpColors(ImageView target, Color startColor, Color endColor, Color startColor0, Color endColor0, Color startColor1, Color endColor1, float duration) {
                float elapsedTime = 0f;
                while (elapsedTime < duration) {
                    float t = elapsedTime / duration;
                    target.color = Color.Lerp(startColor, endColor, t);
                    target.color0 = Color.Lerp(startColor0, endColor0, t);
                    target.color1 = Color.Lerp(startColor1, endColor1, t);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                target.color = endColor;
                target.color0 = endColor0;
                target.color1 = endColor1;
            }

            private void OnDestroy() {
                StopAllCoroutines();
                onClick = null;
                seperator.color = origColour;
                seperator.color0 = origColour0;
                seperator.color1 = origColour1;
            }
        }

    }
}