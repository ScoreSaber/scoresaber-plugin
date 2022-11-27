
#region

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using ScoreSaber.Core.Data.Internal;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Services;
using ScoreSaber.Extensions;
using ScoreSaber.UI.Main;
using System;
using System.Threading.Tasks;
using Tweening;
using UnityEngine;
using Zenject;

#endregion

namespace ScoreSaber.UI.Leaderboard {
    [HotReload]
    internal class PanelView : BSMLAutomaticViewController {

        #region BSML Components
        [UIComponent("scoresaber-logo")]
        protected ImageView _scoresaberLogo;

        [UIComponent("separator")]
        protected ImageView _separator;

        [UIComponent("scoresaber-logo")]
        protected readonly ClickableImage _scoresaberLogoClickable = null;

        [UIComponent("container")]
        protected readonly Backgroundable _container = null;

        [UIComponent("prompt-root")]
        protected readonly RectTransform _promptRoot = null;

        [UIComponent("prompt-text")]
        protected readonly CurvedTextMeshPro _promptText = null;

        [UIComponent("prompt-loader")]
        protected readonly ImageView _promptLoader = null;

        private string _globalLeaderboardRanking = "<b><color=#FFDE1A>Global Ranking: </color></b> Loading...";
        [UIValue("global-leaderboard-ranking")]
        protected string GlobalLeaderboardRanking {
            get => _globalLeaderboardRanking;
            set {
                _globalLeaderboardRanking = value;
                NotifyPropertyChanged();
            }
        }

        private string _leaderboardRankedStatus = "<b><color=#FFDE1A>Ranked Status:</color></b> Loading...";
        [UIValue("leaderboard-ranked-status")]
        protected string LeaderboardRankedStatus {
            get => _leaderboardRankedStatus;
            set {
                _leaderboardRankedStatus = $"<b><color=#FFDE1A>Ranked Status:</color></b> {value}";
                NotifyPropertyChanged();
            }
        }

        private bool _isPromptLoading;
        [UIValue("prompt-loader-active")]
        protected bool PromptLoading {
            get => _isPromptLoading;
            set {
                _isPromptLoading = value;
                NotifyPropertyChanged();
            }
        }

        // Can't rename _isLoaded without breaking LeaderboardCore
        [UIValue("is-loading")]
        protected bool isLoading => !isLoaded;
        private bool _isLoaded;
        [UIValue("is-loaded")]
        protected bool isLoaded {
            get => _isLoaded;
            set {
                _isLoaded = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(isLoading));
            }
        }
        #endregion

        private bool _initialized;
        private ImageView _background;
        private Tween _activeDisableTween;
        private PlayerInfo _currentPlayerInfo;
        private CanvasGroup _promptCanvasGroup;
        private FloatingScreen _floatingScreen;

        public Action disabling;
        public Action statusWasSelected;
        public Action rankingWasSelected;

        private PlayerService _playerService;
        private TimeTweeningManager _timeTweeningManager;
        private PlatformLeaderboardViewController _platformLeaderboardViewController;

        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
        internal static readonly FieldAccessor<ImageView, bool>.Accessor ImageGradient = FieldAccessor<ImageView, bool>.GetAccessor("_gradient");

        [Inject]
        protected void Construct(PlayerService playerService, TimeTweeningManager timeTweeningManager, PlatformLeaderboardViewController platformLeaderboardViewController) {

            _playerService = playerService;
            _timeTweeningManager = timeTweeningManager;
            _platformLeaderboardViewController = platformLeaderboardViewController;
            Plugin.Log.Debug("PanelView Setup!");
        }

        protected void OnDisable() {

            disabling?.Invoke();
        }

