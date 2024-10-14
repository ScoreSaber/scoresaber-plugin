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
using ScoreSaber.Core.ReplaySystem.Data;
using System.Threading;
using BeatSaberMarkupLanguage.ViewControllers;
using LeaderboardCore.Interfaces;
using static ScoreSaber.Patches.LeaderboardPatchesREMOVE;
using TMPro;
using HMUI;
using SiraUtil.Affinity;
using static HMUI.IconSegmentedControl;
using System.Linq;
using System.Collections;
using UnityEngine.EventSystems;
using BeatSaberMarkupLanguage.Components;
using UnityEngine.Diagnostics;

namespace ScoreSaber.UI.Leaderboard {

    internal class ScoreSaberLeaderboardViewController : BSMLAutomaticViewController, IInitializable, IDisposable, INotifyLeaderboardSet {

        public enum ScoreSaberScoresScope {
            Global,
            AroundPlayer,
            Friends,
            Area
        }

        ScoreSaberScoresScope scoreSaberScoresScope;

        internal BeatmapKey currentBeatmapKey;

        #region BSML Components
        [UIParams]
        private readonly BSMLParserParams _parserParams = null;

        [UIComponent("leaderboardTableView")]
        internal readonly LeaderboardTableView leaderboardTableView = null;

        [UIComponent("leaderboardTableView")]
        internal readonly Transform leaderboardTransform = null;

        [UIComponent("myHeader")]
        private readonly Backgroundable myHeader = null;

        [UIComponent("errorText")]
        private readonly TextMeshProUGUI errorText = null;

        [UIValue("imageHolders")]
        internal List<ProfilePictureView> _ImageHolders = null;
        [UIValue("cellClickerHolders")]
        internal List<CellClickingView> _cellClickingHolders = null;
        [UIValue("entry-holder")]
        internal readonly EntryHolder _infoButtons = null;
        [UIValue("score-detail-view")]
        protected readonly ScoreDetailView _scoreDetailView = null;
        [UIComponent("profile-detail-view")]
        protected readonly ProfileDetailView _profileDetailView = null;

        [UIAction("up-button-click")] private void UpButtonClicked() => DirectionalButtonClicked(false);
        [UIAction("down-button-click")] private void DownButtonClicked() => DirectionalButtonClicked(true);

        [UIComponent("up_button")]
        internal readonly Button _upButton = null;

        [UIComponent("down_button")]
        internal readonly Button _downButton = null;

        [UIObject("loadingLB")]
        private readonly GameObject _loadingControl = null;

        [UIAction("downloadPlaylistCLICK")]
        private void downloadPlaylistCLICK() {
            Application.OpenURL($"https://fortnite.com");
        }

        [UIValue("leaderboardIcons")]
        private List<IconSegmentedControl.DataItem> leaderboardIcons {
            get {
#pragma warning disable CS0618 // Type or member is obsolete
                return new IconSegmentedControl.DataItem[] {
                    new IconSegmentedControl.DataItem(Utilities.FindSpriteInAssembly("ScoreSaber.Resources.globe.png"), "Global"),
                    new IconSegmentedControl.DataItem(Utilities.FindSpriteInAssembly("ScoreSaber.Resources.Player.png"), "Around You"),
                    new IconSegmentedControl.DataItem(Utilities.FindSpriteInAssembly("ScoreSaber.Resources.Player.png"), "Friends"),
                    new IconSegmentedControl.DataItem(Utilities.FindSpriteInAssembly("ScoreSaber.Resources.country.png"), "Area"),
                }.ToList();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        [UIAction("OnIconSelected")]
        private void OnIconSelected(SegmentedControl segmentedControl, int index) {
            scoreSaberScoresScope = (ScoreSaberScoresScope)index;
            OnLeaderboardSet(currentBeatmapKey);
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
        internal readonly PlatformLeaderboardViewController _platformLeaderboardViewController;
        private readonly MaxScoreCache _maxScoreCache;
        private readonly BeatmapLevelsModel _beatmapLevelsModel;

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
                    ByeImages();
                    _panelView.SetRankedStatus("N/A (Not Custom Song)");
                }
                _isOST = value;
            }
        }

        public ScoreSaberLeaderboardViewController(DiContainer container, PanelView panelView, IUploadDaemon uploadDaemon, ReplayLoader replayLoader, PlayerService playerService, LeaderboardService leaderboardService, PlayerDataModel playerDataModel, PlatformLeaderboardViewController platformLeaderboardViewController, List<ProfilePictureView> profilePictureView, List<CellClickingView> cellClickingViews, MaxScoreCache maxScoreCache, BeatmapLevelsModel beatmapLevelsModel) {

            _container = container;
            _panelView = panelView;
            _uploadDaemon = uploadDaemon;
            _replayLoader = replayLoader;
            _playerService = playerService;
            _playerDataModel = playerDataModel;
            _leaderboardService = leaderboardService;
            _ImageHolders = profilePictureView;
            _cellClickingHolders = cellClickingViews;
            _platformLeaderboardViewController = platformLeaderboardViewController;
            _maxScoreCache = maxScoreCache;

            _infoButtons = new EntryHolder();
            _scoreDetailView = new ScoreDetailView();

            _beatmapLevelsModel = beatmapLevelsModel;
        }

