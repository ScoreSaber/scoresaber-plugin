using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using IPA.Utilities;
using ScoreSaber.Core.Daemons;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Data.Wrappers;
using ScoreSaber.Core.ReplaySystem;
using ScoreSaber.Core.Services;
using ScoreSaber.Extensions;
using ScoreSaber.UI.Elements.Leaderboard;
using ScoreSaber.UI.Elements.Profile;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using ScoreSaber.Core.Utils;
using System.Threading;

namespace ScoreSaber.UI.Leaderboard {

    internal class ScoreSaberLeaderboardViewController : IInitializable, IDisposable {

        #region BSML Components
        [UIComponent("root")]
        protected readonly RectTransform _root = null;
        [UIParams]
        private readonly BSMLParserParams _parserParams = null;
        [UIComponent("up-button")]
        protected readonly Button _upButton = null;
        [UIComponent("down-button")]
        protected readonly Button _downButton = null;

        [UIValue("info-buttons-view")]
        protected readonly InfoButtonsView _infoButtons = null;
        [UIValue("profile-picture-lb-view")]
        protected readonly ProfilePictureView _profileImages = null;
        [UIValue("score-detail-view")]
        protected readonly ScoreDetailView _scoreDetailView = null;
        [UIComponent("profile-detail-view")]
        protected readonly ProfileDetailView _profileDetailView = null;

        [UIAction("up-button-click")] private void UpButtonClicked() => DirectionalButtonClicked(false);
        [UIAction("down-button-click")] private void DownButtonClicked() => DirectionalButtonClicked(true);

#if PPV3
        [UIAction("PPv3-replay-click")] private void PPv3ReplayClick() => _ = PPv3ReplayClicked();
#endif
        #endregion

        public bool activated { get; private set; }
        public int leaderboardPage { get; set; } = 1;

        private bool _filterAroundCountry;
        private bool _replayDownloading;
        private string _currentLeaderboardRefreshId = string.Empty;

        private readonly PanelView _panelView;
        private readonly DiContainer _container;
        private readonly IUploadDaemon _uploadDaemon;
        private readonly ReplayLoader _replayLoader;
        private readonly PlayerService _playerService;
        private readonly LeaderboardService _leaderboardService;
        private readonly PlayerDataModel _playerDataModel;
        private readonly PlatformLeaderboardViewController _platformLeaderboardViewController;

        // TODO: Put this somewhere nicer?
        public enum UploadStatus {
            Packaging = 0,
            Uploading = 1,
            Success = 2,
            Retrying = 3,
            Error = 4,
            Done
        }

        private bool _isOST = false;
        public bool isOST {
            get { return _isOST; }
            set {
                if (!_isOST && value == true) {
                    _infoButtons.HideInfoButtons();
                    _profileImages.HideImageViews();
                    _panelView.SetRankedStatus("N/A (Not Custom Song)");
                }
                _isOST = value;
            }
        }

        public ScoreSaberLeaderboardViewController(DiContainer container, PanelView panelView, IUploadDaemon uploadDaemon, ReplayLoader replayLoader, PlayerService playerService, LeaderboardService leaderboardService, PlayerDataModel playerDataModel, PlatformLeaderboardViewController platformLeaderboardViewController, StandardLevelDetailViewController standardLevelDetailViewController) {

            _container = container;
            _panelView = panelView;
            _uploadDaemon = uploadDaemon;
            _replayLoader = replayLoader;
            _playerService = playerService;
            _playerDataModel = playerDataModel;
            _leaderboardService = leaderboardService;

            _platformLeaderboardViewController = platformLeaderboardViewController;

            _infoButtons = new InfoButtonsView();
            _profileImages = new ProfilePictureView();
            _scoreDetailView = new ScoreDetailView();
        }

        public void Initialize() {

            _scoreDetailView.showProfile += scoreDetailView_showProfile;
            _scoreDetailView.startReplay += scoreDetailView_startReplay;
            _playerService.LoginStatusChanged += playerService_LoginStatusChanged;
            _infoButtons.infoButtonClicked += infoButtons_infoButtonClicked;
            _uploadDaemon.UploadStatusChanged += uploadDaemon_UploadStatusChanged;
            _platformLeaderboardViewController.didActivateEvent += LeaderboardViewActivate;
        }

