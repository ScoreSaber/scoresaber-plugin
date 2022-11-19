#region

using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.UI.Legacy {
    internal class GameReplayUI : MonoBehaviour {
        [Inject] private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData = null;

        public void Start() {
            CreateReplayUI();
        }

        private void CreateReplayUI() {
            string replayText =
                $"REPLAY MODE - Watching {Plugin.ReplayState.CurrentPlayerName} play {Plugin.ReplayState.CurrentLevel.level.songAuthorName} - {Plugin.ReplayState.CurrentLevel.level.songName} ({Enum.GetName(typeof(BeatmapDifficulty), Plugin.ReplayState.CurrentLevel.difficulty).Replace("ExpertPlus", "Expert+")})";
            float timeScale = 1f;

            switch (Plugin.ReplayState.IsLegacyReplay) {
                case false: {
                    switch (Plugin.ReplayState.LoadedReplayFile.noteKeyframes.Count > 0) {
                        case true:
                            timeScale = Plugin.ReplayState.LoadedReplayFile.noteKeyframes[0].TimeSyncTimescale;
                            break;
                    }

                    break;
                }
            }

            if (timeScale != 1f) {
                replayText += $" [{timeScale:P1}]";
            }

            string friendlyMods = GetFriendlyModifiers(Plugin.ReplayState.CurrentModifiers);
            if (friendlyMods != string.Empty) {
                replayText += $" [{friendlyMods}]";
            }

            GameObject _watermarkCanvas = new GameObject("InGameReplayUI");

            switch (_gameplayCoreSceneSetupData.environmentInfo.environmentName) {
                case "Interscope":
                    _watermarkCanvas.transform.position = new Vector3(0f, 3.5f, 12.0f);
                    break;
                default:
                    _watermarkCanvas.transform.position = new Vector3(0f, 4f, 12.0f);
                    break;
            }

            _watermarkCanvas.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);

            Canvas _canvas = _watermarkCanvas.AddComponent<Canvas>();
            _watermarkCanvas.AddComponent<CurvedCanvasSettings>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.enabled = false;
            TMP_Text _text = CreateText(_canvas.transform as RectTransform, replayText, new Vector2(0, 10),
                new Vector2(100, 20), 15f);
            _text.alignment = TextAlignmentOptions.Center;
            RectTransform rectTransform = _text.transform as RectTransform;
            rectTransform.SetParent(_canvas.transform, false);
            _canvas.enabled = true;
        }

        public TextMeshProUGUI CreateText(RectTransform parent, string text, Vector2 anchoredPosition,
            Vector2 sizeDelta, float fontSize) {
            GameObject gameObject = new GameObject("CustomUIText-ScoreSaber");
            gameObject.SetActive(false);
            TextMeshProUGUI textMeshProUGUI = gameObject.AddComponent<TextMeshProUGUI>();
            textMeshProUGUI.font = Instantiate(Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
                .First(t => t.name == "Teko-Medium SDF"));
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
            switch (gameplayModifiers) {
                case null:
                    return string.Empty;
            }

            List<string> result = new List<string>();
            switch (gameplayModifiers.energyType) {
                case GameplayModifiers.EnergyType.Battery:
                    result.Add("BE");
                    break;
            }

            switch (gameplayModifiers.noFailOn0Energy) {
                case true:
                    result.Add("NF");
                    break;
            }

            switch (gameplayModifiers.instaFail) {
                case true:
                    result.Add("IF");
                    break;
            }

            switch (gameplayModifiers.failOnSaberClash) {
                case true:
                    result.Add("SC");
                    break;
            }

            switch (gameplayModifiers.enabledObstacleType) {
                case GameplayModifiers.EnabledObstacleType.NoObstacles:
                    result.Add("NO");
                    break;
            }

            switch (gameplayModifiers.noBombs) {
                case true:
                    result.Add("NB");
                    break;
            }

            switch (gameplayModifiers.strictAngles) {
                case true:
                    result.Add("SA");
                    break;
            }

            switch (gameplayModifiers.disappearingArrows) {
                case true:
                    result.Add("DA");
                    break;
            }

            switch (gameplayModifiers.ghostNotes) {
                case true:
                    result.Add("GN");
                    break;
            }

            switch (gameplayModifiers.songSpeed) {
                case GameplayModifiers.SongSpeed.Slower:
                    result.Add("SS");
                    break;
                case GameplayModifiers.SongSpeed.Faster:
                    result.Add("FS");
                    break;
                case GameplayModifiers.SongSpeed.SuperFast:
                    result.Add("SF");
                    break;
            }

            switch (gameplayModifiers.smallCubes) {
                case true:
                    result.Add("SC");
                    break;
            }

            switch (gameplayModifiers.strictAngles) {
                case true:
                    result.Add("SA");
                    break;
            }

            switch (gameplayModifiers.proMode) {
                case true:
                    result.Add("PM");
                    break;
            }

            switch (gameplayModifiers.noArrows) {
                case true:
                    result.Add("NA");
                    break;
            }

            return string.Join(",", result);
        }
    }
}