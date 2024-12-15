using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Config.Data;
using IPA.Utilities;
using IPA.Utilities.Async;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Http;
using ScoreSaber.Core.Http.Configuration;
using ScoreSaber.Core.Http.Endpoints.CDN;
using ScoreSaber.Core.Http.Endpoints.Web;
using ScoreSaber.Core.Services;
using ScoreSaber.Core.Utils;
using ScoreSaber.Extensions;
using ScoreSaber.UI.Elements.Leaderboard;
using ScoreSaber.UI.Leaderboard;
using ScoreSaber.UI.Other;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static BeatSaberMarkupLanguage.Components.KEYBOARD;
using static IPA.Logging.Logger;

namespace ScoreSaber.UI.Elements.Profile {

    internal class ProfileDetailView : MonoBehaviour, INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        private const string OnlineResource = "ScoreSaber.Resources.Online.png";
        private const string OfflineResource = "ScoreSaber.Resources.Offline.png";

        #region BSML Components
        [UIParams]
        private BSMLParserParams _parserParams = null;

        [UIComponent("profile-modal-root")]
        public ModalView profileModalRoot = null;

        [UIComponent("badge-grid")]
        protected readonly GridLayoutGroup _badgeGrid = null;

        [UIComponent("badge-prefab")]
        protected readonly RectTransform _badgePrefab = null;

        [UIComponent("profile-top")]
        protected ImageView _profileTop = null;

        [UIComponent("profile-line-border")]
        protected ImageView _profileLineBorder = null;

        [UIComponent("profile-picture")]
        public readonly ImageView profilePicture = null;

