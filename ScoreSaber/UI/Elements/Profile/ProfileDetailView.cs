#region

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Services;
using ScoreSaber.Core.Utils;
using ScoreSaber.UI.Leaderboard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

namespace ScoreSaber.UI.Elements.Profile {

    internal class ProfileDetailView : MonoBehaviour, INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        #region BSML Components
        [UIComponent("profile-modal-root")]
        public ModalView profileModalRoot;

        [UIComponent("badge-grid")]
        protected readonly GridLayoutGroup _badgeGrid = null;

        [UIComponent("badge-prefab")]
        protected readonly RectTransform _badgePrefab = null;

        [UIComponent("profile-top")]
        protected ImageView _profileTop;

        [UIComponent("profile-line-border")]
        protected ImageView _profileLineBorder;

        [UIComponent("profile-picture")]
        public readonly ImageView profilePicture = null;

        [UIComponent("profile-prefix-picture")]
        protected readonly ImageView _profilePrefixPicture = null;
        public string ProfilePrefixPicture {
            set {
                if (value == null) {
                    _profilePrefixPicture.gameObject.SetActive(false);
                    return;
                }
                _profilePrefixPicture.gameObject.SetActive(true);
                _profilePrefixPicture.SetImage(value);
            }
        }

        [UIComponent("player-name-text")]
        public readonly CurvedTextMeshPro playerNameText = null;

        [UIComponent("rank-text")]
        public readonly CurvedTextMeshPro rankText = null;

        [UIComponent("pp-text")]
        public readonly CurvedTextMeshPro ppText = null;

        [UIComponent("ranked-acc-text")]
        public readonly CurvedTextMeshPro rankedAccText = null;

        [UIComponent("total-score-text")]
        public readonly CurvedTextMeshPro totalScoreText = null;
        #endregion

        #region BSML Values
        [UIValue("badge-host-list")]
        protected List<object> badgeList = new List<object>();

        private bool _profileSet;
        [UIValue("profile-set")]
        public bool ProfileSet {
            get => _profileSet;
            set {
                _profileSet = value;
                NotifyPropertyChanged();
            }
        }
        private bool _profileSetLoading;
        [UIValue("profile-set-loading")]
        public bool ProfileSetLoading {
            get => _profileSetLoading;
            set {
                _profileSetLoading = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Custom Properties
        private PlayerInfo PlayerInfo { get; set; }

        private readonly HoverHint _profileHoverHint = null;
        private HoverHint ProfileHoverHint {
            get {
                if (_profileHoverHint == null) {
                    return _profilePrefixPicture.gameObject.GetComponent<HoverHint>();
                }
                return _profileHoverHint;
            }
        }
        #endregion

        private PlayerService _playerService;

#pragma warning disable IDE0051 // Remove unused private members
        [Inject]
        private void Construct(PlayerService playerService) {

            _playerService = playerService;
        }

        [UIAction("profile-url-click")]
        private void ProfileURLClicked() {

            Application.OpenURL($"https://scoresaber.com/u/{PlayerInfo.Id}");
        }
#pragma warning restore IDE0051 // Remove unused private members

        [UIAction("#post-parse")]
        protected void Parsed() {
            // background stuff
            _profileTop.material = Utilities.ImageResources.NoGlowMat;
            var background = profileModalRoot.gameObject.transform.GetChild(0);
            background.gameObject.SetActive(false);

            var modalPic = profilePicture;
            PanelView.ImageSkew(ref modalPic) = 0f;
            PanelView.ImageSkew(ref _profileLineBorder) = 0f;
            PanelView.ImageSkew(ref _profileTop) = 0f;

            modalPic.material = Plugin.NoGlowMatRound;
        }

        protected void Awake() {

            badgeList = new List<object>();
            for (int c = 0; c < 12; c++) {
                badgeList.Add(new BadgeCell());
            }
        }

        internal async Task ShowProfile(string playerId) {

            SetCrowns("0");
            SetLoadingState(true);

            PlayerInfo = await _playerService.GetPlayerInfo(playerId, full: true);

            playerNameText.text = PlayerInfo.Name;
            profilePicture.SetImage(PlayerInfo.ProfilePicture);

            rankText.text = $"#{PlayerInfo.Rank:n0}";
            ppText.text = $"<color=#6772E5>{PlayerInfo.PP:n0}pp</color>";

            rankedAccText.text = $"{Math.Round(PlayerInfo.ScoreStats.AverageRankedAccuracy, 2)}%";
            totalScoreText.text = $"{PlayerInfo.ScoreStats.TotalScore:n0}";

            SetProfileBadges(PlayerInfo.Badges
                .Select(badge => new Tuple<string, string>(badge.Image, badge.Description)).ToArray());
            SetCrowns(playerId);
            SetLoadingState(false);
        }

        public void SetProfileBadges(Tuple<string, string>[] imageNameGroup) {

            if (imageNameGroup == null || imageNameGroup.Length == 0) {
                _badgeGrid.gameObject.SetActive(false);
                return;
            }
            _badgeGrid.gameObject.SetActive(true);
            int c = 0;
            while (c < imageNameGroup.Length && c < badgeList.Count) {
                var cell = badgeList[c] as BadgeCell;
                cell.SetData(imageNameGroup[c].Item1, imageNameGroup[c].Item2);
                cell.SetActive(true);
                c++;
            }
            for (int i = c; i < badgeList.Count; i++) {
                (badgeList[i] as BadgeCell).SetActive(false);
            }
        }

        public void SetLoadingState(bool loading) {

            ProfileSet = !loading;
            ProfileSetLoading = loading;
        }

        private void SetCrowns(string playerId) {

            ProfilePrefixPicture = null;
            ProfileHoverHint.enabled = false;
            Tuple<string, string> crownDetails = LeaderboardUtils.GetCrownDetails(playerId);

            if (!string.IsNullOrEmpty(crownDetails.Item1)) {
                ProfileHoverHint.enabled = true;
                ProfilePrefixPicture = crownDetails.Item1;
                ProfileHoverHint.text = crownDetails.Item2;
            }
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}