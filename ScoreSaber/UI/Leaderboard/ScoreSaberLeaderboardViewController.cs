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
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Data.Wrappers;
using ScoreSaber.Core.ReplaySystem;
using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Core.Services;
using ScoreSaber.Core.Utils;
using ScoreSaber.Extensions;
using ScoreSaber.UI.Elements.Leaderboard;
using ScoreSaber.UI.Elements.Profile;
using ScoreSaber.UI.Leaderboard;
using ScoreSaber.UI.Main;
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
using static ScoreSaber.UI.Leaderboard.ScoreSaberLeaderboardViewController;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Button = UnityEngine.UI.Button;

namespace ScoreSaber.UI.Leaderboard {

    [HotReload(RelativePathToLayout = @"./ScoreSaberLeaderboardViewController.bsml")]
    [ViewDefinition("ScoreSaber.UI.Leaderboard.ScoreSaberLeaderboardViewController.bsml")]
    internal class ScoreSaberLeaderboardViewController : BSMLAutomaticViewController, INotifyLeaderboardSet, IInitializable {

        // TODO: Put both of these somewhere nicer?
#pragma warning disable CS0169 // The field 'ScoreSaberLeaderboardViewController.headerText' is never used
#pragma warning disable CS0649 // Field 'ScoreSaberLeaderboardViewController.myHeader' is never assigned to, and will always have its default value null

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
        private readonly Backgroundable myHeader;

        [UIComponent("headerText")]
        private readonly TextMeshProUGUI headerText;

        [UIObject("headerSTATIC")]
        private readonly GameObject headerSTATIC;

        [UIComponent("headerTextSTATIC")]
        private readonly TextMeshProUGUI headerTextSTATIC;

        [UIComponent("errorText")]
        private readonly TextMeshProUGUI _errorText;

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
        private readonly Button _upButton;

        [UIComponent("down_button")]
        private readonly Button _downButton;

        [UIObject("loadingLB")]
        private readonly GameObject loadingLB;

        [UIObject("starRatingBox")]
        private readonly GameObject starRatingBox;

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


        [Inject] private readonly PanelView _panelView;
        [Inject] private readonly SiraLog _log;
        [Inject] private readonly DiContainer _container;
        [Inject] private readonly IUploadDaemon _uploadDaemon;
        [Inject] private readonly ReplayLoader _replayLoader;
        [Inject] private readonly PlayerService _playerService;
        [Inject] private readonly LeaderboardService _leaderboardService;
        [Inject] private readonly PlayerDataModel _playerDataModel;
        [Inject] internal readonly PlatformLeaderboardViewController _platformLeaderboardViewController;
        [Inject] private readonly MaxScoreCache _maxScoreCache;
        [Inject] private readonly PlatformLeaderboardViewController _plvc;
        [Inject] private readonly BeatmapLevelsModel _beatmapLevelsModel;
        [Inject] private readonly TweeningService _tweeningService;

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
                    _panelView.SetPromptSuccess(status, false, 3f);
                    _panelView.RankUpdater().RunTask();
                    _ImageHolders.ForEach(holder => holder.Active(true));
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
        }