        [UIComponent("profile-prefix-picture")]
        protected readonly ImageView _profilePrefixPicture = null;
        public string profilePrefixPicture {
            set {
                if (value == null) {
                    _profilePrefixPicture.gameObject.SetActive(false);
                    return;
                }
                if (value.Contains("Online")) {
                    _profilePrefixPicture.gameObject.SetActive(true);
                    _profilePrefixPicture.sprite = onlineSprite;
                    return;
                }
                if (value.Contains("Offline")) {
                    _profilePrefixPicture.gameObject.SetActive(true);
                    _profilePrefixPicture.sprite = offlineSprite;
                    return;
                }

                _profilePrefixPicture.gameObject.SetActive(true);
                _profilePrefixPicture.SetImageAsync(value).RunTask();
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

        [UIComponent("map-presence")]
        public readonly HorizontalLayoutGroup mapPresence = null;

        [UIComponent("map-presence-title")]
        public readonly VerticalLayoutGroup mapPresenceTitle = null;

        [UIComponent("map-image-presence")]
        public readonly ImageView mapImagePresence = null;

        #endregion

        #region BSML Values
        [UIValue("badge-host-list")]
        protected List<object> badgeList = new List<object>();

        private bool _profileSet = false;
        [UIValue("profile-set")]
        public bool profileSet {
            get => _profileSet;
            set {
                _profileSet = value;
                NotifyPropertyChanged();
            }
        }
        private bool _profileSetLoading = false;
        [UIValue("profile-set-loading")]
        public bool profileSetLoading {
            get => _profileSetLoading;
            set {
                _profileSetLoading = value;
                NotifyPropertyChanged();
            }
        }

        private string _mapName = "";
        [UIValue("map-name")]
        public string mapName {
            get => _mapName;
            set {
                _mapName = value;
                NotifyPropertyChanged();
            }
        }

        private string _mapArtist = "";
        [UIValue("map-artist")]
        public string mapArtist {
            get => _mapArtist;
            set {
                _mapArtist = value;
                NotifyPropertyChanged();
            }
        }

        private string _mapMapperName = "";
        [UIValue("map-mapper-name")]
        public string MapMapperName {
            get => _mapMapperName;
            set {
                _mapMapperName = value;
                NotifyPropertyChanged();
            }
        }

        private string _mapTime = "";
        [UIValue("map-time")]
        public string mapTime {
            get => _mapTime;
            set {
                _mapTime = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Custom Properties
        private PlayerInfo _playerInfo { get; set; }
        private RichPresenceResponse _richPresence { get; set; }
        private bool _isCyan { get; set; }

        private readonly HoverHint _profileHoverHint = null;
        private HoverHint profileHoverHint {
            get {
                if (_profileHoverHint == null) {
                    return _profilePrefixPicture.gameObject.GetComponent<HoverHint>();
                }
                return _profileHoverHint;
            }
        }

        private bool isDownloadingMap { get; set; } = false;
        #endregion

        private PlayerService _playerService = null;
        private BeatmapLevelsModel _beatmapLevelsModel = null;
        private SoloFreePlayFlowCoordinator _soloFreePlayFlowCoordinator = null;
        private PanelView _panelView = null;
        private ScoreSaberLeaderboardViewController _scoreSaberLeaderboardViewController = null;

        private IScoreSaberBeatmapDownloader _beatmapDownloader => Plugin.Container.TryResolve<IScoreSaberBeatmapDownloader>();

        private Sprite _onlineSprite = null;
        private Sprite onlineSprite {
            get {
                return _onlineSprite;
            }
        }

        private Sprite _offlineSprite = null;
        private Sprite offlineSprite {
            get {
                return _offlineSprite;
            }
        }

        [Inject]
        private void Construct(PlayerService playerService, BeatmapLevelsModel beatmapLevelsModel, SoloFreePlayFlowCoordinator soloFreePlayFlowCoordinator, PanelView panelView, ScoreSaberLeaderboardViewController scoreSaberLeaderboardViewController) {
            _playerService = playerService;
            _beatmapLevelsModel = beatmapLevelsModel;
            _soloFreePlayFlowCoordinator = soloFreePlayFlowCoordinator;
            _panelView = panelView;
            _scoreSaberLeaderboardViewController = scoreSaberLeaderboardViewController;

            
        }

        [UIAction("profile-url-click")]
        private void ProfileURLClicked() {
            Application.OpenURL(new WebUser(_playerInfo.id).BuildUrl());
        }

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
            mapImagePresence.material = Plugin.NoGlowMatRound;
            mapPresence.gameObject.SetActive(false);
            if(_beatmapDownloader == null) {
                Plugin.Log.Error($"BeatmapDownloader is null, install a mod to [APP] that injects IScoreSaberBeatmapDownloader");
            }
            // load sprites
            UnityMainThreadTaskScheduler.Factory.StartNew(() => BeatSaberMarkupLanguage.Utilities.LoadSpriteFromAssemblyAsync(OnlineResource)).ContinueWith(x => { _onlineSprite = x.Result.Result; });
            UnityMainThreadTaskScheduler.Factory.StartNew(() => BeatSaberMarkupLanguage.Utilities.LoadSpriteFromAssemblyAsync(OfflineResource)).ContinueWith(x => { _offlineSprite = x.Result.Result; });
        }

        protected void Awake() {

            badgeList = new List<object>();
            for (int c = 0; c < 12; c++) {
                badgeList.Add(new BadgeCell());
            }
        }

        internal async Task ShowProfile(string playerId) {
            mapPresence.gameObject.SetActive(false);
            mapPresenceTitle.gameObject.SetActive(false);
            SetCrowns("0");
            rankText.text = "Loading...";
            ppText.text = "Loading...";
            rankedAccText.text = "Loading...";
            totalScoreText.text = "Loading...";
            playerNameText.text = "Loading...";
            SetProfileBadges(null);

            SetLoadingState(true);
          
            _playerInfo = await _playerService.GetPlayerInfo(playerId, full: true);

            await CheckCyanOrWimmiuls();

            playerNameText.text = _playerInfo.name;
            if(SpriteCache.cachedSprites.TryGetValue(_playerInfo.profilePicture, out var sprite)) {
                profilePicture.sprite = sprite;
            } else {
                profilePicture.SetImageAsync(_playerInfo.profilePicture).RunTask();
            }

            rankText.text = $"#{string.Format("{0:n0}", _playerInfo.rank)} <color=grey><size=80%>(#{_playerInfo.countryRank} {_playerInfo.country})</size></color>";
            ppText.text = $"<color=#6772E5>{string.Format("{0:n0}", _playerInfo.pp)}pp</color>";

            rankedAccText.text = $"{Math.Round(_playerInfo.scoreStats.averageRankedAccuracy, 2)}%";
            totalScoreText.text = string.Format("{0:n0}", _playerInfo.scoreStats.totalScore);

            List<Tuple<string, string>> list = new List<Tuple<string, string>>();

            foreach (Badge badge in _playerInfo.badges) {
                list.Add(new Tuple<string, string>(badge.image, badge.description));
            }

            SetProfileBadges(list.ToArray());
            SetCrowns(playerId);
            SetLoadingState(false);
            if(_beatmapDownloader == null) {
                Plugin.Log.Error("BeatmapDownloader is null, install a mod that injects IScoreSaberBeatmapDownloader");
                return;
            }
            try {
                _richPresence = await _playerService.GetRichPresence(playerId);
                if(_richPresence.state.currentMap != null) {
                    mapName = _richPresence.state.currentMap.Name;
                    mapArtist = $"<color=grey><size=80%>{_richPresence.state.currentMap.Artist}</size></color>";
                    MapMapperName = $"[<color=#59cf59><size=80%>{_richPresence.state.currentMap.AuthorName}</size></color>]";
                    DateTimeOffset parsedTime = DateTimeOffset.Parse(_richPresence.state.currentMap.Timestamp);
                    DateTimeOffset now = DateTimeOffset.UtcNow;

                    TimeSpan difference = now - parsedTime;
                    mapTime = difference.ToNaturalTime(2, false) + " ago";
                    mapPresence.gameObject.SetActive(true);
                    mapPresenceTitle.gameObject.SetActive(true);
                    mapImagePresence.SetImageAsync(new SongCover(_richPresence.state.currentMap.Hash).BuildUrl()).RunTask();
                }
                Plugin.Log.Notice(_richPresence.state.Scene.ToString());
                if (_richPresence != null) {
                    SetRichStatus(_richPresence.state.Scene);
                }
            } catch(HttpRequestException ex) {
                Plugin.Log.Error(ex.Message);
            }
        }

        [UIAction("map-presence-play")]
        private void MapPresenceClicked() {
            _scoreSaberLeaderboardViewController.CloseModals();
            _parserParams.EmitEvent("close-modals");
            DownloadOrOpenMap(_richPresence.state.currentMap.Hash, _richPresence.state.currentMap.Difficulty).RunTask();
        }

        private async Task DownloadOrOpenMap(string hash, int diff) {
            if(hash.ToCharArray().Length != 40) {
                Plugin.Log.Info("Hash is not 40 characters long, Attempting to open DLC / OST Map");
                OpenOSTDLCMap(hash, diff);
                return;
            }
            if (_beatmapLevelsModel.GetBeatmapLevel("custom_level_" + hash) != null) {
                _ = UnityMainThreadTaskScheduler.Factory.StartNew(() => OpenMap(hash, diff));
            } else {
                if(_beatmapDownloader == null) {
                    Plugin.Log.Error("BeatmapDownloader is null, install a mod that injects IScoreSaberBeatmapDownloader");
                    return;
                }
                if (isDownloadingMap) {
                    return;
                }
                isDownloadingMap = true;
                _panelView.SetPromptInfo("Downloading map...", true);
                await _beatmapDownloader.DownloadBeatmapAsync(hash, new Action(() => { UnityMainThreadTaskScheduler.Factory.StartNew(async() => 
                {
                    _panelView.SetPromptSuccess("Downloaded map!", false, 5f);
                    _ = UnityMainThreadTaskScheduler.Factory.StartNew(() => SongCore.Loader.Instance.RefreshSongs(true));
                    while (SongCore.Loader.AreSongsLoading) {
                        await Task.Delay(200);
                    }
                    await Task.Delay(2000);
                    _ = UnityMainThreadTaskScheduler.Factory.StartNew(() => OpenMap(hash, diff));
                }); }));
            }
        }

        private void OpenMap(string hash, int diff) {
            Plugin.Log.Info($"Opening map {hash} with diff {diff}");
            hash = "custom_level_" + hash;
            var level = SongCore.Loader.BeatmapLevelsModelSO.GetBeatmapLevel(hash);
            if (level == null) {
                Plugin.Log.Error($"Level {hash} not found");
                return;
            }

            diff = (diff - 1) / 2;
            BeatmapKey key = GetBeatmapKey(level, diff);
            LevelSelectionFlowCoordinator.State state = new LevelSelectionFlowCoordinator.State(SelectLevelCategoryViewController.LevelCategory.All,
                                                                                                _beatmapLevelsModel.GetLevelPackForLevelId(hash),
                                                                                                in key,
                                                                                                level);

            var favouriteids = _soloFreePlayFlowCoordinator.levelSelectionNavigationController._levelCollectionNavigationController._levelCollectionViewController._levelCollectionTableView._favoriteLevelIds;
            var levelcollection = _soloFreePlayFlowCoordinator.levelSelectionNavigationController._levelCollectionNavigationController._levelCollectionViewController;
            levelcollection._levelCollectionTableView.SetData(new List<BeatmapLevel>() { level }, favouriteids, false, false);
            levelcollection.SelectLevel(level);
        }

        private void OpenOSTDLCMap(string id, int diff) {

            BeatmapLevel level = _beatmapLevelsModel.GetBeatmapLevel(id);

            diff = (diff - 1) / 2;
            BeatmapKey key = GetBeatmapKey(level, diff);
            LevelSelectionFlowCoordinator.State state = new LevelSelectionFlowCoordinator.State(SelectLevelCategoryViewController.LevelCategory.All,
                                                                                                SongCore.Loader.CustomLevelsPack,
                                                                                                in key,
                                                                                                level);

            _soloFreePlayFlowCoordinator.levelSelectionNavigationController._levelCollectionNavigationController._levelCollectionViewController._levelCollectionTableView.SetData(new List<BeatmapLevel>() { level }, _soloFreePlayFlowCoordinator.levelSelectionNavigationController._levelCollectionNavigationController._levelCollectionViewController._levelCollectionTableView._favoriteLevelIds, false, false);
            _soloFreePlayFlowCoordinator.levelSelectionNavigationController._levelCollectionNavigationController._levelCollectionViewController.SelectLevel(level);
        }

        private BeatmapKey GetBeatmapKey(BeatmapLevel level, int newdiff) {
            BeatmapKey key = level.GetBeatmapKeys().LastOrDefault(x => newdiff == (int)x.difficulty);
            if (key == null || key == default) {
                Plugin.Log.Error($"Difficulty {newdiff} not found for level {level.levelID}");
            }
            return key;
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
            profileSet = !loading;
            profileSetLoading = loading;
        }

        private async Task CheckCyanOrWimmiuls() {

            if (_playerInfo.id == PlayerIDs.CyanSnow) {
                var mat = await Plugin.GetFurryMaterial();
                playerNameText.fontMaterial = mat;
                _isCyan = true;
                return;
            }
            if (_playerInfo.id == PlayerIDs.Williums) {
                playerNameText.text = "<color=#FF0000>w</color><color=#FF7F00>i</color><color=#FFFF00>l</color><color=#00FF00>l</color><color=#0000FF>i</color><color=#4B0082>u</color><color=#8B00FF>m</color><color=#FF0000>s</color>";
            }
            if (_isCyan) {
                playerNameText.fontMaterial = Plugin.NonFurry;
            }
        }

        private void SetCrowns(string playerId) {

            profilePrefixPicture = null;
            profileHoverHint.enabled = false;
            Tuple<string, string> crownDetails = LeaderboardUtils.GetCrownDetails(playerId);

            if (!string.IsNullOrEmpty(crownDetails.Item1)) {
                profileHoverHint.enabled = true;
                profilePrefixPicture = crownDetails.Item1;
                profileHoverHint.text = crownDetails.Item2;
            }
        }

        private void SetRichStatus(Scene scene) {

            switch (scene) {
                case Scene.offline:
                    profilePrefixPicture = OfflineResource;
                    break;
                case Scene.menu:
                case Scene.playing:
                case Scene.online:
                    profilePrefixPicture = OnlineResource;
                    break;
                default:
                    profilePrefixPicture = OfflineResource;
                    break;
            }
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}