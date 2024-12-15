using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Loader;
using IPA.Utilities;
using IPA.Utilities.Async;
using LeaderboardCore.Interfaces;
using ScoreSaber.Core.Daemons;
using ScoreSaber.Core.Data;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Data.Wrappers;
using ScoreSaber.Core.Http;
using ScoreSaber.Core.Http.Configuration;
using ScoreSaber.Core.Http.Endpoints.Web;
using ScoreSaber.Core.ReplaySystem;
using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Core.Services;
using ScoreSaber.Core.Utils;
using ScoreSaber.Extensions;
using ScoreSaber.UI.Elements.Leaderboard;
using ScoreSaber.UI.Elements.Profile;
using ScoreSaber.UI.Leaderboard;
using ScoreSaber.UI.Main;
using ScoreSaber.UI.PromoBanner;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Button = UnityEngine.UI.Button;

namespace ScoreSaber.UI.Leaderboard {

    [HotReload(RelativePathToLayout = @"./ScoreSaberLeaderboardViewController.bsml")]
    [ViewDefinition("ScoreSaber.UI.Leaderboard.ScoreSaberLeaderboardViewController.bsml")]
    internal class ScoreSaberLeaderboardViewController : BSMLAutomaticViewController, INotifyLeaderboardSet, IInitializable {

        // TODO: Put both of these somewhere nicer?

        public enum ScoreSaberScoresScope {
            Global,
            Player,
            Friends,
            Area
        }

        public enum UploadStatus {
            Packaging = 0,
            Uploading = 1,
            Success = 2,
            Retrying = 3,
            Error = 4,
            Done
        }

        [UIParams]
        private readonly BSMLParserParams _parserParams = null;

        [UIComponent("leaderboardTableView")]
        private readonly LeaderboardTableView leaderboardTableView = null;

        [UIComponent("leaderboardTableView")]
        internal readonly Transform leaderboardTransform = null;

        [UIComponent("myHeader")]
        private readonly Backgroundable myHeader = null;

        [UIComponent("headerText")]
        private readonly TextMeshProUGUI headerText = null;

        [UIObject("headerSTATIC")]
        private readonly GameObject headerSTATIC = null;

        [UIComponent("headerTextSTATIC")]
        private readonly TextMeshProUGUI headerTextSTATIC = null;

        [UIComponent("errorText")]
        private readonly TextMeshProUGUI _errorText = null;

        [UIValue("imageHolders")]
        [Inject] internal List<ProfilePictureView> _ImageHolders = null;

        [UIValue("cellClickerHolders")]
        [Inject] internal List<CellClickingView> _cellClickingHolders = null;

        [UIValue("entry-holder")]
        internal EntryHolder _infoButtons = null;
        
        [UIValue("score-detail-view")]
        protected ScoreDetailView _scoreDetailView = null;

        [UIComponent("profile-detail-view")]
        protected readonly ProfileDetailView _profileDetailView = null;

        [UIComponent("up_button")]
        private readonly Button _upButton = null;

        [UIComponent("down_button")]
        private readonly Button _downButton = null;

        [UIObject("loadingLB")]
        private readonly GameObject loadingLB = null;

        [UIObject("starRatingBox")]
        private readonly GameObject starRatingBox = null;

        [UIValue("map-info-view")]
        protected MapInfoView _mapInfoView = null;

        [UIValue("yes-or-no-modal")]
        protected GenericYesOrNoModal _genericYesOrNoModal = null;

        [UIAction("OnPageUp")] private void UpButtonClicked() => UpdatePageChanged(-1);
        [UIAction("OnPageDown")] private void DownButtonClicked() => UpdatePageChanged(1);


        public bool activated { get; private set; }
        public int leaderboardPage { get; set; } = 1;

        public ScoreSaberScoresScope currentScoreScope { get; set; }

        private bool _replayDownloading = false;
        private string _currentLeaderboardRefreshId = string.Empty;
        private BeatmapKey _currentBeatmapKey;

