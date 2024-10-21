using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using ScoreSaber.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using VRUIControls;
using Zenject;

namespace ScoreSaber.Core.ReplaySystem.UI {

    [HotReload(RelativePathToLayout = @"desktop-imber-panel.bsml")]
    [ViewDefinition("ScoreSaber.Core.ReplaySystem.UI.desktop-imber-panel.bsml")]
    internal class DesktopMainImberPanelView : BSMLAutomaticViewController, IDisposable {

        public event Action<XRNode> HandDidSwitchEvent;
        public event Action<float> DidTimeSyncChange;
        public event Action DidClickPausePlay;
        public event Action DidClickRestart;
        public event Action DidPositionJump;
        public event Action DidClickLoop;

        private int _targetFPS = 90;
        private float _initialTime = 1f;
        private static readonly Color _goodColor = Color.green;
        private static readonly Color _ehColor = Color.yellow;
        private static readonly Color _noColor = Color.red;

        [Inject] private readonly VRInputModule _inputModule = null;
        [Inject] private readonly AudioTimeSyncController _audioTimeSyncController = null;
        [Inject] private readonly ImberScrubber _imberScrubber = null;

        private EventSystem originalEventSystem;
        private EventSystem imberEventSystem;

        public bool didParse { get; private set; }

        public Transform Transform {
            get => this.transform;
        }

        public Pose defaultPosition { get; set; }

        private float _timeSync = 1f;


        [UIComponent("currentTimeText")]
        public TextMeshProUGUI currentTimeText = null;

        [UIComponent("timebarbg")]
        public ImageView timebarbg = null;

        [UIComponent("timebarActive")]
        public ImageView timebarActive = null;

        [UIComponent("fadedBoxVertTimeline")]
        public HorizontalLayoutGroup fadedBoxVertTimeline = null;

        [UIValue("time-sync")]
        public float timeSync {
            get => _timeSync;
            set {
                _timeSync = Mathf.Approximately(_initialTime, value) ? _initialTime : value;
                DidTimeSyncChange?.Invoke(_timeSync);
            }
        }

        private string _playPauseText = "PAUSE";
        [UIValue("play-pause-text")]
        public string playPauseText {
            get => _playPauseText;
            set {
                _playPauseText = value;
                NotifyPropertyChanged();
            }
        }

        private string _loopText = "LOOP";
        [UIValue("loop-text")]
        public string loopText {
            get => _loopText;
            set {
                _loopText = value;
                NotifyPropertyChanged();
            }
        }

