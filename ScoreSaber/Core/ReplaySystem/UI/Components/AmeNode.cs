#region

using System;
using UnityEngine;

#endregion

namespace ScoreSaber.Core.ReplaySystem.UI.Components {
    public class AmeNode : MonoBehaviour {
        public event Action<float> PositionDidChange;

        private AmeHandle _handle;
        private Action<AmeNode, Vector2, Camera> _callback;
        public bool IsBeingDragged => _handle.Dragged;

        public bool Moveable { get; set; } = true;
        public float Min { get; set; }
        public float Max { get; set; } = 1f;

        internal void Init(AmeHandle handle) {

            _handle = handle;
        }

        public void AddCallback(Action<AmeNode, Vector2, Camera> dragCallback) {

            _callback = dragCallback;
            _handle.AddCallback(Callback);
        }

        private void Callback(AmeHandle handle, Vector2 x, Camera camera) {

            if (Moveable) {
                _callback?.Invoke(this, x, camera);
            }
        }

        public void SendUpdatePositionCall(float percentOnBar) {

            PositionDidChange?.Invoke(percentOnBar);
        }
    }
}