        Color yellow = new Color(250f / 255f, 221f / 255f, 45f / 255f);
        Color green = new Color(63 / 255f, 191 / 255f, 100 / 255f);
        Color grey = new Color(125f / 255f, 125f / 255f, 125f / 255f);
        Color blue = new Color(62 / 255f, 152 / 255f, 237 / 255f);
        Color pink = new Color(235 / 255f, 73 / 255f, 232 / 255f);
        Color _scoreSaberBlue = new Color(0f, 0.4705882f, 0.7254902f);

        Color easyDiffColour => UIUtils.HexToColor("#50c17b");
        Color normalDiffColour => UIUtils.HexToColor("#65aafb");
        Color hardDiffColour => UIUtils.HexToColor("#f18e40");
        Color expertDiffColour => UIUtils.HexToColor("#e44f5b");
        Color expertPlusDiffColour => UIUtils.HexToColor("#a266dd");

        private const string _leaderboardUrl = "https://scoresaber.com/leaderboard/";

        [Inject] private readonly PanelView _panelView = null;
        [Inject] private readonly DiContainer _container = null;
        [Inject] private readonly IUploadDaemon _uploadDaemon = null;
        [Inject] private readonly ReplayLoader _replayLoader = null;
        [Inject] private readonly PlayerService _playerService = null;
        [Inject] private readonly LeaderboardService _leaderboardService = null;
        [Inject] internal readonly PlatformLeaderboardViewController _platformLeaderboardViewController = null;
        [Inject] internal readonly RichPresenceService _scoresaberRichPresence = null;
        [Inject] private readonly MaxScoreCache _maxScoreCache = null;
        [Inject] private readonly BeatmapLevelsModel _beatmapLevelsModel = null;
        [Inject] private readonly TweeningUtils _tweeningUtils = null;
        [Inject] private readonly PromoBanner.PromoBanner _promoBanner = null;

        private void infoButtons_infoButtonClicked(int index) {
            if (_leaderboardService.currentLoadedLeaderboard == null) { return; }

            _parserParams.EmitEvent("present-score-info");
            _scoreDetailView.SetScoreInfo(_leaderboardService.currentLoadedLeaderboard.scores[index], _replayDownloading);
        }

        private void scoreDetailView_showProfile(string playerId) {

            _parserParams.EmitEvent("close-modals");
            _parserParams.EmitEvent("show-profile");
            _profileDetailView.ShowProfile(playerId).RunTask();
        }

        private void scoreDetailView_startReplay(ScoreMap score) {
            StartReplay(score).RunTask();
        }

        private void playerService_LoginStatusChanged(PlayerService.LoginStatus loginStatus, string status) {
            switch (loginStatus) {
                case PlayerService.LoginStatus.InProgress:
                    _panelView.SetPromptInfo(status, true);
                    break;
                case PlayerService.LoginStatus.Error:
                    _panelView.SetPromptError(status, false);
                    break;
                case PlayerService.LoginStatus.Success:
                    if (Plugin.Settings.enableRichPresence) {
                        UnityMainThreadTaskScheduler.Factory.StartNew(() => _scoresaberRichPresence.Initialize());
                    }
                    _panelView.SetPromptSuccess(status, false, 3f);
                    _panelView.RankUpdater().RunTask();
                    _ImageHolders.ForEach(holder => holder.ClearSprite());
                    RefreshLeaderboard();
                    break;
            }
            Plugin.Log.Debug(status);
        }

