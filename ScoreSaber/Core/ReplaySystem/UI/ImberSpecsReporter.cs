#region

using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Core.ReplaySystem.Playback;
using System;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.UI {
    internal class ImberSpecsReporter : IInitializable, IDisposable {
        private readonly PosePlayer _posePlayer;
        private readonly SaberManager _saberManager;

        public ImberSpecsReporter(PosePlayer posePlayer, SaberManager saberManager) {
            _posePlayer = posePlayer;
            _saberManager = saberManager;
        }

        public void Dispose() {
            _posePlayer.DidUpdatePose -= PosePlayer_DidUpdatePose;
        }

        public void Initialize() {
            _posePlayer.DidUpdatePose += PosePlayer_DidUpdatePose;
        }

        public event Action<int, float, float> DidReport;

        private void PosePlayer_DidUpdatePose(VRPoseGroup pose) {
            DidReport?.Invoke(pose.FPS, _saberManager.leftSaber.movementData.bladeSpeed,
                _saberManager.rightSaber.movementData.bladeSpeed);
        }
    }
}