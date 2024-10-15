using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using ScoreSaber.Core.Data;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Services;
using ScoreSaber.Extensions;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Tweening;
using UnityEngine;
using Zenject;
using ScoreSaber.UI.Main;

namespace ScoreSaber.UI.Leaderboard {
    [HotReload]
    internal class PanelView : BSMLAutomaticViewController {

        #region BSML Components
        [UIComponent("scoresaber-logo")]
        protected ImageView _scoresaberLogo = null;

        [UIComponent("separator")]
        protected ImageView _separator = null;

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

        private bool _isPromptLoading = false;
        [UIValue("prompt-loader-active")]
        protected bool promptLoading {
            get => _isPromptLoading;
            set {
                _isPromptLoading = value;
                NotifyPropertyChanged(nameof(promptLoading));
            }
        }

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

        private bool _gayMode;
        private float _theWilliamVal;
        private Sprite _denyahSprite;
        private ImageView _background;
        private Tween _activeDisableTween;
        internal PlayerInfo _currentPlayerInfo;
        private CanvasGroup _promptCanvasGroup;

        public Action statusWasSelected;
        public Action rankingWasSelected;

        private PlayerService _playerService = null;
        private TimeTweeningManager _timeTweeningManager = null;

        private Color _scoreSaberBlue;
        private Gradient _theWilliamGradient;
        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
        internal static readonly FieldAccessor<ImageView, bool>.Accessor ImageGradient = FieldAccessor<ImageView, bool>.GetAccessor("_gradient");

        private bool _isWilliums;
        internal bool isWilliums {
            get { return _isWilliums; }
            set {
                if (_isWilliums == value) { return; }
                _gayMode = value;
                if (!value) { _background.color = _scoreSaberBlue; }
                _isWilliums = value;
            }
        }

        private bool _isDenyah;
        internal bool isDenyah {
            get { return _isDenyah; }
            set {
                if (_isDenyah == value) { return; }

                if (_background == null) return;
                if (!value) {
                    _background.color = _scoreSaberBlue;
                    return;
                }
                if (_denyahSprite == null) {
#pragma warning disable CS0618 // Type or member is obsolete
                    _denyahSprite = Utilities.LoadSpriteRaw(Utilities.GetResource(Assembly.GetExecutingAssembly(), "ScoreSaber.Resources.bri-ish.png"));
#pragma warning restore CS0618 // Type or member is obsolete
                }
                _background.overrideSprite = _denyahSprite;
                _isDenyah = value;
            }
        }

        [Inject]
        protected void Construct(PlayerService playerService, TimeTweeningManager timeTweeningManager) {
            _scoreSaberBlue = new Color(0f, 0.4705882f, 0.7254902f);
            _theWilliamGradient = new Gradient { mode = GradientMode.Blend, colorKeys = new GradientColorKey[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(new Color(1f, 0.5f, 0f), 0.17f), new GradientColorKey(Color.yellow, 0.34f), new GradientColorKey(Color.green, 0.51f), new GradientColorKey(Color.blue, 0.68f), new GradientColorKey(new Color(0.5f, 0f, 0.5f), 0.85f), new GradientColorKey(Color.red, 1.15f) } };
            _playerService = playerService;
            _timeTweeningManager = timeTweeningManager;
            Plugin.Log.Debug("PanelView Setup!");
        }

        [UIAction("#post-parse")]
        protected void Parsed() {

            _container.Background.material = Utilities.ImageResources.NoGlowMat;
            _background = _container.Background as ImageView;
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

            if (!Plugin.Settings.hasClickedScoreSaberLogo) {
                BlinkLogo().RunTask();
            }
        }