        private void uploadDaemon_UploadStatusChanged(UploadStatus status, string statusText) {
            if (statusText != string.Empty) {
                Plugin.Log.Debug($"{statusText}");
            }
            switch (status) {
                case UploadStatus.Packaging:
                    _panelView.Loaded(false);
                    _panelView.SetPromptInfo(statusText, true);
                    ByeImages();
                    break;
                case UploadStatus.Uploading:
                    _panelView.SetPromptInfo(statusText, true);
                    break;
                case UploadStatus.Success:
                    _panelView.SetPromptSuccess(statusText, false, 2f);
                    break;
                case UploadStatus.Retrying:
                    _panelView.SetPromptError(statusText, true);
                    break;
                case UploadStatus.Error:
                    _panelView.SetPromptError(statusText, false, 3f);
                    break;
                case UploadStatus.Done:
                    RefreshLeaderboard();
                    _panelView.UpdateRank().RunTask();
                    break;
            }
        }

        private ImageView _headerBackground;

        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
        internal static readonly FieldAccessor<ImageView, bool>.Accessor ImageGradient = FieldAccessor<ImageView, bool>.GetAccessor("_gradient");

        [UIAction("#post-parse")]
        private void PostParse() {
            myHeader.Background.material = Utilities.ImageResources.NoGlowMat;
            var loadingLB = leaderboardTransform.Find("LoadingControl").gameObject;
            Transform loadingContainer = loadingLB.transform.Find("LoadingContainer");
            loadingContainer.gameObject.SetActive(false);
            Destroy(loadingContainer.Find("Text").gameObject);
            Destroy(loadingLB.transform.Find("RefreshContainer").gameObject);
            Destroy(loadingLB.transform.Find("DownloadingContainer").gameObject);
            _headerBackground = myHeader.Background as ImageView;

            _headerBackground.color = grey;
            _headerBackground.color0 = grey;
            _headerBackground.color1 = grey;
            ImageSkew(ref _headerBackground) = 0.18f;
            ImageGradient(ref _headerBackground) = true;
            CheckPage();
            _ImageHolders.ForEach(holder => holder.ClearSprite());
            myHeader.transform.SetParent(_platformLeaderboardViewController.transform.Find("HeaderPanel"), true);
        }

        public void CloseModals() {
            _parserParams.EmitEvent("close-modals");
        }

        private void SetPanelStatus(LeaderboardInfoMap leaderboardInfoMap = null) {

            bool fromCached = true;
            if (leaderboardInfoMap == null) {
                if (_leaderboardService.currentLoadedLeaderboard == null) {
                    return;
                }
                leaderboardInfoMap = _leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap;
                fromCached = false;
            }

            if (leaderboardInfoMap == null || leaderboardInfoMap.isOst) {
                _tweeningUtils.LerpColor(_headerBackground, grey);
                headerTextSTATIC.text = "OST";
                _tweeningUtils.FadeText(headerTextSTATIC, true, 0.2f);
                return;
            }

            bool ranked = leaderboardInfoMap.leaderboardInfo.ranked;
            bool qualified = leaderboardInfoMap.leaderboardInfo.qualified;
            bool loved = leaderboardInfoMap.leaderboardInfo.loved;


            if (leaderboardInfoMap.leaderboardInfo.stars != 0) {
                starRatingBox.gameObject.SetActive(true);
                headerText.text = $"<size=70%> </size>{leaderboardInfoMap.leaderboardInfo.stars.ToString().Replace(".", ". ")}<size=70%>★</size>";
                headerText.richText = true;
                headerSTATIC.gameObject.SetActive(false);
            } else {
                starRatingBox.gameObject.SetActive(false);
                headerSTATIC.gameObject.SetActive(true);
            }

            if (!ranked && !qualified && !loved) {
                _tweeningUtils.LerpColor(_headerBackground, grey);
                headerTextSTATIC.text = "UNRANKED";
                if (!fromCached) {
                    _tweeningUtils.FadeText(headerTextSTATIC, true, 0.2f);
                }
            }

            if (ranked) {
                GetSSDifficultyColour(leaderboardInfoMap.beatmapKey.difficulty);
            }

            if (qualified) {
                _tweeningUtils.LerpColor(_headerBackground, _scoreSaberBlue);
                headerTextSTATIC.text = "QUALIFIED";
                if (!fromCached) {
                    _tweeningUtils.FadeText(headerTextSTATIC, true, 0.2f);
                }
            }

            if (loved) {
                _tweeningUtils.LerpColor(_headerBackground, pink);
                headerTextSTATIC.text = "LOVED";
                if (!fromCached) {
                    _tweeningUtils.FadeText(headerTextSTATIC, true, 0.2f);
                }
            }
        }

