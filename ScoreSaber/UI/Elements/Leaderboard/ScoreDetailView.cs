#region

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using ScoreSaber.Core.Data.Internal;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Data.Wrappers;
using ScoreSaber.Core.Utils;
using ScoreSaber.Extensions;
using System;
using UnityEngine.UI;

#endregion

namespace ScoreSaber.UI.Elements.Leaderboard {
    internal class ScoreDetailView {

        #region BSML Components
        [UIComponent("detail-modal-root")]
        public ModalView detailModalRoot = null;
        [UIComponent("prefix-text")]
        protected readonly CurvedTextMeshPro _prefixText = null;
        [UIComponent("name-text")]
        protected readonly CurvedTextMeshPro _nameText = null;
        [UIComponent("device-text")]
        protected readonly CurvedTextMeshPro _deviceText = null;
        [UIComponent("score-text")]
        protected readonly CurvedTextMeshPro _scoreText = null;
        [UIComponent("pp-text")]
        protected readonly CurvedTextMeshPro _ppText = null;
        [UIComponent("max-combo-text")]
        protected readonly CurvedTextMeshPro _maxComboText = null;
        [UIComponent("full-combo-text")]
        protected readonly CurvedTextMeshPro _fullComboText = null;
        [UIComponent("bad-cuts-text")]
        protected readonly CurvedTextMeshPro _badCutsText = null;
        [UIComponent("missed-notes-text")]
        protected readonly CurvedTextMeshPro _missedNotesText = null;
        [UIComponent("modifiers-text")]
        protected readonly CurvedTextMeshPro _modifiersText = null;
        [UIComponent("time-text")]
        protected readonly CurvedTextMeshPro _timeText = null;

        [UIComponent("prefix-image")]
        private readonly ImageView _scoreInfoPrefixPicture = null;
        public string ScoreInfoPrefixPicture {
            set {
                if (value == null) {
                    _scoreInfoPrefixPicture.gameObject.SetActive(false);
                    return;
                }
                _scoreInfoPrefixPicture.gameObject.SetActive(true);
                _scoreInfoPrefixPicture.SetImage(value);
            }
        }

        private readonly HoverHint _scoreInfoHoverHint = null;
        public HoverHint ScoreInfoHoverHint {
            get {
                if (_scoreInfoHoverHint == null) {
                    return _scoreInfoPrefixPicture.gameObject.GetComponent<HoverHint>();
                }
                return _scoreInfoHoverHint;
            }
        }

        [UIComponent("watch-replay-button")]
        protected readonly Button _watchReplayButton = null;
        [UIComponent("show-profile-button")]
        protected readonly Button _showProfileButton = null;
        [UIAction("show-profile-click")] private void ShowProfileClicked() => ShowProfile?.Invoke(CurrentScore.Score.LeaderboardPlayerInfo.Id);
        [UIAction("replay-click")] private void ReplayClicked() => ReplayStart();
        #endregion

        public event Action<string> ShowProfile;
        public event Action<ScoreMap> StartReplay;

        private bool AllowReplayWatching { get; set; } = true;

        private ScoreMap CurrentScore { get; set; }

        [UIAction("#post-parse")]
        public void Parsed() {
            _nameText.fontSizeMin = 2.5f;
            _nameText.fontSizeMax = 4.0f;
            _nameText.enableAutoSizing = true;
            _watchReplayButton.transform.localScale *= .4f;
            _showProfileButton.transform.localScale *= .4f;
        }

        public void SetScoreInfo(ScoreMap scoreMap, bool replayDownloading) {

            CurrentScore = scoreMap;
            Score score = scoreMap.Score;
            SetCrowns(score.LeaderboardPlayerInfo.Id);
            _nameText.text = $"{score.LeaderboardPlayerInfo.Name}'s score";
            _deviceText.SetFancyText("Device", HMD.GetFriendlyName(score.Hmd));
            _scoreText.SetFancyText("Score", $"{string.Format("{0:n0}", score.ModifiedScore)} (<color=#FFD42A>{scoreMap.Accuracy}%</color>)");
            _ppText.SetFancyText("Performance Points", $"<color=#6772E5>{score.PP}pp</color>");
            _maxComboText.SetFancyText("Combo", score.MaxCombo != 0 ? score.MaxCombo.ToString() : "N/A");
            _fullComboText.SetFancyText("Full Combo", score.MaxCombo != 0 ? score.FullCombo ? "<color=#9EDBB1>Yes</color>" : "<color=#FF0000>No</color>" : "N/A");
            _badCutsText.SetFancyText("Bad Cuts", score.MaxCombo != 0 ? score.BadCuts > 0 ? $"<color=#FF0000>{score.BadCuts}</color>" : score.BadCuts.ToString() : "N/A");
            _missedNotesText.SetFancyText("Missed Notes", score.MaxCombo != 0 ? score.MissedNotes > 0 ? $"<color=#FF0000>{score.MissedNotes}</color>" : score.MissedNotes.ToString() : "N/A");
            _modifiersText.SetFancyText("Modifiers", score.Modifiers);
            _timeText.SetFancyText("Time Set", new TimeSpan(DateTime.UtcNow.Ticks - score.TimeSet.Ticks).ToNaturalTime(2, true) + " ago");

            if (score.MaxCombo == 0) { _fullComboText.text = "N/A"; }
            if (!replayDownloading) {
                SetButtonState(_watchReplayButton, score.HasReplay && AllowReplayWatching);
            }
        }

        private void SetCrowns(string playerId) {

            ScoreInfoPrefixPicture = null;
            ScoreInfoHoverHint.enabled = false;
            Tuple<string, string> crownDetails = LeaderboardUtils.GetCrownDetails(playerId);

            if (!string.IsNullOrEmpty(crownDetails.Item1)) {
                ScoreInfoHoverHint.enabled = true;
                ScoreInfoPrefixPicture = crownDetails.Item1;
                ScoreInfoHoverHint.text = crownDetails.Item2;
            }
        }

        private void ReplayStart() {

            _watchReplayButton.interactable = false;
            StartReplay?.Invoke(CurrentScore);
        }

        public void AllowWatchingReplay(bool value) {

            AllowReplayWatching = value;
            SetButtonState(_watchReplayButton, value);
        }

        private void SetButtonState(Button button, bool value) {

            if (button != null) {
                button.interactable = value;
                button.gameObject.GetComponent<HoverHint>().enabled = value;
            }
        }
    }
}