        [UIAction("clicked-logo")]
        protected void PressedLogo() {

            if (!Plugin.Settings.hasClickedScoreSaberLogo) {
                Plugin.Settings.hasClickedScoreSaberLogo = true;
                _scoresaberLogoClickable.DefaultColor = Color.white;
                Settings.SaveSettings(Plugin.Settings);
            }
            if (_playerService.loginStatus == PlayerService.LoginStatus.Success) {
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
            while (!Plugin.Settings.hasClickedScoreSaberLogo) {
                if (_scoresaberLogoClickable.DefaultColor == Color.white) {
                    _scoresaberLogoClickable.DefaultColor = selectedColor;
                } else {
                    _scoresaberLogoClickable.DefaultColor = Color.white;
                }
                await Task.Delay(1000);
            }
        }

        public void SetGlobalRanking(string globalRanking, bool withPrefix = true) {

            if (withPrefix) {
                globalLeaderboardRanking = $"<b><color=#FFDE1A>Global Ranking: </color></b>{globalRanking}";
            } else {
                globalLeaderboardRanking = globalRanking;
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
                if (!Plugin.Settings.showStatusText) { return; }
                if (_promptRoot == null) { return; };
                if (_timeTweeningManager == null) { return; }
                if (_promptText == null) { return; }

                const float tweenTime = 0.5f;
                _promptText.text = status ?? _promptText.text;
                bool dismissable = dismissTime != -1;
                _activeDisableTween = null;
                promptLoading = showLoadingIndicator;
                _promptText.text = status ?? _promptText.text;
                _timeTweeningManager.KillAllTweens(_promptRoot);


                if (!_promptRoot.gameObject.activeInHierarchy) {
                    _promptRoot.gameObject.SetActive(true);
                    _timeTweeningManager.AddTween(new FloatTween(0f, 1f, ChangePromptState, tweenTime, _gayMode ? EaseType.OutBounce : EaseType.InSine), _promptRoot);
                }

                if (_promptRoot.gameObject.activeInHierarchy && dismissTime != -1) {
                    DismissPrompt(dismissTime);
                }

            } catch (Exception) {
            }
        }

        public void DismissPrompt(float dismissTime = 0f, float tweenTime = 0.5f) {

            if (_promptRoot.gameObject.activeSelf) {
                void Disable() { _promptRoot.gameObject.SetActive(false); };

                var endTween = _timeTweeningManager.AddTween(new FloatTween(1f, 0f, ChangePromptState, tweenTime, EaseType.OutCubic, dismissTime), _promptRoot);
                endTween.onCompleted = Disable;
                endTween.onKilled = delegate () {
                    if (_activeDisableTween != null && _activeDisableTween == endTween) {
                        Disable();
                    }
                };
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

        protected void Update() {

            if (_gayMode) {
                _background.color = _theWilliamGradient.Evaluate(_theWilliamVal);
                _theWilliamVal += Time.deltaTime * 0.1f;
                if (_theWilliamVal > 1f) _theWilliamVal = 0f;
            }
        }

        public async Task RankUpdater() {

            await TaskEx.WaitUntil(() => _playerService.loginStatus == PlayerService.LoginStatus.Success);

            if (_playerService.localPlayerInfo.playerId == PlayerIDs.Williums) {
                isWilliums = true;
            }

            if (_playerService.localPlayerInfo.playerId == PlayerIDs.Denyah) {
                isDenyah = true;
            }

            while (true) {
                await UpdateRank();
                await Task.Delay(240000);
            }
        }

        public async Task UpdateRank() {

            try {
                Loaded(false);
                _currentPlayerInfo = await _playerService.GetPlayerInfo(_playerService.localPlayerInfo.playerId, full: false);
                if (Plugin.Settings.showLocalPlayerRank) {
                    SetGlobalRanking($"#{string.Format("{0:n0}", _currentPlayerInfo.rank)}<size=75%> (<color=#6772E5>{string.Format("{0:n0}", _currentPlayerInfo.pp)}pp</color>)");
                } else {
                    SetGlobalRanking("Hidden");
                }
                Loaded(true);
            } catch (HttpErrorException ex) {
                if (ex.isScoreSaberError) {
                    if (ex.scoreSaberError.errorMessage == "Player not found") {
                        SetGlobalRanking("Welcome to ScoreSaber! Set a score to create a profile", false);
                    } else {
                        SetGlobalRanking($"Failed to load player ranking: {ex.scoreSaberError.errorMessage}", false);
                    }
                } else {
                    SetGlobalRanking("", false);
                    SetPromptError("Failed to update local player ranking", false, 1.5f);
                    Plugin.Log.Error("Failed to update local player ranking " + ex.ToString());
                }
            }
            Loaded(true);
        }
    }
}