        private void GetSSDifficultyColour(BeatmapDifficulty beatmapDifficulty) {
            switch (beatmapDifficulty) {
                case BeatmapDifficulty.Easy:
                    _tweeningUtils.LerpColor(_headerBackground, easyDiffColour);
                    break;
                case BeatmapDifficulty.Normal:
                    _tweeningUtils.LerpColor(_headerBackground, normalDiffColour);
                    break;
                case BeatmapDifficulty.Hard:
                    _tweeningUtils.LerpColor(_headerBackground, hardDiffColour);
                    break;
                case BeatmapDifficulty.Expert:
                    _tweeningUtils.LerpColor(_headerBackground, expertDiffColour);
                    break;
                case BeatmapDifficulty.ExpertPlus:
                    _tweeningUtils.LerpColor(_headerBackground, expertPlusDiffColour);
                    break;
                default:
                    break;
            }
        }
        
        [UIAction("OpenLeaderboardPage")]
        internal void OpenLeaderboardPage() {
            Application.OpenURL(new WebLeaderboard(_leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo.id.ToString()).BuildUrl());
        }

        [UIAction("MapInfoClicked")]
        internal void MapInfoClicked() {
            _parserParams.EmitEvent("present-map-info");
        }

        [UIAction("clicked-status")]
        protected void ClickedStatus() {

            _panelView.statusWasSelected?.Invoke();
        }

        [UIAction("OnIconSelected")]
        private void OnIconSelected(SegmentedControl segmentedControl, int index) {
            currentScoreScope = (ScoreSaberScoresScope)index;
            leaderboardPage = 1;
            OnLeaderboardSet(_currentBeatmapKey);
            CheckPage();
        }

        [UIValue("leaderboardIcons")]
        private List<IconSegmentedControl.DataItem> leaderboardIcons {
            get {
#pragma warning disable CS0618 // Type or member is obsolete
                return new List<IconSegmentedControl.DataItem>
        {
            new IconSegmentedControl.DataItem(Utilities.FindSpriteInAssembly("ScoreSaber.Resources.globe.png"), "Global"),
            new IconSegmentedControl.DataItem(Utilities.FindSpriteInAssembly("ScoreSaber.Resources.Player.png"), "Around you"),
            new IconSegmentedControl.DataItem(Utilities.FindSpriteInAssembly("ScoreSaber.Resources.friend.png"), "Friends"),
            new IconSegmentedControl.DataItem(Utilities.FindSpriteInAssembly("ScoreSaber.Resources.country.png"), "Area")
        };
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        private void ShowRichPresenceDisclaimer() {
            var info = new GenericYesOrNoModal.YesOrNoModalInfo("Rich Presence", delegate () {
                Plugin.Settings.enableRichPresence = true;
                Plugin.Settings.hasAcceptedRichPresenceDisclaimer = true;
                _scoresaberRichPresence.Initialize();
                _parserParams.EmitEvent("close-modals");
            }, delegate () {
                Plugin.Settings.enableRichPresence = false;
                Plugin.Settings.hasAcceptedRichPresenceDisclaimer = true;
                _parserParams.EmitEvent("close-modals");

            }, "Rich Presence is a feature that allows you to display your current status in Beat Saber on your profile. Would you like to enable this?\n(You can turn it off anytime in the ScoreSaber settings menu)");
            _genericYesOrNoModal.Show(info);
            _parserParams.EmitEvent("present-yes-no-modal");
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) { 
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!base.isActiveAndEnabled) return;
            if (!_platformLeaderboardViewController) return;
            if (firstActivation) {
                _panelView.statusWasSelected = delegate () {
                    if (_leaderboardService.currentLoadedLeaderboard == null) { return; }
                    _parserParams.EmitEvent("close-modals");
                    Application.OpenURL($"{_leaderboardUrl}{_leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo.id}");
                };

                _panelView.rankingWasSelected = delegate () {
                    _parserParams.EmitEvent("close-modals");
                    _parserParams.EmitEvent("show-profile");
                    _profileDetailView.ShowProfile(_playerService.localPlayerInfo.playerId).RunTask();
                };

                _container.Inject(_profileDetailView);
                _ImageHolders.ForEach(holder => holder.ClearSprite());
                activated = true;
                OnIconSelected(null, 0);
                _promoBanner.CreatePromoBanner();
                _promoBanner._floatingScreen.gameObject.SetActive(true);
                _promoBanner._promoBannerView.GetComponent<CanvasGroup>().alpha = 1;
            }

            UnityMainThreadTaskScheduler.Factory.StartNew(async() => {
                if (!Plugin.Settings.hasAcceptedRichPresenceDisclaimer) {
                    await Task.Delay(10); // seems to be consistent? // no idea exactly why // yield didnt work
                    ShowRichPresenceDisclaimer();
                }
            });

            Transform header = _platformLeaderboardViewController.transform.Find("HeaderPanel");
            _platformLeaderboardViewController.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0, 0, 0, 0);
            myHeader.gameObject.SetActive(true);
        }

