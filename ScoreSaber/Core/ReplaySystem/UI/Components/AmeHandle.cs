#region

using System;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

namespace ScoreSaber.Core.ReplaySystem.UI.Components {
    public class AmeHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEventSystemHandler, IInitializePotentialDragHandler, IEndDragHandler {
        public bool Dragged { get; private set; }
        private Action<AmeHandle, Vector2, Camera> _handleMoveCallback;

        public void AddCallback(Action<AmeHandle, Vector2, Camera> callback) {

            _handleMoveCallback = callback;
        }

        public void OnBeginDrag(PointerEventData eventData) {

            Dragged = true;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) {

            Dragged = true;
            eventData.useDragThreshold = false;
        }

        public void OnDrag(PointerEventData eventData) {

            if (eventData.position.x == 0f) {
                return;
            }

            _handleMoveCallback?.Invoke(this, eventData.position, eventData.pressEventCamera);
        }

        public void OnEndDrag(PointerEventData eventData) {

            Dragged = false;
        }
    }
}