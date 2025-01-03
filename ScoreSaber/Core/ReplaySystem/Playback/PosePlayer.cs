using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Extensions;
using SiraUtil.Tools.FPFC;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SpatialTracking;
using Zenject;

namespace ScoreSaber.Core.ReplaySystem.Playback
{
    internal class PosePlayer : TimeSynchronizer, IInitializable, ITickable, IScroller, IDisposable
    {
        private int _nextIndex = 0;
        private readonly MainCamera _mainCamera;
        private readonly SaberManager _saberManager;
        private readonly VRPoseGroup[] _sortedPoses;
        private readonly IFPFCSettings _fpfcSettings;
        private readonly SettingsManager _settingsManager;
        private readonly IReturnToMenuController _returnToMenuController;
        private readonly PauseController _pauseController;
        public event Action<VRPoseGroup> DidUpdatePose;
        private PlayerTransforms _playerTransforms;
        private Camera _spectatorCamera;
        private Camera _desktopCamera;
        private bool _saberEnabled = true;
        private Vector3 _spectatorOffset;

        private bool initialFPFCState;

        public PosePlayer(ReplayFile file, MainCamera mainCamera, SaberManager saberManager, IReturnToMenuController returnToMenuController, IFPFCSettings fpfcSettings, PlayerTransforms playerTransforms, SettingsManager settingsManager, PauseController pauseController) {

            _fpfcSettings = fpfcSettings;
            initialFPFCState = fpfcSettings.Enabled;
            _fpfcSettings.Enabled = false;

            _mainCamera = mainCamera;
            _saberManager = saberManager;
            _sortedPoses = file.poseKeyframes.ToArray();
            _returnToMenuController = returnToMenuController;
            _spectatorOffset = new Vector3(0f, 0f, -2f);
            _settingsManager = settingsManager;
            _playerTransforms = playerTransforms;
            _pauseController = pauseController;
        }

        public void Initialize() {

            SetupCameras();
            //_saberManager.leftSaber.disableCutting = true;
            //_saberManager.rightSaber.disableCutting = true;
            _saberManager.leftSaber.transform.GetComponentInParent<VRController>().enabled = false;
            _saberManager.rightSaber.transform.GetComponentInParent<VRController>().enabled = false;
            _fpfcSettings.Changed += fpfcSettings_Changed;
        }