        public void OnEnable() {
            if(_promoBanner._floatingScreen != null) {
                _promoBanner.ShowBanner.Invoke(true);
            }
        }

        public void OnDisable() {
            if (_promoBanner._floatingScreen != null) {
                _promoBanner.ShowBanner.Invoke(false);
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            if (!_platformLeaderboardViewController || !_platformLeaderboardViewController.isActivated) return;
            _platformLeaderboardViewController.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            myHeader.gameObject.SetActive(false);
            if (!_platformLeaderboardViewController.isActivated) return;
            if (_scoreDetailView.detailModalRoot != null) _scoreDetailView.detailModalRoot.Hide(false);
            if (_profileDetailView.profileModalRoot != null) _profileDetailView.profileModalRoot.Hide(false);
        }

        private CancellationTokenSource cancellationToken;

        public async Task RefreshLeaderboard(BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, LeaderboardTableView tableView, ScoreSaberScoresScope scope, GameObject loadingControl, string refreshId) {
            try {
                if (loadingControl == null || tableView == null) return;
                bool setPanelStatusFromCache = false;
                loadingControl.SetActive(false);
                _errorText.gameObject.SetActive(false);
                SetErrorState(tableView, ref loadingControl, errorText: "");
                tableView.SetScores(new List<LeaderboardTableView.ScoreData>(), -1);
                SetClickersOff();
                headerTextSTATIC.text = "";
                _currentLeaderboardRefreshId = refreshId;
                if (_uploadDaemon.uploading) { return; }
                if (!activated) { return; }

                if (scope == ScoreSaberScoresScope.Player) {
                    _upButton.interactable = false;
                    _downButton.interactable = false;
                }

                ByeImages();
                _errorText.gameObject.SetActive(false);
                loadingControl.SetActive(true);
                starRatingBox.gameObject.SetActive(false);
                headerSTATIC.gameObject.SetActive(true);
                _mapInfoView.ResetName();
                _mapInfoView.mapInfoSetLoading.gameObject.SetActive(true);
                _mapInfoView.mapInfoSet.SetActive(false);

                if (_leaderboardService.GetLeaderboardInfoMapFromCache(beatmapKey) != null) {
                    SetPanelStatus(_leaderboardService.GetLeaderboardInfoMapFromCache(beatmapKey));
                    setPanelStatusFromCache = true;
                } else {
                    _tweeningUtils.LerpColor(_headerBackground, grey, 0.1f);
                }

                if (cancellationToken != null) {
                    cancellationToken.Cancel();
                    cancellationToken.Dispose();
                }
                cancellationToken = new CancellationTokenSource();

                if (_playerService.loginStatus == PlayerService.LoginStatus.Error) {
                    SetErrorState(tableView, ref loadingControl, null, null, "ScoreSaber authentication failed, please restart Beat Saber", false);
                    ByeImages();
                    return;
                }

                if (_playerService.loginStatus != PlayerService.LoginStatus.Success) {
                    return;
                }
                await Task.Delay(500); // Delay before doing anything to prevent leaderboard spam

                if (_currentLeaderboardRefreshId == refreshId) {
                    int maxMultipliedScore = await _maxScoreCache.GetMaxScore(beatmapLevel, beatmapKey);
                    LeaderboardMap leaderboardData = await _leaderboardService.GetLeaderboardData(maxMultipliedScore, beatmapLevel, beatmapKey, scope, leaderboardPage);

                    if (_currentLeaderboardRefreshId != refreshId) {
                        return; // we need to check this again, since some time may have passed due to waiting for leaderboard data
                    }
                    if (!setPanelStatusFromCache) {
                        SetPanelStatus();
                    }
                    _mapInfoView._currentMap = beatmapLevel;
                    _mapInfoView.SetScoreInfo(leaderboardData.leaderboardInfoMap.leaderboardInfo);
                    List<LeaderboardTableView.ScoreData> leaderboardTableScoreData = leaderboardData.ToScoreData();
                    int playerScoreIndex = GetPlayerScoreIndex(leaderboardData);
                    if (leaderboardTableScoreData.Count != 0) {
                        if (scope == ScoreSaberScoresScope.Player && playerScoreIndex == -1) {
                            SetErrorState(tableView, ref loadingControl, null, null, "You haven't set a score on this leaderboard");
                        } else {
                            if (_currentLeaderboardRefreshId != refreshId) {
                                return; // we need to check this again, since some time may have passed due to waiting for leaderboard data
                            }
                            tableView.SetScores(leaderboardTableScoreData, playerScoreIndex);

                            for (int i = 0; i < leaderboardTableScoreData.Count; i++) {
                                if (_currentLeaderboardRefreshId != refreshId) {
                                    return; // we need to check this again, since some time may have passed due to waiting for leaderboard data
                                }
                                _ImageHolders[i].setProfileImage(leaderboardData.scores[i].score.leaderboardPlayerInfo.profilePicture, i, cancellationToken.Token);
                            }
                            loadingControl.gameObject.SetActive(false);
                            _errorText.gameObject.SetActive(false);
                            if (_uploadDaemon.uploading) {
                                _panelView.DismissPrompt();
                            }
                            CheckPage();
                        }
                    } else {
                        if (leaderboardPage > 1) {
                            SetErrorState(tableView, ref loadingControl, null, null, "No scores on this page");
                        } else {
                            SetErrorState(tableView, ref loadingControl, null, null, "No scores on this leaderboard, be the first!");
                        }
                        ByeImages();
                    }
                    PrettifyLeaderboardTableView(tableView, leaderboardData.scores, cancellationToken.Token);
                }
            } catch (HttpRequestException httpError) {
                SetErrorState(tableView, ref loadingControl, httpError);
            } catch (Exception exception) {
                SetErrorState(tableView, ref loadingControl, null, exception);
            }
        }

        public int GetPlayerScoreIndex(LeaderboardMap leaderboardMap) {
            for (int i = 0; i < leaderboardMap.scores.Length; i++) {
                if (leaderboardMap.scores[i].score.leaderboardPlayerInfo.id == _playerService.localPlayerInfo.playerId) {
                    return i;
                }
            }
            return -1;
        }

        public void AllowReplayWatching(bool value) {

            _scoreDetailView.AllowReplayWatching(value);
        }

        private void SetErrorState(LeaderboardTableView tableView, ref GameObject loadingControl, HttpRequestException httpErrorException = null, Exception exception = null, string errorText = "Failed to load leaderboard, score won't upload", bool showRefreshButton = true) {
            try {
                SetClickersOff();
                if (httpErrorException != null) {
                    if (httpErrorException.IsNetworkError) {
                        errorText = "Failed to load leaderboard due to a network error, score won't upload";
                        _leaderboardService.currentLoadedLeaderboard = null;
                    }
                    if (httpErrorException.IsScoreSaberError) {
                        errorText = httpErrorException.ScoreSaberError.errorMessage;
                        if (errorText == "Leaderboard not found") {
                            _leaderboardService.currentLoadedLeaderboard = null;
                        }
                        if (errorText == "Player hasn't set a score on this leaderboard") {
                            errorText = "You haven't set a score on this map!";
                        }
                    }
                }
                if (exception != null) {
                    Plugin.Log.Error(exception.ToString());
                }
                loadingControl.gameObject.SetActive(false);
                _tweeningUtils.FadeText(_errorText, true, 0.2f);
                _errorText.text = errorText;
                tableView.SetScores(new List<LeaderboardTableView.ScoreData>(), -1);
                ByeImages();
                SetClickersOff();
            } catch {

            }
        }

        public void CheckPage() {
            if(_leaderboardService.currentLoadedLeaderboard == null || currentScoreScope == ScoreSaberScoresScope.Player) {
                _upButton.interactable = false;
                _downButton.interactable = false;
                return;
            }
            var totalPages = Mathf.CeilToInt((float)_leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo.plays / 10);
            _upButton.interactable = leaderboardPage > 1;
            _downButton.interactable = leaderboardPage < totalPages;
        }

        private void UpdatePageChanged(int inc) {
            if (_leaderboardService.currentLoadedLeaderboard == null) return;
            var totalPages = Mathf.CeilToInt((float)_leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo.plays / 10);
            leaderboardPage = Mathf.Clamp(leaderboardPage + inc, 0, totalPages - 1);
            RefreshLeaderboard();
            CheckPage();
        }

        public void RefreshLeaderboard() {

            if (!activated || _currentBeatmapKey == null)
                return;
            BeatmapLevel beatmapLevel = _beatmapLevelsModel.GetBeatmapLevel(_currentBeatmapKey.levelId);
            RefreshLeaderboard(beatmapLevel, _currentBeatmapKey, leaderboardTableView, currentScoreScope, loadingLB, Guid.NewGuid().ToString()).RunTask();
        }

        internal void ByeImages() {
            _ImageHolders.ForEach(holder => holder.ClearSprite());
        }

        private async Task StartReplay(ScoreMap score) {

            _parserParams.EmitEvent("close-modals");
            _replayDownloading = true;

            try {
                _panelView.SetPromptInfo("Downloading Replay...", true);
                byte[] replay = await _playerService.GetReplayData(score.parent.beatmapLevel, score.parent.beatmapKey, score.parent.leaderboardInfo.id, score);
                _panelView.SetPromptInfo("Replay downloaded! Unpacking...", true);
                Plugin.ReplayState.isUsersReplay = score.score.leaderboardPlayerInfo.id == _playerService.localPlayerInfo.playerId;
                await _replayLoader.Load(replay, score.parent.beatmapLevel, score.parent.beatmapKey, score.gameplayModifiers, score.score.leaderboardPlayerInfo.name);
                _panelView.SetPromptSuccess("Replay Started!", false, 1f);
            } catch (ReplayVersionException ex) {
                _panelView.SetPromptError("Unsupported replay version", false);
                Plugin.Log.Error($"Failed to start replay (unsupported version): {ex}");
            } catch (Exception ex) {
                _panelView.SetPromptError("Failed to start replay! Error written to log.", false);
                Plugin.Log.Error($"Failed to start replay: {ex}");
            }
            _replayDownloading = false;
        }

        private void SetClickersOff() {
            foreach (var holder in _cellClickingHolders) {
                var x = holder.cellClickerImage.gameObject.GetComponent<CellClicker>();
                if (x != null) {
                    x.clickable = false;
                }
            }
        }

        private bool obtainedAnchor = false;
        private Vector2 normalAnchor = Vector2.zero;

        void PrettifyLeaderboardTableView(LeaderboardTableView tableView, ScoreMap[] leaderboardInfo, CancellationToken cancellationToken) {
            LeaderboardTableCell[] cells = tableView.GetComponentsInChildren<LeaderboardTableCell>();

            for (int i = 0; i < cells.Length; i++) {
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }
                LeaderboardTableCell tableCell = cells[i];
                int cellIdx = tableCell.idx;
                Score score = leaderboardInfo[cellIdx].score;

                if (cellIdx < _cellClickingHolders.Count) {
                    var clickerHolder = _cellClickingHolders[cellIdx];

                    CellClicker cellClicker = clickerHolder.cellClickerImage.gameObject.GetComponent<CellClicker>();
                    if (cellClicker == null) {
                        cellClicker = clickerHolder.cellClickerImage.gameObject.AddComponent<CellClicker>();
                    }

                    cellClicker.onClick = _infoButtons.InfoButtonClicked;
                    cellClicker.index = cellIdx;
                    cellClicker.seperator = (ImageView)tableCell._separatorImage;
                    cellClicker.clickable = true;
                    cellClicker.OnPointerExit(null);
                    cellClicker.isCool = score.leaderboardPlayerInfo.id == PlayerIDs.Speecil;
                    cellClicker.ResetColourAndSize();

                    TextMeshProUGUI _playerNameText = tableCell._playerNameText;
                    TextMeshProUGUI _scoreText = tableCell._scoreText;
                    TextMeshProUGUI _rankText = tableCell._rankText;

                    _playerNameText.richText = true;

                    if (!obtainedAnchor) {
                        normalAnchor = _playerNameText.rectTransform.anchoredPosition;
                        obtainedAnchor = true;
                    }
                    Vector2 newPosition = new Vector2(normalAnchor.x + 3f, normalAnchor.y);
                    _playerNameText.rectTransform.anchoredPosition = newPosition;

                    tableCell.showSeparator = true;
                    _tweeningUtils.FadeText(_playerNameText, true, 0.2f);
                }
            }

            _downButton.interactable = cells.Length >= 9;
        }

