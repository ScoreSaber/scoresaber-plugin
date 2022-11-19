#region

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using IPA.Utilities;
using ScoreSaber.Core.Daemons;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Data.Wrappers;
using ScoreSaber.Core.ReplaySystem;
using ScoreSaber.Core.Services;
using ScoreSaber.Core.Utils;
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

#endregion

namespace ScoreSaber.UI.Leaderboard {
    internal class ScoreSaberLeaderboardViewController : IInitializable, IDisposable {
        #region BSML Components

        [UIComponent("root")] protected readonly RectTransform _root = null;
        [UIParams] private readonly BSMLParserParams _parserParams = null;
        [UIComponent("up-button")] protected readonly Button _upButton = null;
        [UIComponent("down-button")] protected readonly Button _downButton = null;

        [UIValue("info-buttons-view")] protected readonly InfoButtonsView _infoButtons;
        [UIValue("score-detail-view")] protected readonly ScoreDetailView _scoreDetailView;
        [UIComponent("profile-detail-view")] protected readonly ProfileDetailView _profileDetailView = null;

        [UIAction("up-button-click")]
        private void UpButtonClicked() {
            DirectionalButtonClicked(false);
        }

        [UIAction("down-button-click")]
        private void DownButtonClicked() {
            DirectionalButtonClicked(true);
        }

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

        private bool _isOST;

        public bool isOST {
            get => _isOST;
            set {
                switch (_isOST) {
                    case false when value:
                        _infoButtons.HideInfoButtons();
                        _panelView.SetRankedStatus("N/A (Not Custom Song)");
                        break;
                }

                _isOST = value;
            }
        }

        public ScoreSaberLeaderboardViewController(DiContainer container, PanelView panelView,
            IUploadDaemon uploadDaemon, ReplayLoader replayLoader, PlayerService playerService,
            LeaderboardService leaderboardService, PlayerDataModel playerDataModel,
            PlatformLeaderboardViewController platformLeaderboardViewController,
            StandardLevelDetailViewController standardLevelDetailViewController) {
            _container = container;
            _panelView = panelView;
            _uploadDaemon = uploadDaemon;
            _replayLoader = replayLoader;
            _playerService = playerService;
            _playerDataModel = playerDataModel;
            _leaderboardService = leaderboardService;

            _platformLeaderboardViewController = platformLeaderboardViewController;

            _infoButtons = new InfoButtonsView();
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
            activated = true;
        }

        public void AllowReplayWatching(bool value) {
            _scoreDetailView.AllowReplayWatching(value);
        }

        private void LeaderboardViewActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            switch (firstActivation) {
                case false:
                    return;
            }

            BSMLParser.instance.Parse(
                Utilities.GetResourceContent(Assembly.GetExecutingAssembly(),
                    "ScoreSaber.UI.Leaderboard.ScoreSaberLeaderboardViewController.bsml"),
                _platformLeaderboardViewController.gameObject, this);

            _panelView.Show();

            _panelView.disabling = delegate {
                if (_scoreDetailView.detailModalRoot == null || _profileDetailView.profileModalRoot == null) {
                    return;
                }

                _scoreDetailView.detailModalRoot.gameObject.SetActive(false);
                _profileDetailView.profileModalRoot.gameObject.SetActive(false);
                Accessors.animateParentCanvas(ref _scoreDetailView.detailModalRoot) = true;
                Accessors.animateParentCanvas(ref _profileDetailView.profileModalRoot) = true;
            };

            _panelView.statusWasSelected = delegate {
                switch (_leaderboardService.currentLoadedLeaderboard) {
                    case null:
                        return;
                    default:
                        _parserParams.EmitEvent("close-modals");
                        Application.OpenURL(
                            $"https://scoresaber.com/leaderboard/{_leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo.id}");
                        break;
                }
            };

            _panelView.rankingWasSelected = delegate {
                _parserParams.EmitEvent("close-modals");
                _parserParams.EmitEvent("show-profile");
                _profileDetailView.ShowProfile(_playerService.localPlayerInfo.playerId).RunTask();
            };

            _container.Inject(_profileDetailView);
            _playerService.GetLocalPlayerInfo();
        }

