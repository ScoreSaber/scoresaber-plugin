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
        private PlayerService _playerService;

        protected void Awake() {
            badgeList = new List<object>();
            for (int c = 0; c < 12; c++) {
                badgeList.Add(new BadgeCell());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [Inject]
        private void Construct(PlayerService playerService) {
            _playerService = playerService;
        }

        [UIAction("profile-url-click")]
        private void ProfileURLClicked() {
            Application.OpenURL($"https://scoresaber.com/u/{_playerInfo.id}");
        }

        [UIAction("#post-parse")]
        protected void Parsed() {
            // background stuff
            _profileTop.material = Utilities.ImageResources.NoGlowMat;
            Transform background = profileModalRoot.gameObject.transform.GetChild(0);
            background.gameObject.SetActive(false);

            ImageView modalPic = profilePicture;
            PanelView.ImageSkew(ref modalPic) = 0f;
            PanelView.ImageSkew(ref _profileLineBorder) = 0f;
            PanelView.ImageSkew(ref _profileTop) = 0f;

            modalPic.material = Plugin.NoGlowMatRound;
        }

        internal async Task ShowProfile(string playerId) {
            SetCrowns("0");
            SetLoadingState(true);

            _playerInfo = await _playerService.GetPlayerInfo(playerId, true);

            await CheckCyanOrWimmiuls();

            playerNameText.text = _playerInfo.name;
            profilePicture.SetImage(_playerInfo.profilePicture);

            rankText.text = $"#{_playerInfo.rank:n0}";
            ppText.text = $"<color=#6772E5>{_playerInfo.pp:n0}pp</color>";

            rankedAccText.text = $"{Math.Round(_playerInfo.scoreStats.averageRankedAccuracy, 2)}%";
            totalScoreText.text = $"{_playerInfo.scoreStats.totalScore:n0}";

            SetProfileBadges(_playerInfo.badges
                .Select(badge => new Tuple<string, string>(badge.image, badge.description)).ToArray());
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
                BadgeCell cell = badgeList[c] as BadgeCell;
                cell.SetData(imageNameGroup[c].Item1, imageNameGroup[c].Item2);
                cell.SetActive(true);
                c++;
            }

            for (int i = c; i < badgeList.Count; i++) {
                (badgeList[i] as BadgeCell)?.SetActive(false);
            }
        }

        public void SetLoadingState(bool loading) {
            profileSet = !loading;
            profileSetLoading = loading;
        }

        private async Task CheckCyanOrWimmiuls() {
            switch (_playerInfo.id) {
                case PlayerIDs.CyanSnow: {
                    Material mat = await Plugin.GetFurryMaterial();
                    playerNameText.fontMaterial = mat;
                    _isCyan = true;
                    return;
                }
                case PlayerIDs.Williums:
                    playerNameText.text =
                        "<color=#FF0000>w</color><color=#FF7F00>i</color><color=#FFFF00>l</color><color=#00FF00>l</color><color=#0000FF>i</color><color=#4B0082>u</color><color=#8B00FF>m</color><color=#FF0000>s</color>";
                    break;
            }

            switch (_isCyan) {
                case true:
                    playerNameText.fontMaterial = Plugin.NonFurry;
                    break;
            }
        }

        private void SetCrowns(string playerId) {
            profilePrefixPicture = null;
            profileHoverHint.enabled = false;
            Tuple<string, string> crownDetails = LeaderboardUtils.GetCrownDetails(playerId);

            switch (string.IsNullOrEmpty(crownDetails.Item1)) {
                case false:
                    profileHoverHint.enabled = true;
                    profilePrefixPicture = crownDetails.Item1;
                    profileHoverHint.text = crownDetails.Item2;
                    break;
            }
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region BSML Components

        [UIComponent("profile-modal-root")] public ModalView profileModalRoot;

        [UIComponent("badge-grid")] protected readonly GridLayoutGroup _badgeGrid = null;

        [UIComponent("badge-prefab")] protected readonly RectTransform _badgePrefab = null;

        [UIComponent("profile-top")] protected ImageView _profileTop;

        [UIComponent("profile-line-border")] protected ImageView _profileLineBorder;

        [UIComponent("profile-picture")] public readonly ImageView profilePicture = null;

        [UIComponent("profile-prefix-picture")]
        protected readonly ImageView _profilePrefixPicture = null;

        public string profilePrefixPicture {
            set {
                switch (value) {
                    case null:
                        _profilePrefixPicture.gameObject.SetActive(false);
                        return;
                    default:
                        _profilePrefixPicture.gameObject.SetActive(true);
                        _profilePrefixPicture.SetImage(value);
                        break;
                }
            }
        }

        [UIComponent("player-name-text")] public readonly CurvedTextMeshPro playerNameText = null;

        [UIComponent("rank-text")] public readonly CurvedTextMeshPro rankText = null;

        [UIComponent("pp-text")] public readonly CurvedTextMeshPro ppText = null;

        [UIComponent("ranked-acc-text")] public readonly CurvedTextMeshPro rankedAccText = null;

        [UIComponent("total-score-text")] public readonly CurvedTextMeshPro totalScoreText = null;

        #endregion

        #region BSML Values

        [UIValue("badge-host-list")] protected List<object> badgeList = new List<object>();

        private bool _profileSet;

        [UIValue("profile-set")]
        public bool profileSet {
            get => _profileSet;
            set {
                _profileSet = value;
                NotifyPropertyChanged();
            }
        }

        private bool _profileSetLoading;

        [UIValue("profile-set-loading")]
        public bool profileSetLoading {
            get => _profileSetLoading;
            set {
                _profileSetLoading = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Custom Properties

        private PlayerInfo _playerInfo { get; set; }
        private bool _isCyan { get; set; }

        private readonly HoverHint _profileHoverHint = null;

        private HoverHint profileHoverHint => _profileHoverHint == null
            ? _profilePrefixPicture.gameObject.GetComponent<HoverHint>()
            : _profileHoverHint;

        #endregion
    }
}