        public void Initialize() {

            _scoreDetailView.showProfile += scoreDetailView_showProfile;
            _scoreDetailView.startReplay += scoreDetailView_startReplay;
            _playerService.LoginStatusChanged += playerService_LoginStatusChanged;
            _infoButtons.infoButtonClicked += infoButtons_infoButtonClicked;
            _uploadDaemon.UploadStatusChanged += uploadDaemon_UploadStatusChanged;
        }

        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
        internal static readonly FieldAccessor<ImageView, bool>.Accessor ImageGradient = FieldAccessor<ImageView, bool>.GetAccessor("_gradient");

        [UIAction("#post-parse")]
        public void Parsed() {
            //_upButton.transform.localScale *= .5f;
            //_downButton.transform.localScale *= .5f;
            myHeader.Background.material = Resources.FindObjectsOfTypeAll<Material>().Where(m => m.name == "UINoGlowRoundEdge").First();
            ByeImages();
            errorText.gameObject.SetActive(false);

            var _loadingControlA = leaderboardTransform.Find("LoadingControl").gameObject;
            Transform loadingContainer = _loadingControlA.transform.Find("LoadingContainer");
            loadingContainer.gameObject.SetActive(false);
            Destroy(loadingContainer.Find("Text").gameObject);
            Destroy(_loadingControlA.transform.Find("RefreshContainer").gameObject);
            Destroy(_loadingControlA.transform.Find("DownloadingContainer").gameObject);

            var _imgView = myHeader.Background as ImageView;
            Color color = new Color(255f / 255f, 222f / 255f, 24f / 255f);
            _imgView.color = color;
            _imgView.color0 = color;
            _imgView.color1 = color;
            ImageSkew(ref _imgView) = 0.18f;
            ImageGradient(ref _imgView) = true;

            activated = true;
        }

