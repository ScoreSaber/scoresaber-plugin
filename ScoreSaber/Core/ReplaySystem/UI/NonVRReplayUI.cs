#region

using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Core.ReplaySystem.Playback;
using System.Linq;
using UnityEngine;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.UI {
    internal class NonVRReplayUI : MonoBehaviour {
        [Inject] private readonly AudioTimeSyncController _audioTimeSyncController = null;
        [Inject] private readonly PosePlayer _posePlayer = null;
        [Inject] private readonly SaberManager _saberManager = null;
        [Inject] private readonly ReplayFile _file = null;

        private GUIStyle _headerStyle;

        private int _currentPosition;
        const int _offset = 16;
        const int _headerOffset = 20;
        private float _initialTimeScale;

        private int _fps;
        private string _leftSaberSpeed;
        private string _rightSaberSpeed;

        protected void Start() {

            _headerStyle = new GUIStyle();
            _headerStyle.fontSize = 16;
            _headerStyle.normal.textColor = Color.white;
            _initialTimeScale = _file.noteKeyframes.FirstOrDefault().TimeSyncTimescale;
            _posePlayer.DidUpdatePose += PosePlayer_DidUpdatePose;
        }

        protected void OnDestroy() {

            _posePlayer.DidUpdatePose -= PosePlayer_DidUpdatePose;
        }

        private void PosePlayer_DidUpdatePose(VRPoseGroup pose) {

            _fps = pose.FPS;
            _leftSaberSpeed = $"{_saberManager.leftSaber.movementData.bladeSpeed * (_initialTimeScale / _audioTimeSyncController.timeScale):0.0} m/s";
            _rightSaberSpeed = $"{_saberManager.rightSaber.movementData.bladeSpeed * (_initialTimeScale / _audioTimeSyncController.timeScale):0.0} m/s";
        }

        protected void OnGUI() {

            if (!Plugin.Settings.HideReplayUI) {
                _currentPosition = 0;
                DrawLabel("Replay Controls -", header: true);
                DrawLabel("Pause: Space");
                DrawLabel("Seek: 1-9");
                DrawLabel("Increase Time Scale: +");
                DrawLabel("Decrease Time Scale: -");
                DrawLabel("Hide Sabers: H");
                DrawLabel("Hide Desktop Replay UI: C");
                DrawLabel("Replay Player Status -", header: true);
                DrawLabel($"Current Song Time: {(int)_audioTimeSyncController.songTime / 60}:{_audioTimeSyncController.songTime % 60f:00}");
                DrawLabel($"Current Time Scale: {_audioTimeSyncController.timeScale:P0}");
                DrawLabel($"Player's FPS: {_fps}");
                DrawLabel($"Left Saber Speed: {_leftSaberSpeed}");
                DrawLabel($"Right Saber Speed: {_rightSaberSpeed}");
            }
        }

        protected void Update() {

            if (Input.GetKeyDown(KeyCode.C)) {
                Plugin.Settings.HideReplayUI = !Plugin.Settings.HideReplayUI;
            }
        }

        private void DrawLabel(string text, bool header = false) {

            if (header) {
                _currentPosition += _headerOffset;
                GUI.Label(new Rect(10, _currentPosition, 300, 20), text, _headerStyle);
            } else {
                _currentPosition += _offset;
                GUI.Label(new Rect(10, _currentPosition, 300, 20), text);
            }
        }
    }
}