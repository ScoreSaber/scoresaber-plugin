#region

using HMUI;
using ScoreSaber.Core.Data.Internal;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using VRUIControls;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.UI {
    internal class ImberUIPositionController : IInitializable, ITickable, IDisposable {
        private bool _isActive;
        private bool _isClicking;
        private bool _didClickOnce;
        private DateTime _lastTriggerDownTime;
        private XRNode _handTrack = XRNode.LeftHand;
        private readonly float _sensitivityToClick = 0.5f;
        private readonly float _timeBufferToDoubleClick = 0.75f;

        private readonly IGamePause _gamePause;
        private readonly ImberScrubber _imberScrubber;
        private readonly MainImberPanelView _mainImberPanelView;
        private readonly VRControllerAccessor _vrControllerAccessor;

        private bool _isPaused;
        private readonly VRGraphicRaycaster _vrGraphicsRaycaster;
        private readonly Transform _menuControllerTransform;
        private readonly Transform _menuWrapperTransform;
        private readonly Transform _pauseMenuManagerTransform;
        private readonly CurvedCanvasSettings _curve;
        private readonly Canvas _canvas;
        private readonly MainSettingsModelSO _mainSettingsModelSO;
        private Vector3 _controllerOffset;

        public ImberUIPositionController(IGamePause gamePause, ImberScrubber imberScrubber, PauseMenuManager pauseMenuManager, MainImberPanelView mainImberPanelView, VRControllerAccessor vrControllerAccessor) {

            _gamePause = gamePause;
            _imberScrubber = imberScrubber;
            _mainImberPanelView = mainImberPanelView;
            _vrControllerAccessor = vrControllerAccessor;
            _menuWrapperTransform = pauseMenuManager.transform.Find("Wrapper/MenuWrapper");
            _pauseMenuManagerTransform = pauseMenuManager.transform;
            _menuControllerTransform = _vrControllerAccessor.LeftController.transform.parent;
            _vrGraphicsRaycaster = _menuWrapperTransform.GetComponentInChildren<VRGraphicRaycaster>();
            _canvas = _vrGraphicsRaycaster.GetComponent<Canvas>();
            _curve = _canvas.GetComponent<CurvedCanvasSettings>();
            _mainSettingsModelSO = Resources.FindObjectsOfTypeAll<MainSettingsModelSO>()[0];
            _controllerOffset = new Vector3(0f, 0f, -2f);
        }

        public void Initialize() {

            _gamePause.didPauseEvent += GamePause_didPauseEvent;
            _gamePause.didResumeEvent += GamePause_didResumeEvent;
            _pauseMenuManagerTransform.position = new Vector3(_controllerOffset.x, _controllerOffset.y, _controllerOffset.z);

            if (Plugin.Settings.LeftHandedReplayUI) {
                _handTrack = XRNode.RightHand;
            }
        }

        private void GamePause_didResumeEvent() {

            _isPaused = false;
            _menuWrapperTransform.gameObject.SetActive(_isActive);
            _menuControllerTransform.gameObject.SetActive(_isActive);
            _vrGraphicsRaycaster.enabled = _isActive;
        }

        private void GamePause_didPauseEvent() {

            _isPaused = true;
            _menuWrapperTransform.gameObject.SetActive(false);
            _curve.enabled = true;
            _canvas.enabled = true;
            _menuWrapperTransform.gameObject.SetActive(true);
        }

        public void Tick() {

            VRController controller = _handTrack == XRNode.LeftHand ? _vrControllerAccessor.LeftController : _vrControllerAccessor.RightController;

            // Detect Trigger Double Click
            if (_didClickOnce && DateTime.Now > _lastTriggerDownTime.AddSeconds(_timeBufferToDoubleClick)) {
                _didClickOnce = false;
            } else {
                if (controller.triggerValue >= _sensitivityToClick && !_isClicking) {
                    _isClicking = true;
                    if (_didClickOnce) {
                        _didClickOnce = false;
                        // DID DOUBLE CLICK HERE!!!
                        _isActive = !_isActive;
                        _imberScrubber.Visibility = _isActive;
                        _mainImberPanelView.Visibility = _isActive;
                        OpenedUI();
                        _mainImberPanelView.StartCoroutine(KillMe(controller));

                        if (!_isPaused) {
                            _curve.enabled = !_isActive;
                            _canvas.enabled = !_isActive;
                            _menuWrapperTransform.gameObject.SetActive(_isActive);
                            _menuControllerTransform.gameObject.SetActive(_isActive);
                            _vrGraphicsRaycaster.enabled = _isActive;
                        }
                    } else {
                        _lastTriggerDownTime = DateTime.Now;
                        _didClickOnce = true;
                    }
                } else if (controller.triggerValue < _sensitivityToClick && _isClicking) {
                    _isClicking = false;
                }
            }

            // Update Active UI Position
            if (_isActive && !Plugin.Settings.LockedReplayUIMode) {

                SetUIPosition(controller);
            }
        }

        private IEnumerator KillMe(VRController controller) {

            for (int i = 0; i < 5; i++) {
                yield return new WaitForEndOfFrame();
            }
            SetUIPosition(controller);
        }

        private void SetUIPosition(VRController controller) {

            var viewOffset = _handTrack == XRNode.LeftHand ? new Vector3(0.25f, 0.25f, 0.25f) : new Vector3(-0.25f, 0.25f, 0.25f);
            var scrubberOffset = _handTrack == XRNode.LeftHand ? new Vector3(0.46f, -0.06f, 0.25f) : new Vector3(-0.46f, -0.06f, 0.25f);

            _mainImberPanelView.Transform.SetLocalPositionAndRotation(controller.transform.TransformPoint(viewOffset), controller.transform.rotation);
            _imberScrubber.Transform.SetLocalPositionAndRotation(controller.transform.TransformPoint(scrubberOffset), controller.transform.rotation);
        }

        private void OpenedUI() {

            if (!Plugin.Settings.HasOpenedReplayUI) {
                var replayPrompt = GameObject.Find("Replay Prompt");
                if (replayPrompt != null) {
                    GameObject.Destroy(replayPrompt);
                }
                Plugin.Settings.HasOpenedReplayUI = true;
                Settings.SaveSettings(Plugin.Settings);
            }
        }

        public void UpdateTrackingHand(XRNode node) {

            _handTrack = node;
        }

        public void SetActiveState(bool value) {

            _isActive = value;
        }

        public void SetControllerOffset(Vector3 value) {

            _controllerOffset = value;
            _pauseMenuManagerTransform.position = new Vector3(_controllerOffset.x, _controllerOffset.y, _controllerOffset.z);
        }

        public void Dispose() {

            _gamePause.didResumeEvent -= GamePause_didResumeEvent;
            _gamePause.didPauseEvent -= GamePause_didPauseEvent;
        }
    }
}