        private void fpfcSettings_Changed(IFPFCSettings fpfcSettings) {

            if (fpfcSettings.Enabled) {
                _desktopCamera.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }

        private void SetupCameras() {

            _mainCamera.enabled = false;
            _mainCamera.gameObject.SetActive(false);
            _desktopCamera = Resources.FindObjectsOfTypeAll<Camera>().First(x => (x.name == "RecorderCamera"));

            //Desktop Camera
            _desktopCamera.fieldOfView = Plugin.Settings.replayCameraFOV;
            _desktopCamera.transform.position = new Vector3(_desktopCamera.transform.position.x, _desktopCamera.transform.position.y, _desktopCamera.transform.position.z);
            _desktopCamera.gameObject.SetActive(true);
            _desktopCamera.tag = "MainCamera";
            _desktopCamera.depth = 1;

            _mainCamera.SetField("_camera", _desktopCamera);

            //InGame Camera
            GameObject spectatorObject = new GameObject("SpectatorParent");
            _spectatorCamera = UnityEngine.Object.Instantiate(_desktopCamera);
            spectatorObject.transform.position = new Vector3(_settingsManager.settings.room.center.x + _spectatorOffset.x, _settingsManager.settings.room.center.y + _spectatorOffset.y, _settingsManager.settings.room.center.z + _spectatorOffset.z);
            Quaternion rotation = new Quaternion {
                eulerAngles = new Vector3(0.0f, _settingsManager.settings.room.rotation, 0.0f)
            };
            spectatorObject.transform.rotation = rotation;
            _spectatorCamera.stereoTargetEye = StereoTargetEyeMask.Both;
            _mainCamera.gameObject.GetComponent<TrackedPoseDriver>().CopyComponent<TrackedPoseDriver>(_spectatorCamera.gameObject);
            
            // recreate the DepthTextureController since Instantiate seems to leave it wrongly initialized (i.e. without Zenject objects)
            Component.Destroy(_spectatorCamera.gameObject.GetComponent<DepthTextureController>());
            _mainCamera.gameObject.GetComponent<DepthTextureController>().CopyComponent<DepthTextureController>(_spectatorCamera.gameObject);

            _spectatorCamera.gameObject.SetActive(true);
            _spectatorCamera.depth = 0;
            _spectatorCamera.transform.SetParent(spectatorObject.transform);

            if (Plugin.Settings.enableReplayFrameRenderer) {
                var ss = Resources.FindObjectsOfTypeAll<ScreenshotRecorder>().Last();
                ss.SetField("_directory", Plugin.Settings.replayFramePath);
                ss.enabled = true;
                _desktopCamera.depth = 1;
                var gc = Resources.FindObjectsOfTypeAll<DisableGCWhileEnabled>().Last();
                gc.enabled = false;
            }
        }

        public bool _replayReachedEnd = false;

        public void Tick() {
            bool foundPoseThisFrame = false;

            while (_nextIndex < _sortedPoses.Count() && audioTimeSyncController.songTime >= _sortedPoses[_nextIndex].Time) {
                foundPoseThisFrame = true;
                VRPoseGroup activePose = _sortedPoses[_nextIndex++];

                if (_nextIndex < _sortedPoses.Count()) {
                    VRPoseGroup nextPose = _sortedPoses[_nextIndex];
                    UpdatePoses(activePose, nextPose);
                }
            }

            if (foundPoseThisFrame) {
                return;
            } else if (_nextIndex > 0 && _nextIndex < _sortedPoses.Count() && !ReachedEnd()) {
                VRPoseGroup previousGroup = _sortedPoses[_nextIndex - 1];
                VRPoseGroup nextGroup = _sortedPoses[_nextIndex];
                UpdatePoses(previousGroup, nextGroup);
            }

            if (ReachedEnd()) {
                _replayReachedEnd = true;
                audioTimeSyncController.Pause();
                return;
            } else {
                if (_replayReachedEnd) {
                    audioTimeSyncController.Resume();
                    _replayReachedEnd = false;
                }
            }
        }

        private void UpdatePoses(VRPoseGroup activePose, VRPoseGroup nextPose) {

            if (Input.GetKeyDown(KeyCode.H))
                _saberEnabled = !_saberEnabled;

            float lerpTime = (audioTimeSyncController.songTime - activePose.Time) / Mathf.Max(0.000001f, nextPose.Time - activePose.Time);

            //_mainCamera.transform.SetPositionAndRotation(activePose.Head.Position.Convert(), activePose.Head.Rotation.Convert());
            _playerTransforms._headTransform.SetPositionAndRotation(activePose.Head.Position.Convert(), activePose.Head.Rotation.Convert());

            if (_saberEnabled) {

                _saberManager.leftSaber.OverridePositionAndRotation(
                    Vector3.Lerp(activePose.Left.Position.Convert(), nextPose.Left.Position.Convert(), lerpTime),
                    Quaternion.Lerp(activePose.Left.Rotation.Convert(), nextPose.Left.Rotation.Convert(), lerpTime)
                );

                _saberManager.rightSaber.OverridePositionAndRotation(
                    Vector3.Lerp(activePose.Right.Position.Convert(), nextPose.Right.Position.Convert(), lerpTime),
                    Quaternion.Lerp(activePose.Right.Rotation.Convert(), nextPose.Right.Rotation.Convert(), lerpTime)
                );
            } else {
                _saberManager.leftSaber.OverridePositionAndRotation(Vector3.zero, new Quaternion(0, 0, 0, 0));
                _saberManager.rightSaber.OverridePositionAndRotation(Vector3.zero, new Quaternion(0, 0, 0, 0));
            }


            var pos = Vector3.Lerp(activePose.Head.Position.Convert(), nextPose.Head.Position.Convert(), lerpTime);
            var rot = Quaternion.Lerp(activePose.Head.Rotation.Convert(), nextPose.Head.Rotation.Convert(), lerpTime);

            var eulerAngles = rot.eulerAngles;
            Vector3 headRotationOffset = new Vector3(Plugin.Settings.replayCameraXRotation, Plugin.Settings.replayCameraYRotation, Plugin.Settings.replayCameraZRotation);
            eulerAngles += headRotationOffset;
            rot.eulerAngles = eulerAngles;

            float t2 = Plugin.Settings.replayCameraSmoothing ? Time.deltaTime * 6f : 1.0f;
            pos.x += Plugin.Settings.replayCameraXOffset;
            pos.y += Plugin.Settings.replayCameraYOffset;
            pos.z += Plugin.Settings.replayCameraZOffset;
            if (!_fpfcSettings.Enabled) {
                _desktopCamera.transform.SetPositionAndRotation(Vector3.Lerp(_desktopCamera.transform.position, pos, t2), Quaternion.Lerp(_desktopCamera.transform.rotation, rot, t2));
            }

            DidUpdatePose?.Invoke(activePose);
        }

        // intro skip doesnt skip to the end of the map, it gives a little time, so this is fine
        private bool ReachedEnd() {
            return _nextIndex >= _sortedPoses.Length;
        }

        public void TimeUpdate(float newTime) {

            for (int c = 0; c < _sortedPoses.Length; c++) {
                if (_sortedPoses[c].Time > newTime) {
                    _nextIndex = c;
                    Tick();
                    return;
                }
            }
            _nextIndex = _sortedPoses.Length;
        }

        public void SetSpectatorOffset(Vector3 value) {

            _spectatorCamera.transform.parent.position = new Vector3(_settingsManager.settings.room.center.x + value.x, _settingsManager.settings.room.center.y + value.y, _settingsManager.settings.room.center.z + value.z);

            _spectatorOffset = value;
        }

        public void Dispose() {
            _fpfcSettings.Changed -= fpfcSettings_Changed;
            _fpfcSettings.Enabled = initialFPFCState;
        }
    }
}