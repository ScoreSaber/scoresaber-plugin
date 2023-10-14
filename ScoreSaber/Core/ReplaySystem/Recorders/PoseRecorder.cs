using ScoreSaber.Core.ReplaySystem.Data;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace ScoreSaber.Core.ReplaySystem.Recorders {
    internal class PoseRecorder : TimeSynchronizer, ITickable
    {
        private readonly PlayerTransforms _playerTransforms;
        private readonly List<VRPoseGroup> _vrPoseGroup;
        private bool _recording;

        public PoseRecorder(PlayerTransforms playerTransforms) {

            _playerTransforms = playerTransforms;
            _vrPoseGroup = new List<VRPoseGroup>();
            _recording = true;
        }

        public void StopRecording() {
            _recording = false;
        }

        public void Tick() {

            if (!_recording)
                return;

            _vrPoseGroup.Add(new VRPoseGroup() {
                Head = new VRPose() {
                    Position = new VRPosition() { 
                        X = _playerTransforms.headPseudoLocalPos.x, Y = _playerTransforms.headPseudoLocalPos.y, Z = _playerTransforms.headPseudoLocalPos.z
                    },
                    Rotation = new VRRotation() {
                        X = _playerTransforms.headPseudoLocalRot.x, Y = _playerTransforms.headPseudoLocalRot.y, Z = _playerTransforms.headPseudoLocalRot.z, W = _playerTransforms.headPseudoLocalRot.w
                    }
                },
                Left = new VRPose() {
                    Position = new VRPosition() {
                        X = _playerTransforms.leftHandPseudoLocalPos.x, Y = _playerTransforms.leftHandPseudoLocalPos.y, Z = _playerTransforms.leftHandPseudoLocalPos.z
                    },
                    Rotation = new VRRotation() {
                        X = _playerTransforms.leftHandPseudoLocalRot.x, Y = _playerTransforms.leftHandPseudoLocalRot.y, Z = _playerTransforms.leftHandPseudoLocalRot.z, W = _playerTransforms.leftHandPseudoLocalRot.w
                    }
                },
                Right = new VRPose() {
                    Position = new VRPosition() {
                        X = _playerTransforms.rightHandPseudoLocalPos.x, Y = _playerTransforms.rightHandPseudoLocalPos.y, Z = _playerTransforms.rightHandPseudoLocalPos.z
                    },
                    Rotation = new VRRotation() {
                        X = _playerTransforms.rightHandPseudoLocalRot.x, Y = _playerTransforms.rightHandPseudoLocalRot.y, Z = _playerTransforms.rightHandPseudoLocalRot.z, W = _playerTransforms.rightHandPseudoLocalRot.w
                    }
                },
                Time = audioTimeSyncController.songTime,
                FPS = (int)(1f / Time.unscaledDeltaTime)
            });
        }

        public List<VRPoseGroup> Export() {

            return _vrPoseGroup;
        }
    }
}