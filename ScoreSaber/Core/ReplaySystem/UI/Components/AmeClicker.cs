#region

using System;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

namespace ScoreSaber.Core.ReplaySystem.UI.Components {
    internal class AmeClicker : MonoBehaviour, IPointerClickHandler {
        private Action<float> _clickCallback;
        private RectTransform _rect;

        public void OnPointerClick(PointerEventData eventData) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position,
                eventData.pressEventCamera, out Vector2 computedVector);
            float maxX = _rect.rect.width / 2f;
            _clickCallback?.Invoke(Mathf.InverseLerp(-maxX, maxX, computedVector.x));
        }

        internal void Setup(Action<float> callback) {
            _rect = transform as RectTransform;
            _clickCallback = callback;
        }
    }
}