        public void AllowReplayWatching(bool value) {

            _scoreDetailView.AllowReplayWatching(value);
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {

            if (!firstActivation) { return; }

            _panelView.statusWasSelected = delegate () {
                if (_leaderboardService.currentLoadedLeaderboard == null) { return; }
                _parserParams.EmitEvent("close-modals");
                Application.OpenURL($"https://scoresaber.com/leaderboard/{_leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo.id}");
            };

            _panelView.rankingWasSelected = delegate () {
                _parserParams.EmitEvent("close-modals");
                _parserParams.EmitEvent("show-profile");
                //_profileDetailView.ShowProfile(_playerService.localPlayerInfo.playerId).RunTask();
            };

            _container.Inject(_profileDetailView);
            _playerService.GetLocalPlayerInfo();
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            if (_scoreDetailView.detailModalRoot != null) _scoreDetailView.detailModalRoot.Hide(false);
            if (_profileDetailView.profileModalRoot != null) _profileDetailView.profileModalRoot.Hide(false);
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
                case PlayerService.LoginStatus.InProgress:
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

        private CancellationTokenSource cancellationToken;

        public async Task RefreshLeaderboard(BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, LeaderboardTableView tableView, ScoreSaberScoresScope scope, LoadingControl loadingControl, string refreshId) {

            try {

                _currentLeaderboardRefreshId = refreshId;
                if (_uploadDaemon.uploading) { return; }
                if (!activated) { return; }

                if (scope == ScoreSaberScoresScope.AroundPlayer) {
                    _upButton.interactable = false;
                    _downButton.interactable = false;
                } else {
                    _upButton.interactable = true;
                    _downButton.interactable = true;
                }

                ByeImages();

                if(cancellationToken != null) {
                    cancellationToken.Cancel();
                    cancellationToken.Dispose();
                }
                cancellationToken = new CancellationTokenSource();

                if (_playerService.loginStatus == PlayerService.LoginStatus.Error) {
                    SetErrorState(tableView, loadingControl, null, null, "ScoreSaber authentication failed, please restart Beat Saber", false);
                    ByeImages();
                    return;
                }

                if (_playerService.loginStatus != PlayerService.LoginStatus.Success) {
                    return;
                }


                await Task.Delay(500); // Delay before doing anything to prevent leaderboard spam


                if (_currentLeaderboardRefreshId == refreshId) {
                    int maxMultipliedScore = await _maxScoreCache.GetMaxScore(beatmapLevel, beatmapKey);
                    LeaderboardMap leaderboardData = await _leaderboardService.GetLeaderboardData(maxMultipliedScore, beatmapLevel, beatmapKey, scope, leaderboardPage, _playerDataModel.playerData.playerSpecificSettings, _filterAroundCountry);
                    if (_currentLeaderboardRefreshId != refreshId) {
                        return; // we need to check this again, since some time may have passed due to waiting for leaderboard data
                    }
                    SetRankedStatus(leaderboardData.leaderboardInfoMap.leaderboardInfo);
                    List<LeaderboardTableView.ScoreData> leaderboardTableScoreData = leaderboardData.ToScoreData();
                    int playerScoreIndex = GetPlayerScoreIndex(leaderboardData);
                    if (leaderboardTableScoreData.Count != 0) {
                        if (scope == ScoreSaberScoresScope.AroundPlayer && playerScoreIndex == -1 && !_filterAroundCountry) {
                            SetErrorState(tableView, loadingControl, null, null, "You haven't set a score on this leaderboard");
                        } else {
                            tableView.SetScores(leaderboardTableScoreData, playerScoreIndex);
                            RichMyText(tableView);
                            for (int i = 0; i < leaderboardTableScoreData.Count; i++) {
                                _ImageHolders[i].setProfileImage(leaderboardData.scores[i].score.leaderboardPlayerInfo.profilePicture, i, cancellationToken.Token);
                            }
                            loadingControl.ShowText("", false);
                            loadingControl.Hide();
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
                        ByeImages();
                    }
                }
            } catch (HttpErrorException httpError) {
                SetErrorState(tableView, loadingControl, httpError);
            } catch (Exception exception) {
                SetErrorState(tableView, loadingControl, null, exception);
            }
        }

        private bool obtainedAnchor = false;
        private Vector2 normalAnchor = Vector2.zero;

        void RichMyText(LeaderboardTableView tableView) {
            int i = 0;
            foreach (LeaderboardTableCell tableCell in tableView.GetComponentsInChildren<LeaderboardTableCell>()) {

                CellClicker cellClicker = _cellClickingHolders[i].cellClickerImage.gameObject.AddComponent<CellClicker>();
                cellClicker.onClick = _infoButtons.InfoButtonClicked;
                cellClicker.index = i;
                cellClicker.seperator = tableCell.GetField<Image, LeaderboardTableCell>("_separatorImage") as ImageView;

                TextMeshProUGUI _playerNameText = tableCell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText");

                if (!obtainedAnchor) {
                    normalAnchor = _playerNameText.rectTransform.anchoredPosition;
                    obtainedAnchor = true;
                }

                if (isOST) {
                    _playerNameText.richText = false;
                    _playerNameText.rectTransform.anchoredPosition = normalAnchor;
                    tableCell.showSeparator = i != tableView._scores.Count - 1;
                } else {
                    _playerNameText.richText = true;
                    Vector2 newPosition = new Vector2(normalAnchor.x + 2.5f, 0f);
                    _playerNameText.rectTransform.anchoredPosition = newPosition;
                    tableCell.showSeparator = true;
                }
                i++;
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
            ByeImages();
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

            OnLeaderboardSet(currentBeatmapKey);
        }

        public void ChangeScope(bool filterAroundCountry) {

            if (activated) {
                _filterAroundCountry = filterAroundCountry;
                leaderboardPage = 1;
                RefreshLeaderboard();
                CheckPage();
            }
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

        public void Dispose() {

            _playerService.LoginStatusChanged -= playerService_LoginStatusChanged;
            _uploadDaemon.UploadStatusChanged -= uploadDaemon_UploadStatusChanged;
            _infoButtons.infoButtonClicked -= infoButtons_infoButtonClicked;
            _scoreDetailView.startReplay -= scoreDetailView_startReplay;
            _scoreDetailView.showProfile -= scoreDetailView_showProfile;
        }

        public void OnLeaderboardSet(BeatmapKey beatmapKey) {
            currentBeatmapKey = beatmapKey;
            // clean up cell clickers
            foreach (var holder in _cellClickingHolders) {
                CellClicker existingCellClicker = holder?.cellClickerImage?.gameObject?.GetComponent<CellClicker>();
                if (existingCellClicker != null) {
                    GameObject.Destroy(existingCellClicker);
                }
            }

            if (beatmapKey.levelId.StartsWith("custom_level_")) {
                _loadingControl.gameObject.SetActive(true);

                isOST = false;
                BeatmapLevel beatmapLevel = _beatmapLevelsModel.GetBeatmapLevel(beatmapKey.levelId);
                RefreshLeaderboard(beatmapLevel, beatmapKey, leaderboardTableView, scoreSaberScoresScope, null, Guid.NewGuid().ToString()).RunTask();
            } else {
                isOST = true;
            }
        }

        // probably a better place to put this
        public class CellClicker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
            public Action<int> onClick;
            public int index;
            public ImageView seperator;
            private Vector3 originalScale;
            private bool isScaled = false;

            private Color origColour = new Color(1, 1, 1, 1);
            private Color origColour0 = new Color(1, 1, 1, 0.2509804f);
            private Color origColour1 = new Color(1, 1, 1, 0);

            private void Start() {
                originalScale = seperator.transform.localScale;
            }

            public void OnPointerClick(PointerEventData data) {
                BeatSaberUI.BasicUIAudioManager.HandleButtonClickEvent();
                onClick(index);
            }

            public void OnPointerEnter(PointerEventData eventData) {
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