        private void SetPanelStatus(LeaderboardInfoMap leaderboardInfoMap = null) {
            bool fromCached = true;
            if(leaderboardInfoMap == null) {
                leaderboardInfoMap = _leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap;
                fromCached = false;
            }

            if (leaderboardInfoMap == null) {
                _tweeningService.LerpColor(_headerBackground, grey);
                headerTextSTATIC.text = "OST";
                _tweeningService.FadeText(headerTextSTATIC, true, 0.3f);
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
           
            if(!ranked && !qualified && !loved) {
                _tweeningService.LerpColor(_headerBackground, grey);
                headerTextSTATIC.text = "UNRANKED";
                if (!fromCached) {
                    _tweeningService.FadeText(headerTextSTATIC, true, 0.3f);
                }
            }

            if (ranked) {
                _tweeningService.LerpColor(_headerBackground, yellow);
            }

            if (qualified) {
                _tweeningService.LerpColor(_headerBackground, _scoreSaberBlue);
                headerTextSTATIC.text = "QUALIFIED";
                if (!fromCached) {
                    _tweeningService.FadeText(headerTextSTATIC, true, 0.3f);
                }
            }

            if (loved) {
                _tweeningService.LerpColor(_headerBackground, pink);
                headerTextSTATIC.text = "LOVED";
                if (!fromCached) {
                    _tweeningService.FadeText(headerTextSTATIC, true, 0.3f);
                }
            }
        }

        [UIAction("OpenLeaderboardPage")]
        internal void OpenLeaderboardPage() {
            Application.OpenURL($"https://scoresaber.com/leaderboard/{_leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo.id}");
        }

        [UIAction("SettingsClicked")]
        internal void OpenSettingsPage() => ScoreSaberSettingsFlowCoordinator.ShowSettingsFlowCoordinator();

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

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!base.isActiveAndEnabled) return;
            if (!_plvc) return;
            if (firstActivation) {
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
                _ImageHolders.ForEach(holder => holder.ClearSprite());
                activated = true;
                OnIconSelected(null, 0);
            }
            Transform header = _plvc.transform.Find("HeaderPanel");
            _plvc.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0, 0, 0, 0);
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            if (!_plvc || !_plvc.isActivated) return;
            _plvc.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            if (!_plvc.isActivated) return;
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

                if(_leaderboardService.GetLeaderboardInfoMapFromCache(beatmapKey) != null) {
                    SetPanelStatus(_leaderboardService.GetLeaderboardInfoMapFromCache(beatmapKey));
                    setPanelStatusFromCache = true;
                } else {
                    _tweeningService.LerpColor(_headerBackground, grey, 0.1f);
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

                if(!beatmapKey.levelId.Contains("custom_level_")) {
                    SetErrorState(tableView, ref loadingControl, null, null, "This is not a custom level", false);
                    ByeImages();
                    SetPanelStatus();
                    return;
                } 

                await Task.Delay(500); // Delay before doing anything to prevent leaderboard spam

                if (_currentLeaderboardRefreshId == refreshId) {
                    int maxMultipliedScore = await _maxScoreCache.GetMaxScore(beatmapLevel, beatmapKey);
                    LeaderboardMap leaderboardData = await _leaderboardService.GetLeaderboardData(maxMultipliedScore, beatmapLevel, beatmapKey, scope, leaderboardPage, _playerDataModel.playerData.playerSpecificSettings);

                    if (_currentLeaderboardRefreshId != refreshId) {
                        return; // we need to check this again, since some time may have passed due to waiting for leaderboard data
                    }
                    if (!setPanelStatusFromCache) {
                        SetPanelStatus();
                    }
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
                    PatchLeaderboardTableView(tableView);
                }
            } catch (HttpErrorException httpError) {
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

        private void SetErrorState(LeaderboardTableView tableView, ref GameObject loadingControl, HttpErrorException httpErrorException = null, Exception exception = null, string errorText = "Failed to load leaderboard, score won't upload", bool showRefreshButton = true) {
            try {
                SetClickersOff();
                if (httpErrorException != null) {
                    if (httpErrorException.isNetworkError) {
                        errorText = "Failed to load leaderboard due to a network error, score won't upload";
                        _leaderboardService.currentLoadedLeaderboard = null;
                    }
                    if (httpErrorException.isScoreSaberError) {
                        errorText = httpErrorException.scoreSaberError.errorMessage;
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
                _errorText.gameObject.SetActive(true);
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

        void PatchLeaderboardTableView(LeaderboardTableView tableView) {
            LeaderboardTableCell[] cells = tableView.GetComponentsInChildren<LeaderboardTableCell>();

            for (int i = 0; i < cells.Length; i++) {
                LeaderboardTableCell tableCell = cells[i];

                int cellIdx = tableCell.idx;

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
                    _tweeningService.FadeText(_playerNameText, true, 0.3f);
                }
            }

            _downButton.interactable = cells.Length >= 9;
        }




        public void Initialize() {
            _infoButtons = new EntryHolder();
            _scoreDetailView = new ScoreDetailView();
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

        internal static class BeatmapDataCache {
            internal static Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();
            private static int MaxSpriteCacheSize = 150;
            internal static Queue<string> spriteCacheQueue = new Queue<string>();
            internal static void MaintainSpriteCache() {
                while (cachedSprites.Count > MaxSpriteCacheSize) {
                    string oldestUrl = spriteCacheQueue.Dequeue();
                    cachedSprites.Remove(oldestUrl);
                    
                }
            }

            internal static void AddSpriteToCache(string url, Sprite sprite) {
                if (cachedSprites.ContainsKey(url)) {
                    return;
                }
                cachedSprites.Add(url, sprite);
                spriteCacheQueue.Enqueue(url);
            }
        }


        // probably a better place to put this
        public class CellClicker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
            public Action<int> onClick;
            public int index;
            public ImageView seperator;
            public Vector3 originalScale = new Vector3(1, 1, 1);
            private bool isScaled = false;

            private Color origColour = new Color(1, 1, 1, 1);
            private Color origColour0 = new Color(1, 1, 1, 0.2509804f);
            private Color origColour1 = new Color(1, 1, 1, 0);

            public bool clickable = false;
            private void Start() {
                seperator.transform.localScale = originalScale;
            }

            public void OnPointerClick(PointerEventData data) {
                if(!clickable) return;
                BeatSaberUI.BasicUIAudioManager.HandleButtonClickEvent();
                onClick(index);
            }

            public void OnPointerEnter(PointerEventData eventData) {
                if (!clickable) return;
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
                if(!clickable) return;
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