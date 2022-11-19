#region

using ScoreSaber.Core.ReplaySystem.Data;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Recorders {
    internal class PoseRecorder : TimeSynchronizer, ITickable {
        private readonly MainCamera _mainCamera;
        private readonly VRController _controllerLeft;
        private readonly VRController _controllerRight;
        private readonly List<VRPoseGroup> _vrPoseGroup;

        public PoseRecorder(MainCamera mainCamera, SaberManager saberManager) {

            _mainCamera = mainCamera;
            _controllerLeft = saberManager.leftSaber.transform.parent.GetComponent<VRController>();
            _controllerRight = saberManager.rightSaber.transform.parent.GetComponent<VRController>();
            _vrPoseGroup = new List<VRPoseGroup>();
        }

        public void Tick() {

            _vrPoseGroup.Add(new VRPoseGroup {
                Head = new VRPose {
                    Position = new VRPosition { X = _mainCamera.position.x, Y = _mainCamera.position.y, Z = _mainCamera.position.z },
                    Rotation = new VRRotation {
                        X = _mainCamera.rotation.x, Y = _mainCamera.rotation.y, Z = _mainCamera.rotation.z, W = _mainCamera.rotation.w
                    }
                },
                Left = new VRPose {
                    Position = new VRPosition { X = _controllerLeft.position.x, Y = _controllerLeft.position.y, Z = _controllerLeft.position.z },
                    Rotation = new VRRotation {
                        X = _controllerLeft.rotation.x, Y = _controllerLeft.rotation.y, Z = _controllerLeft.rotation.z, W = _controllerLeft.rotation.w
                    }
                },
                Right = new VRPose {
                    Position = new VRPosition { X = _controllerRight.position.x, Y = _controllerRight.position.y, Z = _controllerRight.position.z },
                    Rotation = new VRRotation {
                        X = _controllerRight.rotation.x, Y = _controllerRight.rotation.y, Z = _controllerRight.rotation.z, W = _controllerRight.rotation.w
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