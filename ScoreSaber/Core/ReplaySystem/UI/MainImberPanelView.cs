#region

using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.UI {
    [HotReload(RelativePathToLayout = @"imber-panel.bsml")]
    [ViewDefinition("ScoreSaber.Core.ReplaySystem.UI.imber-panel.bsml")]
    internal class MainImberPanelView : BSMLAutomaticViewController {
        private static readonly Color _goodColor = Color.green;
        private static readonly Color _ehColor = Color.yellow;
        private static readonly Color _noColor = Color.red;

        [UIComponent("fps-text")] protected readonly CurvedTextMeshPro fpsText = null;

        [UIComponent("left-speed-text")] protected readonly CurvedTextMeshPro leftSpeedText = null;

        [UIValue("locations")] protected readonly List<object> locations = new List<object>();

        [UIComponent("right-speed-text")] protected readonly CurvedTextMeshPro rightSpeedText = null;

        [UIComponent("tab-selector")] protected readonly TabSelector tabSelector = null;

        private FloatingScreen _floatingScreen;
        private float _initialTime = 1f;

        private int _lastTab;

        private string _location = "";

        private string _loopText = "LOOP";

        private string _playPauseText = "PAUSE";
        private int _targetFPS = 90;

        private float _timeSync;

        public bool didParse { get; private set; }

        public Transform Transform => _floatingScreen.transform;

        public Pose defaultPosition { get; set; }

        [UIValue("time-sync")]
        public float timeSync {
            get => _timeSync;
            set {
                _timeSync = Mathf.Approximately(_initialTime, value) ? _initialTime : value;
                DidTimeSyncChange?.Invoke(_timeSync);
            }
        }

        public bool visibility {
            set => _floatingScreen.SetRootViewController(value ? this : null,
                value ? AnimationType.In : AnimationType.Out);
        }

        [UIValue("play-pause-text")]
        public string playPauseText {
            get => _playPauseText;
            set {
                _playPauseText = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("loop-text")]
        public string loopText {
            get => _loopText;
            set {
                _loopText = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("location")]
        public string location {
            get => _location;
            protected set {
                _location = value;
                DidPositionPreviewChange?.Invoke(_location);
            }
        }

        public int fps {
            set {
                fpsText.text = value.ToString();
                switch (value > 0.85f * _targetFPS) {
                    case true:
                        fpsText.color = _goodColor;
                        break;
                    default: {
                        switch (value > 0.6f * _targetFPS) {
                            case true:
                                fpsText.color = _ehColor;
                                break;
                            default:
                                fpsText.color = _noColor;
                                break;
                        }

                        break;
                    }
                }
            }
        }

        public float leftSaberSpeed {
            set {
                leftSpeedText.text = $"{value:0.0} m/s";
                leftSpeedText.color = value >= 2f ? _goodColor : _noColor; // 2 is the min. saber speed to hit a note
            }
        }

        public float rightSaberSpeed {
            set {
                rightSpeedText.text = $"{value:0.0} m/s";
                rightSpeedText.color = value >= 2f ? _goodColor : _noColor; // 2 is the min. saber speed to hit a note
            }
        }

        public event Action<bool> DidPositionTabVisibilityChange;
        public event Action<string> DidPositionPreviewChange;
        public event Action<XRNode> HandDidSwitchEvent;
        public event Action<float> DidTimeSyncChange;
        public event Action<bool> DidChangeVisiblity;
        public event Action DidClickPausePlay;
        public event Action DidClickRestart;
        public event Action DidPositionJump;
        public event Action DidClickLoop;

        [Inject]
        protected void Construct() {
            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(60f, 45f), false,
                defaultPosition.position, defaultPosition.rotation);
            _floatingScreen.GetComponent<Canvas>().sortingOrder = 31;
            _floatingScreen.name = "Imber Replay Panel (Screen)";
            _floatingScreen.transform.localScale /= 2f;
            name = "Imber Replay Panel (View)";
        }

        public void Setup(float initialSongTime, int targetFramerate, string defaultLocation,
            IEnumerable<string> locations) {
            _initialTime = initialSongTime;
            _targetFPS = targetFramerate;
            _timeSync = initialSongTime;
            _location = defaultLocation;
            this.locations.AddRange(locations);
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            tabSelector.textSegmentedControl.didSelectCellEvent += DidSelect;
            didParse = true;
            switch (firstActivation) {
                case true:
                    tabSelector.transform.localScale *= .9f;
                    break;
            }
        }

        private void DidSelect(SegmentedControl _, int selected) {
            const int positionTabIndex = 2;
            if (_lastTab == 2 || selected == 2) {
                DidPositionTabVisibilityChange?.Invoke(selected == positionTabIndex);
            }

            _lastTab = selected;
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            tabSelector.textSegmentedControl.didSelectCellEvent -= DidSelect;
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }

        public void SwitchHand(XRNode xrNode) {
            HandDidSwitchEvent?.Invoke(xrNode);
        }

        [UIAction("pause-play")]
        protected void PausePlay() {
            DidClickPausePlay?.Invoke();
        }

        [UIAction("restart")]
        protected void Restart() {
            DidClickRestart?.Invoke();
        }

        [UIAction("loop")]
        protected void Loop() {
            DidClickLoop?.Invoke();
        }

        [UIAction("left-hand")]
        protected void SwitchHandLeft() {
            SwitchHand(XRNode.LeftHand);
        }

        [UIAction("right-hand")]
        protected void SwitchHandRight() {
            SwitchHand(XRNode.RightHand);
        }

        [UIAction("request-dismiss")]
        protected void RequestDismiss() {
            DidChangeVisiblity?.Invoke(false);
        }

        [UIAction("format-time-percent")]
        protected string FormatTimePercent(float value) {
            return value.ToString("P0");
        }

        [UIAction("jump")]
        protected void Jump() {
            DidPositionJump?.Invoke();
        }
    }
}