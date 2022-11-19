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
using System.Reflection;
using System.Threading.Tasks;
using Tweening;
using UnityEngine;
using Zenject;

#endregion

namespace ScoreSaber.UI.Leaderboard {
    [HotReload]
    internal class PanelView : BSMLAutomaticViewController {
        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew =
            FieldAccessor<ImageView, float>.GetAccessor("_skew");

        internal static readonly FieldAccessor<ImageView, bool>.Accessor ImageGradient =
            FieldAccessor<ImageView, bool>.GetAccessor("_gradient");

        private Tween _activeDisableTween;
        private ImageView _background;
        private PlayerInfo _currentPlayerInfo;
        private Sprite _denyahSprite;
        private FloatingScreen _floatingScreen;

        private bool _gayMode;
        private bool _initialized;

        private bool _isDenyah;

        private bool _isWilliums;
        private PlatformLeaderboardViewController _platformLeaderboardViewController;

        private PlayerService _playerService;
        private CanvasGroup _promptCanvasGroup;

        private Color _scoreSaberBlue;
        private Gradient _theWilliamGradient;
        private float _theWilliamVal;
        private TimeTweeningManager _timeTweeningManager;

        public Action disabling;
        public Action rankingWasSelected;
        public Action statusWasSelected;

        internal bool isWilliums {
            get => _isWilliums;
            set {
                if (_isWilliums == value) { return; }

                _gayMode = value;
                switch (value) {
                    case false:
                        _background.color = _scoreSaberBlue;
                        break;
                }

                _isWilliums = value;
            }
        }

        internal bool isDenyah {
            get => _isDenyah;
            set {
                if (_isDenyah == value) { return; }

                if (_background == null) {
                    return;
                }

                switch (value) {
                    case false:
                        _background.color = _scoreSaberBlue;
                        return;
                }

                if (_denyahSprite == null) {
                    _denyahSprite = Utilities.LoadSpriteRaw(Utilities.GetResource(Assembly.GetExecutingAssembly(),
                        "ScoreSaber.Resources.bri-ish.png"));
                }

                _background.overrideSprite = _denyahSprite;
                _isDenyah = value;
            }
        }

        protected void Update() {
            switch (_initialized) {
                case true when _gayMode: {
                    _background.color = _theWilliamGradient.Evaluate(_theWilliamVal);
                    _theWilliamVal += Time.deltaTime * 0.1f;
                    switch (_theWilliamVal > 1f) {
                        case true:
                            _theWilliamVal = 0f;
                            break;
                    }

                    break;
                }
            }
        }

        protected void OnDisable() {
            disabling?.Invoke();
        }

        [Inject]
        protected void Construct(PlayerService playerService, TimeTweeningManager timeTweeningManager,
            PlatformLeaderboardViewController platformLeaderboardViewController) {
            _scoreSaberBlue = new Color(0f, 0.4705882f, 0.7254902f);
            _theWilliamGradient = new Gradient {
                mode = GradientMode.Blend,
                colorKeys = new[] {
                    new GradientColorKey(Color.red, 0f), new GradientColorKey(new Color(1f, 0.5f, 0f), 0.17f),
                    new GradientColorKey(Color.yellow, 0.34f), new GradientColorKey(Color.green, 0.51f),
                    new GradientColorKey(Color.blue, 0.68f), new GradientColorKey(new Color(0.5f, 0f, 0.5f), 0.85f),
                    new GradientColorKey(Color.red, 1.15f)
                }
            };
            _playerService = playerService;
            _timeTweeningManager = timeTweeningManager;
            _platformLeaderboardViewController = platformLeaderboardViewController;
            Plugin.Log.Debug("PanelView Setup!");
        }

        internal void Init() {
            _floatingScreen =
                FloatingScreen.CreateFloatingScreen(new Vector2(100f, 15f), false, Vector3.zero, Quaternion.identity);
            _floatingScreen =
                FloatingScreen.CreateFloatingScreen(new Vector2(100f, 25f), false, Vector3.zero, Quaternion.identity);
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

            // Part 3: Clean up the image views! (They're a little dirty...)
            ImageSkew(ref _scoresaberLogo) = 0.18f;
            _scoresaberLogo.SetVerticesDirty();
            ImageSkew(ref _separator) = 0.18f;
            _separator.SetVerticesDirty();

            switch (Plugin.Settings.hasClickedScoreSaberLogo) {
                case false:
                    BlinkLogo().RunTask();
                    break;
            }
        }

