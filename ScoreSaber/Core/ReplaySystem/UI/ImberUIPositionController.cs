#region

using HMUI;
using ScoreSaber.Core.Data.Internal;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using VRUIControls;
using Zenject;
using Object = UnityEngine.Object;

#endregion

namespace ScoreSaber.Core.ReplaySystem.UI {
    internal class ImberUIPositionController : IInitializable, ITickable, IDisposable {
        private readonly Canvas _canvas;
        private readonly CurvedCanvasSettings _curve;

        private readonly IGamePause _gamePause;
        private readonly ImberScrubber _imberScrubber;
        private readonly MainImberPanelView _mainImberPanelView;
        private readonly MainSettingsModelSO _mainSettingsModelSO;
        private readonly Transform _menuControllerTransform;
        private readonly Transform _menuWrapperTransform;
        private readonly Transform _pauseMenuManagerTransform;
        private readonly float _sensitivityToClick = 0.5f;
        private readonly float _timeBufferToDoubleClick = 0.75f;
        private readonly VRControllerAccessor _vrControllerAccessor;
        private readonly VRGraphicRaycaster _vrGraphicsRaycaster;
        private Vector3 _controllerOffset;
        private bool _didClickOnce;
        private XRNode _handTrack = XRNode.LeftHand;
        private bool _isActive;
        private bool _isClicking;

        private bool _isPaused;
        private DateTime _lastTriggerDownTime;

        public ImberUIPositionController(IGamePause gamePause, ImberScrubber imberScrubber,
            PauseMenuManager pauseMenuManager, MainImberPanelView mainImberPanelView,
            VRControllerAccessor vrControllerAccessor) {
            _gamePause = gamePause;
            _imberScrubber = imberScrubber;
            _mainImberPanelView = mainImberPanelView;
            _vrControllerAccessor = vrControllerAccessor;
            _menuWrapperTransform = pauseMenuManager.transform.Find("Wrapper/MenuWrapper");
            _pauseMenuManagerTransform = pauseMenuManager.transform;
            _menuControllerTransform = _vrControllerAccessor.leftController.transform.parent;
            _vrGraphicsRaycaster = _menuWrapperTransform.GetComponentInChildren<VRGraphicRaycaster>();
            _canvas = _vrGraphicsRaycaster.GetComponent<Canvas>();
            _curve = _canvas.GetComponent<CurvedCanvasSettings>();
            _mainSettingsModelSO = Resources.FindObjectsOfTypeAll<MainSettingsModelSO>()[0];
            _controllerOffset = new Vector3(0f, 0f, -2f);
        }

        public void Dispose() {
            _gamePause.didResumeEvent -= GamePause_didResumeEvent;
            _gamePause.didPauseEvent -= GamePause_didPauseEvent;
        }

        public void Initialize() {
            _gamePause.didPauseEvent += GamePause_didPauseEvent;
            _gamePause.didResumeEvent += GamePause_didResumeEvent;
            _pauseMenuManagerTransform.position =
                new Vector3(_controllerOffset.x, _controllerOffset.y, _controllerOffset.z);

            switch (Plugin.Settings.leftHandedReplayUI) {
                case true:
                    _handTrack = XRNode.RightHand;
                    break;
            }
        }

        public void Tick() {
            VRController controller = _handTrack == XRNode.LeftHand
                ? _vrControllerAccessor.leftController
                : _vrControllerAccessor.rightController;

            switch (_didClickOnce) {
                // Detect Trigger Double Click
                case true when DateTime.Now > _lastTriggerDownTime.AddSeconds(_timeBufferToDoubleClick):
                    _didClickOnce = false;
                    break;
                default: {
                    switch (controller.triggerValue >= _sensitivityToClick) {
                        case true when !_isClicking: {
                            _isClicking = true;
                            switch (_didClickOnce) {
                                case true: {
                                    _didClickOnce = false;
                                    // DID DOUBLE CLICK HERE!!!
                                    _isActive = !_isActive;
                                    _imberScrubber.visibility = _isActive;
                                    _mainImberPanelView.visibility = _isActive;
                                    OpenedUI();
                                    _mainImberPanelView.StartCoroutine(KillMe(controller));

                                    switch (_isPaused) {
                                        case false:
                                            _curve.enabled = !_isActive;
                                            _canvas.enabled = !_isActive;
                                            _menuWrapperTransform.gameObject.SetActive(_isActive);
                                            _menuControllerTransform.gameObject.SetActive(_isActive);
                                            _vrGraphicsRaycaster.enabled = _isActive;
                                            break;
                                    }

                                    break;
                                }
                                default:
                                    _lastTriggerDownTime = DateTime.Now;
                                    _didClickOnce = true;
                                    break;
                            }

                            break;
                        }
                        default: {
                            switch (controller.triggerValue < _sensitivityToClick) {
                                case true when _isClicking:
                                    _isClicking = false;
                                    break;
                            }

                            break;
                        }
                    }

                    break;
                }
            }

            switch (_isActive) {
                // Update Active UI Position
                case true when !Plugin.Settings.lockedReplayUIMode:
                    SetUIPosition(controller);
                    break;
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

        private IEnumerator KillMe(VRController controller) {
            for (int i = 0; i < 5; i++) {
                yield return new WaitForEndOfFrame();
            }

            SetUIPosition(controller);
        }

        private void SetUIPosition(VRController controller) {
            Vector3 viewOffset = _handTrack == XRNode.LeftHand
                ? new Vector3(0.25f, 0.25f, 0.25f)
                : new Vector3(-0.25f, 0.25f, 0.25f);
            Vector3 scrubberOffset = _handTrack == XRNode.LeftHand
                ? new Vector3(0.46f, -0.06f, 0.25f)
                : new Vector3(-0.46f, -0.06f, 0.25f);

            _mainImberPanelView.Transform.SetLocalPositionAndRotation(controller.transform.TransformPoint(viewOffset),
                controller.transform.rotation);
            _imberScrubber.transform.SetLocalPositionAndRotation(controller.transform.TransformPoint(scrubberOffset),
                controller.transform.rotation);
        }

        private void OpenedUI() {
            switch (Plugin.Settings.hasOpenedReplayUI) {
                case false: {
                    GameObject replayPrompt = GameObject.Find("Replay Prompt");
                    if (replayPrompt != null) {
                        Object.Destroy(replayPrompt);
                    }

                    Plugin.Settings.hasOpenedReplayUI = true;
                    Settings.SaveSettings(Plugin.Settings);
                    break;
                }
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
            _pauseMenuManagerTransform.position =
                new Vector3(_controllerOffset.x, _controllerOffset.y, _controllerOffset.z);
        }
    }
}