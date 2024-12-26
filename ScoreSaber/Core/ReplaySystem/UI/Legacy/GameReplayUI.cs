using HMUI;
using ScoreSaber.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using Zenject;

namespace ScoreSaber.Core.ReplaySystem.Legacy.UI
{
    internal class GameReplayUI : MonoBehaviour {

        [Inject] private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData = null;

        public void Start() {
            if(Plugin.Settings.hideWatermarkIfUsersReplay && Plugin.ReplayState.isUsersReplay) {
                return;
            }
            CreateReplayUI();
        }

        private void CreateReplayUI() {

            string replayText = string.Format("<color=yellow>REPLAY</color>   <i>{0}</i> playing ({1} - {2}) [<color=#59cf59>{3}</color>]", Plugin.ReplayState.CurrentPlayerName, 
                Plugin.ReplayState.CurrentBeatmapLevel.songAuthorName, Plugin.ReplayState.CurrentBeatmapLevel.songName, BeatmapUtils.FriendlyLevelAuthorName(Plugin.ReplayState.CurrentBeatmapLevel.allMappers, Plugin.ReplayState.CurrentBeatmapLevel.allLighters));
            float timeScale = 1f;

            if (!Plugin.ReplayState.IsLegacyReplay) {
                if (Plugin.ReplayState.LoadedReplayFile.noteKeyframes.Count > 0) {
                    timeScale = Plugin.ReplayState.LoadedReplayFile.noteKeyframes[0].TimeSyncTimescale;
                }
            }
            if (timeScale != 1f) {
                replayText += $" [{timeScale:P1}]";
            }
            string friendlyMods = GetFriendlyModifiers(Plugin.ReplayState.CurrentModifiers);
            if (friendlyMods != string.Empty) {
                replayText += string.Format(" <color=grey>[{0}]</color>", friendlyMods);
            }
            GameObject _watermarkCanvas = new GameObject("InGameReplayUI");

            if (_gameplayCoreSceneSetupData.targetEnvironmentInfo.environmentName == "Interscope") {
                _watermarkCanvas.transform.position = new Vector3(0f, 3.5f, 12.0f);
            } else {
                _watermarkCanvas.transform.position = new Vector3(0f, 4f, 12.0f);
            }
            _watermarkCanvas.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);

            Canvas _canvas = _watermarkCanvas.AddComponent<Canvas>();
            _watermarkCanvas.AddComponent<CurvedCanvasSettings>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.enabled = false;
            TMP_Text _text = CreateText(_canvas.transform as RectTransform, replayText, new Vector2(0, 10), new Vector2(100, 20), 15f);
            _text.alignment = TextAlignmentOptions.Center;
            var rectTransform = _text.transform as RectTransform;
            rectTransform.SetParent(_canvas.transform, false);
            _canvas.enabled = true;
        }

        public TextMeshProUGUI CreateText(RectTransform parent, string text, Vector2 anchoredPosition, Vector2 sizeDelta, float fontSize) {

            GameObject gameObject = new GameObject("CustomUIText-ScoreSaber");
            gameObject.SetActive(false);
            TextMeshProUGUI textMeshProUGUI = gameObject.AddComponent<TextMeshProUGUI>();
            textMeshProUGUI.font = Instantiate(Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First((TMP_FontAsset t) => t.name == "Teko-Medium SDF"));
            textMeshProUGUI.rectTransform.SetParent(parent, false);
            textMeshProUGUI.text = text;
            textMeshProUGUI.fontSize = fontSize;
            textMeshProUGUI.color = Color.white;
            textMeshProUGUI.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            textMeshProUGUI.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            textMeshProUGUI.rectTransform.sizeDelta = sizeDelta;
            textMeshProUGUI.rectTransform.anchoredPosition = anchoredPosition;
            gameObject.SetActive(true);
            return textMeshProUGUI;
        }



        public string GetFriendlyModifiers(GameplayModifiers gameplayModifiers) {

            if (gameplayModifiers == null) return string.Empty;

            List<string> result = new List<string>();
            if (gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery) {
                result.Add("BE");
            }
            if (gameplayModifiers.noFailOn0Energy) {
                result.Add("NF");
            }
            if (gameplayModifiers.instaFail) {
                result.Add("IF");
            }
            if (gameplayModifiers.failOnSaberClash) {
                result.Add("SC");
            }
            if (gameplayModifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles) {
                result.Add("NO");
            }
            if (gameplayModifiers.noBombs) {
                result.Add("NB");
            }
            if (gameplayModifiers.strictAngles) {
                result.Add("SA");
            }
            if (gameplayModifiers.disappearingArrows) {
                result.Add("DA");
            }
            if (gameplayModifiers.ghostNotes) {
                result.Add("GN");
            }
            if (gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Slower) {
                result.Add("SS");
            }
            if (gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Faster) {
                result.Add("FS");
            }
            if (gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.SuperFast) {
                result.Add("SF");
            }
            if (gameplayModifiers.smallCubes) {
                result.Add("SC");
            }
            if (gameplayModifiers.proMode) {
                result.Add("PM");
            }
            if (gameplayModifiers.noArrows) {
                result.Add("NA");
            }
            return string.Join(",", result);
        }

    }
}