        public void Initialize() {
            _infoButtons = new EntryHolder();
            _scoreDetailView = new ScoreDetailView();
            _mapInfoView = new MapInfoView();
            _genericYesOrNoModal = new GenericYesOrNoModal();
            _scoreDetailView.showProfile += scoreDetailView_showProfile;
            _scoreDetailView.startReplay += scoreDetailView_startReplay;
            _playerService.LoginStatusChanged += playerService_LoginStatusChanged;
            _infoButtons.infoButtonClicked += infoButtons_infoButtonClicked;
            _uploadDaemon.UploadStatusChanged += uploadDaemon_UploadStatusChanged;
        }

        public void Dispose() {

            _playerService.LoginStatusChanged -= playerService_LoginStatusChanged;
            _uploadDaemon.UploadStatusChanged -= uploadDaemon_UploadStatusChanged;
            _infoButtons.infoButtonClicked -= infoButtons_infoButtonClicked;
            _scoreDetailView.startReplay -= scoreDetailView_startReplay;
            _scoreDetailView.showProfile -= scoreDetailView_showProfile;
        }

        public void OnLeaderboardSet(BeatmapKey beatmapKey) {
            _currentBeatmapKey = beatmapKey;
            try {
                BeatmapLevel beatmapLevel = _beatmapLevelsModel.GetBeatmapLevel(beatmapKey.levelId);
                RefreshLeaderboard(beatmapLevel, beatmapKey, leaderboardTableView, currentScoreScope, loadingLB, Guid.NewGuid().ToString()).RunTask();
            } catch(Exception ex) { Plugin.Log.Error(ex.Message); }
        }
    }
}