        [UIAction("#post-parse")]
        public void Parsed() {
            _upButton.transform.localScale *= .5f;
            _downButton.transform.localScale *= .5f;
            _root.name = "ScoreSaberLeaderboardElements";
            _infoButtons.HideInfoButtons();
            _profileImages.HideImageViews();
            activated = true;
        }

        public void AllowReplayWatching(bool value) {

            _scoreDetailView.AllowReplayWatching(value);
        }

        private void LeaderboardViewActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {

            if (!firstActivation) { return; }

            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "ScoreSaber.UI.Leaderboard.ScoreSaberLeaderboardViewController.bsml"), _platformLeaderboardViewController.gameObject, this);

            _panelView.Show();

            _panelView.disabling = delegate () {
                if (_scoreDetailView.detailModalRoot != null && _profileDetailView.profileModalRoot != null) {
                    _scoreDetailView.detailModalRoot.gameObject.SetActive(false);
                    _profileDetailView.profileModalRoot.gameObject.SetActive(false);
                    Accessors.animateParentCanvas(ref _scoreDetailView.detailModalRoot) = true;
                    Accessors.animateParentCanvas(ref _profileDetailView.profileModalRoot) = true;
                }
            };

            _panelView.statusWasSelected = delegate () {
                if (_leaderboardService.currentLoadedLeaderboard == null) { return; }
                _parserParams.EmitEvent("close-modals");
                Application.OpenURL($"https://scoresaber.com/leaderboard/{_leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo.id}");
            };

            _panelView.rankingWasSelected = delegate () {
                _parserParams.EmitEvent("close-modals");
                _parserParams.EmitEvent("show-profile");
                _profileDetailView.ShowProfile(_playerService.localPlayerInfo.playerId).RunTask();
            };