        [UIAction("clicked-logo")]
        protected void PressedLogo() {
            switch (Plugin.Settings.hasClickedScoreSaberLogo) {
                case false:
                    Plugin.Settings.hasClickedScoreSaberLogo = true;
                    _scoresaberLogoClickable.DefaultColor = Color.white;
                    Settings.SaveSettings(Plugin.Settings);
                    break;
            }

            switch (_playerService.loginStatus) {
                case PlayerService.LoginStatus.Success:
                    ScoreSaberFlowCoordinator.ShowMainFlowCoordinator();
                    break;
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
            Color selectedColor = new Color(0.60f, 0.80f, 1);
            while (!Plugin.Settings.hasClickedScoreSaberLogo) {
                if (_scoresaberLogoClickable.DefaultColor == Color.white) {
                    _scoresaberLogoClickable.DefaultColor = selectedColor;
                } else {
                    _scoresaberLogoClickable.DefaultColor = Color.white;
                }

                await Task.Delay(1000);
            }
        }

        public void Show() {
            switch (_initialized) {
                case false:
                    Init();
                    _initialized = true;
                    break;
            }

            _floatingScreen.SetRootViewController(this, AnimationType.In);
        }

        public void Hide() {
            _floatingScreen.SetRootViewController(null, AnimationType.Out);
        }

        public void SetGlobalRanking(string globalRanking, bool withPrefix = true) {
            switch (withPrefix) {
                case true:
                    globalLeaderboardRanking = $"<b><color=#FFDE1A>Global Ranking: </color></b>{globalRanking}";
                    break;
                default:
                    globalLeaderboardRanking = globalRanking;
                    break;
            }
        }

        public void SetRankedStatus(string rankedStatus) {
            leaderboardRankedStatus = rankedStatus;
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
                switch (Plugin.Settings.showStatusText) {
                    case false:
                        return;
                }

                if (_promptRoot == null) { return; }

                ;
                if (_timeTweeningManager == null) { return; }

                if (_promptText == null) { return; }

                const float tweenTime = 0.5f;
                _promptText.text = status ?? _promptText.text;
                bool dismissable = dismissTime != -1;
                _activeDisableTween = null;
                promptLoading = showLoadingIndicator;
                _promptText.text = status ?? _promptText.text;
                _timeTweeningManager.KillAllTweens(_promptRoot);


                switch (_promptRoot.gameObject.activeInHierarchy) {
                    case false:
                        _promptRoot.gameObject.SetActive(true);
                        _timeTweeningManager.AddTween(
                            new FloatTween(0f, 1f, ChangePromptState, tweenTime,
                                _gayMode ? EaseType.OutBounce : EaseType.InSine), _promptRoot);
                        break;
                }

                switch (_promptRoot.gameObject.activeInHierarchy) {
                    case true when dismissTime != -1:
                        DismissPrompt(dismissTime);
                        break;
                }
            } catch (Exception) {
            }
        }

        public void DismissPrompt(float dismissTime = 0f, float tweenTime = 0.5f) {
            switch (_promptRoot.gameObject.activeSelf) {
                case true: {
                    void Disable() { _promptRoot.gameObject.SetActive(false); }
                    ;

                    Tween endTween = _timeTweeningManager.AddTween(
                        new FloatTween(1f, 0f, ChangePromptState, tweenTime, EaseType.OutCubic, dismissTime),
                        _promptRoot);
                    endTween.onCompleted = Disable;
                    endTween.onKilled = delegate {
                        if (_activeDisableTween != null && _activeDisableTween == endTween) {
                            Disable();
                        }
                    };
                    break;
                }
            }
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

        public async Task RankUpdater() {
            await TaskEx.WaitUntil(() => _playerService.loginStatus == PlayerService.LoginStatus.Success);

            switch (_playerService.localPlayerInfo.playerId) {
                case PlayerIDs.Williums:
                    isWilliums = true;
                    break;
            }

            switch (_playerService.localPlayerInfo.playerId) {
                case PlayerIDs.Denyah:
                    isDenyah = true;
                    break;
            }

            while (true) {
                await UpdateRank();
                await Task.Delay(240000);
            }
        }

        public async Task UpdateRank() {
            try {
                Loaded(false);
                _currentPlayerInfo = await _playerService.GetPlayerInfo(_playerService.localPlayerInfo.playerId, false);
                switch (Plugin.Settings.showLocalPlayerRank) {
                    case true:
                        SetGlobalRanking(
                            $"#{$"{_currentPlayerInfo.rank:n0}"}<size=75%> (<color=#6772E5>{$"{_currentPlayerInfo.pp:n0}"}pp</color>)");
                        break;
                    default:
                        SetGlobalRanking("Hidden");
                        break;
                }

                Loaded(true);
            } catch (HttpErrorException ex) {
                switch (ex.isScoreSaberError) {
                    case true when ex.scoreSaberError.errorMessage == "Player not found":
                        SetGlobalRanking("Welcome to ScoreSaber! Set a score to create a profile", false);
                        break;
                    case true:
                        SetGlobalRanking($"Failed to load player ranking: {ex.scoreSaberError.errorMessage}", false);
                        break;
                    default:
                        SetGlobalRanking("", false);
                        SetPromptError("Failed to update local player ranking", false, 1.5f);
                        Plugin.Log.Error("Failed to update local player ranking " + ex);
                        break;
                }
            }

            Loaded(true);
        }

        #region BSML Components

        [UIComponent("scoresaber-logo")] protected ImageView _scoresaberLogo;

        [UIComponent("separator")] protected ImageView _separator;

        [UIComponent("scoresaber-logo")] protected readonly ClickableImage _scoresaberLogoClickable = null;

        [UIComponent("container")] protected readonly Backgroundable _container = null;

        [UIComponent("prompt-root")] protected readonly RectTransform _promptRoot = null;

        [UIComponent("prompt-text")] protected readonly CurvedTextMeshPro _promptText = null;

        [UIComponent("prompt-loader")] protected readonly ImageView _promptLoader = null;

        private string _globalLeaderboardRanking = "<b><color=#FFDE1A>Global Ranking: </color></b> Loading...";

        [UIValue("global-leaderboard-ranking")]
        protected string globalLeaderboardRanking {
            get => _globalLeaderboardRanking;
            set {
                _globalLeaderboardRanking = value;
                NotifyPropertyChanged();
            }
        }

        private string _leaderboardRankedStatus = "<b><color=#FFDE1A>Ranked Status:</color></b> Loading...";

        [UIValue("leaderboard-ranked-status")]
        protected string leaderboardRankedStatus {
            get => _leaderboardRankedStatus;
            set {
                _leaderboardRankedStatus = $"<b><color=#FFDE1A>Ranked Status:</color></b> {value}";
                NotifyPropertyChanged();
            }
        }

        private bool _isPromptLoading;

        [UIValue("prompt-loader-active")]
        protected bool promptLoading {
            get => _isPromptLoading;
            set {
                _isPromptLoading = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("is-loading")] protected bool isLoading => !isLoaded;

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
    }
}