        public int fps {
            set {
                fpsText.text = value.ToString();
                if (value > 0.85f * _targetFPS)
                    fpsText.color = _goodColor;
                else if (value > 0.6f * _targetFPS)
                    fpsText.color = _ehColor;
                else
                    fpsText.color = _noColor;
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

        [UIComponent("tab-selector")]
        protected readonly TabSelector tabSelector = null;

        [UIComponent("fps-text")]
        protected readonly CurvedTextMeshPro fpsText = null;

        [UIComponent("left-speed-text")]
        protected readonly CurvedTextMeshPro leftSpeedText = null;

        [UIComponent("right-speed-text")]
        protected readonly CurvedTextMeshPro rightSpeedText = null;

        [UIObject("container")]
        public GameObject _container = null;

        [Inject]
        protected void Construct() {

            if (!Environment.GetCommandLineArgs().Contains("fpfc")) return;
            GameObject inputOBJ;

            var canvasGameObj = new GameObject();
            var canvas = canvasGameObj.AddComponent<Canvas>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            HMUI.Screen screen = canvasGameObj.AddComponent<HMUI.Screen>();
            var canvasScaler = screen.gameObject.AddComponent<CanvasScaler>();
            canvasScaler.referenceResolution = new Vector2(350, 300);
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            canvasScaler.dynamicPixelsPerUnit = 3.44f;
            canvasScaler.referencePixelsPerUnit = 10f;

            canvas.name = "ScoreSaberDesktopImberUI";

            canvasGameObj.SetActive(true);

            canvas.sortingOrder = 1;
            canvas.overrideSorting = true;

            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;

            var canvasGR = canvas.gameObject.AddComponent<GraphicRaycaster>();
            gameObject.AddComponent<GraphicRaycaster>();

            originalEventSystem = _inputModule.GetComponent<EventSystem>();

            inputOBJ = new GameObject("ImberInputGO");
            inputOBJ.AddComponent<StandaloneInputModule>();
            Cursor.visible = true;

            if(inputOBJ.GetComponent<EventSystem>() == null) {
                imberEventSystem = inputOBJ.AddComponent<EventSystem>();
            }

            EventSystem.current = imberEventSystem;

            gameObject.transform.SetParent(canvas.transform, false);
            __Init(screen, parentViewController, containerViewController);
            screen.SetRootViewController(this, ViewController.AnimationType.None);

            
        }


        public void Dispose() {
            if (!Environment.GetCommandLineArgs().Contains("fpfc")) return;
            //EventSystem.current = originalEventSystem;
            Cursor.visible = false;
        }


        public void Setup(float initialSongTime, int targetFramerate) {

            _initialTime = initialSongTime;
            _targetFPS = targetFramerate;
            _timeSync = initialSongTime;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {

            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation) {
                var x = timebarActive.gameObject.AddComponent<ProgressHandler>();
                x.timebarBackground = timebarbg;
                x.timebarActive = timebarActive;
                x.OnProgressUpdated += (progress) => {
                    _imberScrubber.MainNode_PositionDidChange(progress);
                };
                //var containerRect = this.GetComponent<RectTransform>();
                //_container.GetComponent<RectTransform>().position = Vector3.zero;
                //_container.GetComponent<RectTransform>().anchorMax = new Vector2(0.2f, 0.2f);

                //Plugin.Log.Notice($"{Plugin.Settings.replayUIPosition.x},{Plugin.Settings.replayUIPosition.y}");
                //containerRect.anchorMax = new Vector2(Plugin.Settings.replayUIPosition.x, Plugin.Settings.replayUIPosition.y);
            }
            didParse = true;
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {

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

        [UIAction("format-time-percent")]
        protected string FormatTimePercent(float value) {

            return value.ToString("P0");
        }

        [UIAction("jump")]
        protected void Jump() {

            DidPositionJump?.Invoke();
        }
        private string FloatToTimeStamp(float timeInSeconds) {
            int minutes = (int)timeInSeconds / 60;
            int seconds = (int)timeInSeconds % 60;

            return $"{minutes:D2}:{seconds:D2}";
        }

        public void FixedUpdate() {

            if (!didParse) return;
            currentTimeText.text = FloatToTimeStamp(_audioTimeSyncController.songTime) + "/" + FloatToTimeStamp(_audioTimeSyncController.songLength);

            float progressPercentage = Mathf.Clamp01(_audioTimeSyncController.songTime / _audioTimeSyncController.songLength);

            float timebarActiveX = Mathf.Lerp(-19, 19, progressPercentage);
            timebarActive.rectTransform.anchoredPosition = new Vector2(timebarActiveX, 0);
        }

        public class ProgressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {

            public ImageView timebarActive;
            public ImageView timebarBackground;

            public event Action<float> OnProgressUpdated;

            private bool isDragging = false;
            private float minX = -19f;
            private float maxX = 19f;

            public void OnPointerDown(PointerEventData eventData) {
                isDragging = true;
                UpdateTimebarPosition(eventData);
            }

            public void OnPointerUp(PointerEventData eventData) {
                isDragging = false;
                UpdateTimebarPosition(eventData);
            }

            public void OnDrag(PointerEventData eventData) {
                if (isDragging) {
                    UpdateTimebarPosition(eventData);
                }
            }

            private void UpdateTimebarPosition(PointerEventData eventData) {
                RectTransform timebarRect = timebarBackground.rectTransform;
                Vector2 localPoint;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(timebarRect, eventData.position, eventData.pressEventCamera, out localPoint)) {
                    float clampedX = Mathf.Clamp(localPoint.x, minX, maxX);

                    timebarActive.rectTransform.anchoredPosition = new Vector2(clampedX, 0);

                    float progress = Mathf.InverseLerp(minX, maxX, clampedX);

                    OnProgressUpdated?.Invoke(progress);
                }
            }
        }


        //public class DraggableViewController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {

        //    public RectTransform draggableRectTransform;
        //    public GameObject canvas;

        //    private Vector2 lastMousePosition;
        //    private bool isDragging = false;

        //    public void OnPointerDown(PointerEventData eventData) {
        //        isDragging = true;
        //        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out lastMousePosition);
        //    }

        //    public void OnDrag(PointerEventData eventData) {
        //        if (isDragging && draggableRectTransform != null) {
        //            Vector2 mousePosition;
        //            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out mousePosition);

        //            Vector2 delta = mousePosition - lastMousePosition;

        //            draggableRectTransform.anchoredPosition += delta;

        //            Vector2 normalizedPosition = new Vector2(
        //                (draggableRectTransform.anchoredPosition.x + canvas.GetComponent<RectTransform>().rect.width / 2) / canvas.GetComponent<RectTransform>().rect.width,
        //                (draggableRectTransform.anchoredPosition.y + canvas.GetComponent<RectTransform>().rect.height / 2) / canvas.GetComponent<RectTransform>().rect.height
        //            );

        //            normalizedPosition.x = Mathf.Clamp01(normalizedPosition.x);
        //            normalizedPosition.y = Mathf.Clamp01(normalizedPosition.y);

        //            draggableRectTransform.anchorMin = normalizedPosition;
        //            draggableRectTransform.anchorMax = normalizedPosition;

        //            lastMousePosition = mousePosition;
        //        }
        //    }

        //    public void OnPointerUp(PointerEventData eventData) {
        //        isDragging = false;
        //    }
        //}

    }
}