            _container.Inject(_profileDetailView);
            _playerService.GetLocalPlayerInfo();
        }

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
                case PlayerService.LoginStatus.Info:
                    _panelView.SetPromptInfo(status, true);
                    break;
                case PlayerService.LoginStatus.Error:
                    _panelView.SetPromptError(status, false);
                    break;
                case PlayerService.LoginStatus.Success:
                    _panelView.SetPromptSuccess(status, false, 3f);
                    _panelView.RankUpdater().RunTask();
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
                    _profileImages.HideImageViews();
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

        private CancellationTokenSource cancellationToken;

        public async Task RefreshLeaderboard(IDifficultyBeatmap difficultyBeatmap, LeaderboardTableView tableView, PlatformLeaderboardsModel.ScoresScope scope, LoadingControl loadingControl, string refreshId) {

            try {

                _currentLeaderboardRefreshId = refreshId;
                if (_uploadDaemon.uploading) { return; }
                if (!activated) { return; }

                if (scope == PlatformLeaderboardsModel.ScoresScope.AroundPlayer && !_filterAroundCountry) {
                    _upButton.interactable = false;
                    _downButton.interactable = false;
                } else {
                    _upButton.interactable = true;
                    _downButton.interactable = true;
                }

                _infoButtons.HideInfoButtons();
                _profileImages.HideImageViews();

                if(cancellationToken != null) {
                    cancellationToken.Cancel();
                    cancellationToken.Dispose();
                }
                cancellationToken = new CancellationTokenSource();

                var beatmapData = await difficultyBeatmap.GetBeatmapDataAsync(difficultyBeatmap.level.environmentInfo, _playerDataModel.playerData.playerSpecificSettings);

                if (LeaderboardUtils.ContainsV3Stuff(beatmapData)) {
                    SetErrorState(tableView, loadingControl, null, null, "Maps with new note types currently not supported", false);
                    _profileImages.HideImageViews();
                    return;
                }

                if (_playerService.loginStatus == PlayerService.LoginStatus.Error) {
                    SetErrorState(tableView, loadingControl, null, null, "ScoreSaber authentication failed, please restart Beat Saber", false);
                    _profileImages.HideImageViews();
                    return;
                }

                if (_playerService.loginStatus != PlayerService.LoginStatus.Success) {
                    return;
                }


                await Task.Delay(500); // Delay before doing anything to prevent leaderboard spam


                if (_currentLeaderboardRefreshId == refreshId) {
                    LeaderboardMap leaderboardData = await _leaderboardService.GetLeaderboardData(difficultyBeatmap, scope, leaderboardPage, _playerDataModel.playerData.playerSpecificSettings, _filterAroundCountry);
                    SetRankedStatus(leaderboardData.leaderboardInfoMap.leaderboardInfo);
                    List<LeaderboardTableView.ScoreData> leaderboardTableScoreData = leaderboardData.ToScoreData();
                    int playerScoreIndex = GetPlayerScoreIndex(leaderboardData);
                    if (leaderboardTableScoreData.Count != 0) {
                        if (scope == PlatformLeaderboardsModel.ScoresScope.AroundPlayer && playerScoreIndex == -1 && !_filterAroundCountry) {
                            SetErrorState(tableView, loadingControl, null, null, "You haven't set a score on this leaderboard");
                        } else {
                            tableView.SetScores(leaderboardTableScoreData, playerScoreIndex);
                            List<string> avatarURLS = new List<string>();
                            for (int i = 0; i < leaderboardTableScoreData.Count; i++) {
                                avatarURLS.Add(leaderboardData.scores[i].score.leaderboardPlayerInfo.profilePicture);
                            }
                            _profileImages.SetImages(avatarURLS, cancellationToken.Token);
                            loadingControl.ShowText("", false);
                            loadingControl.Hide();
                            _infoButtons.UpdateInfoButtonState(leaderboardTableScoreData.Count);
                            if (_uploadDaemon.uploading) {
                                _panelView.DismissPrompt();
                            }
                        }
                    } else {
                        if (leaderboardPage > 1) {
                            SetErrorState(tableView, loadingControl, null, null, "No scores on this page");
                        } else {
                            SetErrorState(tableView, loadingControl, null, null, "No scores on this leaderboard, be the first!");
                        }
                        _profileImages.HideImageViews();
                    }
                }
            } catch (HttpErrorException httpError) {
                SetErrorState(tableView, loadingControl, httpError);
            } catch (Exception exception) {
                SetErrorState(tableView, loadingControl, null, exception);
            }
        }

        private void SetRankedStatus(LeaderboardInfo leaderboardInfo) {
            if (leaderboardInfo.ranked) {
                if (leaderboardInfo.positiveModifiers) {
                    _panelView.SetRankedStatus("Ranked (DA = +0.02, GN +0.04)");
                } else {
                    _panelView.SetRankedStatus("Ranked (modifiers disabled)");
                }
                return;
            }
            if (leaderboardInfo.qualified) {
                _panelView.SetRankedStatus("Qualified");
                return;
            }
            if (leaderboardInfo.loved) {
                _panelView.SetRankedStatus("Loved");
                return;
            }
            _panelView.SetRankedStatus("Unranked");
        }

        public int GetPlayerScoreIndex(LeaderboardMap leaderboardMap) {
            for (int i = 0; i < leaderboardMap.scores.Length; i++) {
                if (leaderboardMap.scores[i].score.leaderboardPlayerInfo.id == _playerService.localPlayerInfo.playerId) {
                    return i;
                }
            }
            return -1;
        }

        private void SetErrorState(LeaderboardTableView tableView, LoadingControl loadingControl, HttpErrorException httpErrorException = null, Exception exception = null, string errorText = "Failed to load leaderboard, score won't upload", bool showRefreshButton = true) {


            if (httpErrorException != null) {
                if (httpErrorException.isNetworkError) {
                    errorText = "Failed to load leaderboard due to a network error, score won't upload";
                    _leaderboardService.currentLoadedLeaderboard = null;
                }
                if (httpErrorException.isScoreSaberError) {
                    errorText = httpErrorException.scoreSaberError.errorMessage;
                    if (errorText == "Leaderboard not found") {
                        _leaderboardService.currentLoadedLeaderboard = null;
                        _panelView.SetRankedStatus("");
                    }
                }
            }
            if (exception != null) {
                Plugin.Log.Error(exception.ToString());
            }
            loadingControl.Hide();
            loadingControl.ShowText(errorText, showRefreshButton);
            tableView.SetScores(new List<LeaderboardTableView.ScoreData>(), -1);
            _profileImages.HideImageViews();
        }

        public void DirectionalButtonClicked(bool down) {

            if (down) {
                leaderboardPage++;
            } else {
                leaderboardPage--;
            }
            RefreshLeaderboard();
            CheckPage();
        }

        public void ChangePageButtonsEnabledState(bool state) {

            if (state) {
                if (leaderboardPage > 1) {
                    _upButton.interactable = state;
                }
                _downButton.interactable = state;
            } else {
                _upButton.interactable = state;
                _downButton.interactable = state;
            }
        }

        public void CheckPage() {

            if (leaderboardPage > 1) {
                _upButton.interactable = true;
            } else {
                _upButton.interactable = false;
            }
        }

        public void RefreshLeaderboard() {

            if (!activated)
                return;

            _platformLeaderboardViewController?.InvokeMethod<object, PlatformLeaderboardViewController>("Refresh", true, true);
        }

        public void ChangeScope(bool filterAroundCountry) {

            if (activated) {
                _filterAroundCountry = filterAroundCountry;
                leaderboardPage = 1;
                RefreshLeaderboard();
                CheckPage();
            }
        }

        private async Task StartReplay(ScoreMap score) {

            _parserParams.EmitEvent("close-modals");
            _replayDownloading = true;

            try {
                _panelView.SetPromptInfo("Downloading Replay...", true);
                byte[] replay = await _playerService.GetReplayData(score.parent.difficultyBeatmap, score.parent.leaderboardInfo.id, score);
                _panelView.SetPromptInfo("Replay downloaded! Unpacking...", true);
                await _replayLoader.Load(replay, score.parent.difficultyBeatmap, score.gameplayModifiers, score.score.leaderboardPlayerInfo.name);
                _panelView.SetPromptSuccess("Replay Started!", false, 1f);
            } catch (Exception ex) {
                _panelView.SetPromptError("Failed to start replay! Error written to log.", false);
                Plugin.Log.Error($"Failed to start replay: {ex}");
            }
            _replayDownloading = false;
        }

        public void Dispose() {

            _platformLeaderboardViewController.didActivateEvent -= LeaderboardViewActivate;
            _playerService.LoginStatusChanged -= playerService_LoginStatusChanged;
            _uploadDaemon.UploadStatusChanged -= uploadDaemon_UploadStatusChanged;
            _infoButtons.infoButtonClicked -= infoButtons_infoButtonClicked;
            _scoreDetailView.startReplay -= scoreDetailView_startReplay;
            _scoreDetailView.showProfile -= scoreDetailView_showProfile;
        }