        internal void Init() {

            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100f, 15f), false, Vector3.zero, Quaternion.identity);
            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100f, 25f), false, Vector3.zero, Quaternion.identity);
            _floatingScreen.name = "ScoreSaberPanelScreen";

            _floatingScreen.transform.SetParent(_platformLeaderboardViewController.transform, false);
            _floatingScreen.transform.localPosition = new Vector3(3f, 50f);
            _floatingScreen.transform.localScale = Vector3.one;
            _floatingScreen.gameObject.SetActive(false);
            _floatingScreen.gameObject.SetActive(true);
        }


        [UIAction("#post-parse")]
        protected void Parsed() {

            _container.background.material = Utilities.ImageResources.NoGlowMat;
            _background = _container.background as ImageView;
            _scoresaberLogo.name = "ScoreSaberLogoImage";
            _separator.name = "Separator";

            _background.color0 = Color.white;
            _background.color1 = new Color(1f, 1f, 1f, 0f);
            ImageGradient(ref _background) = true;
            ImageSkew(ref _background) = 0.18f;
            _background.enabled = false;
            _background.enabled = true;

            _promptCanvasGroup = _promptRoot.gameObject.AddComponent<CanvasGroup>();
            _promptCanvasGroup.gameObject.SetActive(false);
            _promptCanvasGroup.alpha = 0f;

            ImageSkew(ref _scoresaberLogo) = 0.18f;
            _scoresaberLogo.SetVerticesDirty();
            ImageSkew(ref _separator) = 0.18f;
            _separator.SetVerticesDirty();

            if (!Plugin.Settings.HasClickedScoreSaberLogo) {
                BlinkLogo().RunTask();
            }
        }

        [UIAction("clicked-logo")]
        protected void PressedLogo() {

            if (!Plugin.Settings.HasClickedScoreSaberLogo) {
                Plugin.Settings.HasClickedScoreSaberLogo = true;
                _scoresaberLogoClickable.DefaultColor = Color.white;
                Settings.SaveSettings(Plugin.Settings);
            }
            if (_playerService.Status == PlayerService.LoginStatus.Success) {
                ScoreSaberFlowCoordinator.ShowMainFlowCoordinator();
            }
        }

        [UIAction("clicked-ranking")]
        protected void ClickedRanking() {

            if (_currentPlayerInfo != null) {
                rankingWasSelected?.Invoke();
            }
        }

        [UIAction("clicked-status")]
        protected void ClickedStatus() {

            statusWasSelected?.Invoke();
        }

        private async Task BlinkLogo() {

            var selectedColor = new Color(0.60f, 0.80f, 1);
            while (!Plugin.Settings.HasClickedScoreSaberLogo) {
                _scoresaberLogoClickable.DefaultColor = _scoresaberLogoClickable.DefaultColor == Color.white
                    ? selectedColor
                    : Color.white;
                await Task.Delay(1000);
            }
        }

        public void Show() {

            if (!_initialized) {
                Init();
                _initialized = true;
            }

            _floatingScreen.SetRootViewController(this, AnimationType.In);
        }

        public void Hide() {

            _floatingScreen.SetRootViewController(null, AnimationType.Out);
        }

        public void SetGlobalRanking(string globalRanking, bool withPrefix = true) {

            GlobalLeaderboardRanking =
                withPrefix ? $"<b><color=#FFDE1A>Global Ranking: </color></b>{globalRanking}" : globalRanking;
        }

        public void SetRankedStatus(string rankedStatus) {

            LeaderboardRankedStatus = rankedStatus;
        }

        public void SetPromptInfo(string status, bool showLoadingIndicator, float dismissTime = -1f) {

            SetPrompt(status, showLoadingIndicator, dismissTime);
        }

        public void SetPromptError(string status, bool showLoadingIndicator, float dismissTime = -1f) {

            status = $"<color=#fc8181>{status}</color>";
            SetPrompt(status, showLoadingIndicator, dismissTime);
        }

        public void SetPromptSuccess(string status, bool showLoadingIndicator, float dismissTime = -1f) {

            status = $"<color=#89fc81>{status}</color>";
            SetPrompt(status, showLoadingIndicator, dismissTime);
        }

        public void SetPrompt(string status, bool showLoadingIndicator, float dismissTime = -1f) {

            try {
                if (!Plugin.Settings.ShowStatusText) { return; }
                if (_promptRoot == null) { return; };
                if (_timeTweeningManager == null) { return; }
                if (_promptText == null) { return; }

                const float tweenTime = 0.5f;
                _promptText.text = status ?? _promptText.text;
                bool dismissable = dismissTime != -1;
                _activeDisableTween = null;
                PromptLoading = showLoadingIndicator;
                _promptText.text = status ?? _promptText.text;
                _timeTweeningManager.KillAllTweens(_promptRoot);


                if (!_promptRoot.gameObject.activeInHierarchy) {
                    _promptRoot.gameObject.SetActive(true);
                    _timeTweeningManager.AddTween(new FloatTween(0f, 1f, ChangePromptState, tweenTime, EaseType.InSine), _promptRoot);
                }

                if (_promptRoot.gameObject.activeInHierarchy && Math.Abs(dismissTime - (-1)) > 0.0001) {
                    DismissPrompt(dismissTime);
                }

            } catch (Exception) {
                // ignored
            }
        }

        public void DismissPrompt(float dismissTime = 0f, float tweenTime = 0.5f) {

            if (!_promptRoot.gameObject.activeSelf) {
                return;
            }

            void Disable() { _promptRoot.gameObject.SetActive(false); };

            var endTween = _timeTweeningManager.AddTween(new FloatTween(1f, 0f, ChangePromptState, tweenTime, EaseType.OutCubic, dismissTime), _promptRoot);
            endTween.onCompleted = Disable;
            endTween.onKilled = delegate {
                if (_activeDisableTween != null && _activeDisableTween == endTween) {
                    Disable();
                }
            };
        }

        private void ChangePromptState(float value) {

            const float fullY = 10.30f;
            const float hiddenY = 4.30f;
            _promptCanvasGroup.alpha = value;
            _promptRoot.gameObject.SetActive(true);
            _promptRoot.transform.localPosition = new Vector3(0f, Mathf.Lerp(hiddenY, fullY, value), 0f);
        }

        public void Loaded(bool value) {

            isLoaded = value;
        }

        protected void Update() {
            // Remove?
        }

        public async Task RankUpdater() {

            await TaskEx.WaitUntil(() => _playerService.Status == PlayerService.LoginStatus.Success);

            while (true) {
                await UpdateRank();
                await Task.Delay(240000);
            }
        }

        public async Task UpdateRank() {

            try {
                Loaded(false);
                _currentPlayerInfo = await _playerService.GetPlayerInfo(_playerService.LocalPlayerInfo.PlayerId, full: false);
                SetGlobalRanking(Plugin.Settings.ShowLocalPlayerRank
                    ? $"#{_currentPlayerInfo.Rank:n0}<size=75%> (<color=#6772E5>{_currentPlayerInfo.PP:n0}pp</color>)"
                    : "Hidden");
                Loaded(true);
            } catch (HttpErrorException ex) {
                if (ex.IsScoreSaberError) {
                    SetGlobalRanking(
                        ex.ScoreSaberError.ErrorMessage == "Player not found"
                            ? "Welcome to ScoreSaber! Set a score to create a profile"
                            : $"Failed to load player ranking: {ex.ScoreSaberError.ErrorMessage}", false);
                } else {
                    SetGlobalRanking("", false);
                    SetPromptError("Failed to update local player ranking", false, 1.5f);
                    Plugin.Log.Error("Failed to update local player ranking " + ex);
                }
            }
            Loaded(true);
        }
    }
}