        private void infoButtons_infoButtonClicked(int index) {
            switch (_leaderboardService.currentLoadedLeaderboard) {
                case null:
                    return;
                default:
                    _parserParams.EmitEvent("present-score-info");
                    _scoreDetailView.SetScoreInfo(_leaderboardService.currentLoadedLeaderboard.scores[index],
                        _replayDownloading);
                    break;
            }
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

        public async Task RefreshLeaderboard(IDifficultyBeatmap difficultyBeatmap, LeaderboardTableView tableView,
            PlatformLeaderboardsModel.ScoresScope scope, LoadingControl loadingControl, string refreshId) {
            try {
                _currentLeaderboardRefreshId = refreshId;
                switch (_uploadDaemon.uploading) {
                    case true:
                        return;
                }

                switch (activated) {
                    case false:
                        return;
                }


                switch (scope) {
                    case PlatformLeaderboardsModel.ScoresScope.AroundPlayer:
                        _upButton.interactable = false;
                        _downButton.interactable = false;
                        break;
                    default:
                        _upButton.interactable = true;
                        _downButton.interactable = true;
                        break;
                }

                _infoButtons.HideInfoButtons();

                IReadonlyBeatmapData beatmapData = await difficultyBeatmap.GetBeatmapDataAsync(
                    difficultyBeatmap.level.environmentInfo, _playerDataModel.playerData.playerSpecificSettings);

                if (LeaderboardUtils.ContainsV3Stuff(beatmapData)) {
                    SetErrorState(tableView, loadingControl, null, null,
                        "Maps with new note types currently not supported", false);
                    return;
                }

                switch (_playerService.loginStatus) {
                    case PlayerService.LoginStatus.Error:
                        SetErrorState(tableView, loadingControl, null, null,
                            "ScoreSaber authentication failed, please restart Beat Saber", false);
                        return;
                }

                if (_playerService.loginStatus != PlayerService.LoginStatus.Success) {
                    return;
                }


                await Task.Delay(500); // Delay before doing anything to prevent leaderboard spam


                if (_currentLeaderboardRefreshId == refreshId) {
                    LeaderboardMap leaderboardData = await _leaderboardService.GetLeaderboardData(difficultyBeatmap,
                        scope, leaderboardPage, _playerDataModel.playerData.playerSpecificSettings,
                        _filterAroundCountry);
                    SetRankedStatus(leaderboardData.leaderboardInfoMap.leaderboardInfo);
                    List<LeaderboardTableView.ScoreData> leaderboardTableScoreData = leaderboardData.ToScoreData();
                    int playerScoreIndex = GetPlayerScoreIndex(leaderboardData);
                    if (leaderboardTableScoreData.Count != 0) {
                        switch (scope) {
                            case PlatformLeaderboardsModel.ScoresScope.AroundPlayer when playerScoreIndex == -1:
                                SetErrorState(tableView, loadingControl, null, null,
                                    "You haven't set a score on this leaderboard");
                                break;
                            default: {
                                tableView.SetScores(leaderboardTableScoreData, playerScoreIndex);
                                loadingControl.ShowText("", false);
                                loadingControl.Hide();
                                _infoButtons.UpdateInfoButtonState(leaderboardTableScoreData.Count);
                                switch (_uploadDaemon.uploading) {
                                    case true:
                                        _panelView.DismissPrompt();
                                        break;
                                }

                                break;
                            }
                        }
                    } else {
                        switch (leaderboardPage > 1) {
                            case true:
                                SetErrorState(tableView, loadingControl, null, null, "No scores on this page");
                                break;
                            default:
                                SetErrorState(tableView, loadingControl, null, null,
                                    "No scores on this leaderboard, be the first!");
                                break;
                        }
                    }
                }
            } catch (HttpErrorException httpError) {
                SetErrorState(tableView, loadingControl, httpError);
            } catch (Exception exception) {
                SetErrorState(tableView, loadingControl, null, exception);
            }
        }

        private void SetRankedStatus(LeaderboardInfo leaderboardInfo) {
            switch (leaderboardInfo.ranked) {
                case true: {
                    switch (leaderboardInfo.positiveModifiers) {
                        case true:
                            _panelView.SetRankedStatus("Ranked (DA = +0.02, GN +0.04)");
                            break;
                        default:
                            _panelView.SetRankedStatus("Ranked (modifiers disabled)");
                            break;
                    }

                    return;
                }
            }

            switch (leaderboardInfo.qualified) {
                case true:
                    _panelView.SetRankedStatus("Qualified");
                    return;
            }

            switch (leaderboardInfo.loved) {
                case true:
                    _panelView.SetRankedStatus("Loved");
                    return;
                default:
                    _panelView.SetRankedStatus("Unranked");
                    break;
            }
        }

        public int GetPlayerScoreIndex(LeaderboardMap leaderboardMap) {
            for (int i = 0; i < leaderboardMap.scores.Length; i++) {
                if (leaderboardMap.scores[i].score.leaderboardPlayerInfo.id ==
                    _playerService.localPlayerInfo.playerId) {
                    return i;
                }
            }

            return -1;
        }

        private void SetErrorState(LeaderboardTableView tableView, LoadingControl loadingControl,
            HttpErrorException httpErrorException = null, Exception exception = null,
            string errorText = "Failed to load leaderboard, score won't upload", bool showRefreshButton = true) {
            if (httpErrorException != null) {
                switch (httpErrorException.isNetworkError) {
                    case true:
                        errorText = "Failed to load leaderboard due to a network error, score won't upload";
                        _leaderboardService.currentLoadedLeaderboard = null;
                        break;
                }

                switch (httpErrorException.isScoreSaberError) {
                    case true: {
                        errorText = httpErrorException.scoreSaberError.errorMessage;
                        switch (errorText) {
                            case "Leaderboard not found":
                                _leaderboardService.currentLoadedLeaderboard = null;
                                _panelView.SetRankedStatus("");
                                break;
                        }

                        break;
                    }
                }
            }

            if (exception != null) {
                Plugin.Log.Error(exception.ToString());
            }

            loadingControl.Hide();
            loadingControl.ShowText(errorText, showRefreshButton);
            tableView.SetScores(new List<LeaderboardTableView.ScoreData>(), -1);
        }

        public void DirectionalButtonClicked(bool down) {
            switch (down) {
                case true:
                    leaderboardPage++;
                    break;
                default:
                    leaderboardPage--;
                    break;
            }

            RefreshLeaderboard();
            CheckPage();
        }

        public void ChangePageButtonsEnabledState(bool state) {
            switch (state) {
                case true: {
                    switch (leaderboardPage > 1) {
                        case true:
                            _upButton.interactable = state;
                            break;
                    }

                    _downButton.interactable = state;
                    break;
                }
                default:
                    _upButton.interactable = state;
                    _downButton.interactable = state;
                    break;
            }
        }

        public void CheckPage() {
            switch (leaderboardPage > 1) {
                case true:
                    _upButton.interactable = true;
                    break;
                default:
                    _upButton.interactable = false;
                    break;
            }
        }

        public void RefreshLeaderboard() {
            switch (activated) {
                case false:
                    return;
                default:
                    _platformLeaderboardViewController?.InvokeMethod<object, PlatformLeaderboardViewController>(
                        "Refresh", true,
                        true);
                    break;
            }
        }

        public void ChangeScope(bool filterAroundCountry) {
            switch (activated) {
                case true:
                    _filterAroundCountry = filterAroundCountry;
                    leaderboardPage = 1;
                    RefreshLeaderboard();
                    CheckPage();
                    break;
            }
        }

        private async Task StartReplay(ScoreMap score) {
            _parserParams.EmitEvent("close-modals");
            _replayDownloading = true;

            try {
                _panelView.SetPromptInfo("Downloading Replay...", true);
                byte[] replay = await _playerService.GetReplayData(score.parent.difficultyBeatmap,
                    score.parent.leaderboardInfo.id, score);
                _panelView.SetPromptInfo("Replay downloaded! Unpacking...", true);
                await _replayLoader.Load(replay, score.parent.difficultyBeatmap, score.gameplayModifiers,
                    score.score.leaderboardPlayerInfo.name);
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
            string ppv3ReplayPath =
 $@"{Settings.replayPath}\PPv3-{Shared.ReplaceInvalidChars(_currentPPv3ScoreData.level.level.songName)}-{levelId}-{(_currentPPv3ScoreData.level.difficulty.SerializedName())}.dat";
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
                string ppv3ReplayPath =
 $@"{Settings.replayPath}\PPv3-{Shared.ReplaceInvalidChars(_currentPPv3ScoreData.level.level.songName)}-{levelId}-{(_currentPPv3ScoreData.level.difficulty.SerializedName())}.dat";
              
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