#if PPV3

        public void UpdatePPv3ButtonState(bool active) {
            buttonPPv3Replay.gameObject.SetActive(active);
        }

        private LeaderboardScoreData _currentPPv3ScoreData = null;
        public void CheckPPv3ReplayExists(LeaderboardScoreData scoreData) {

            _currentPPv3ScoreData = scoreData;
            string levelId = _currentPPv3ScoreData.level.level.levelID.Replace(" WIP", "").Split('_')[2];
            string ppv3ReplayPath = $@"{Settings.replayPath}\PPv3-{Shared.ReplaceInvalidChars(_currentPPv3ScoreData.level.level.songName)}-{levelId}-{(_currentPPv3ScoreData.level.difficulty.SerializedName())}.dat";
            Plugin.Log.Info(ppv3ReplayPath);
            if (File.Exists(ppv3ReplayPath)) {
                UpdatePPv3ButtonState(true);
            } else {
                UpdatePPv3ButtonState(false);
            }
        }

        private async Task PPv3ReplayClicked() {

            _replayDownloading = true;
            buttonPPv3Replay.interactable = false;
            parserParams.EmitEvent("close-modals");
            try {
                string levelId = _currentPPv3ScoreData.level.level.levelID.Replace(" WIP", "").Split('_')[2];
                string ppv3ReplayPath = $@"{Settings.replayPath}\PPv3-{Shared.ReplaceInvalidChars(_currentPPv3ScoreData.level.level.songName)}-{levelId}-{(_currentPPv3ScoreData.level.difficulty.SerializedName())}.dat";
              
                if (File.Exists(ppv3ReplayPath)) {
                    byte[] replay = File.ReadAllBytes(ppv3ReplayPath);
                    if (replay != null) {
                        SetPanelViewPromptInfo("Replay loaded! Unpacking...", true);
                        await Shared.replayLoader.Load(replay, _currentPPv3ScoreData.level, new GameplayModifiers(), "PPv3");
                        SetPanelViewPromptSuccess("Replay Started!", false, 1f);
                        buttonPPv3Replay.interactable = true;
                    }
                }
            } catch (Exception ex) {
                SetPanelViewPromptError("Failed to start replay! Error written to log.", false);
                Plugin.Log.Error($"Failed to start replay: {ex}");
            }
            _replayDownloading = false;
        }

#endif
    }
}