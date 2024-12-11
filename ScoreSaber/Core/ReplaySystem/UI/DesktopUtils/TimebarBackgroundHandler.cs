using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;

namespace ScoreSaber.Core.ReplaySystem.UI {
    public class TimebarBackgroundHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {

        public ProgressHandler progressHandler;

        private float minX = -19f;
        private float maxX = 19f;

        public delegate void upClickEvent();
        public event upClickEvent upClick;

        public delegate void downClickEvent();
        public event downClickEvent downClick;

        public void OnDrag(PointerEventData eventData) {
            OnPointerDown(eventData);
        }

        public void OnPointerDown(PointerEventData eventData) {
            RectTransform timebarRect = GetComponent<RectTransform>();
            Vector2 localPoint;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(timebarRect, eventData.position, eventData.pressEventCamera, out localPoint)) {
                float clampedX = Mathf.Clamp(localPoint.x, minX, maxX);

                progressHandler.timebarActive.rectTransform.anchoredPosition = new Vector2(clampedX, 0);

                float progress = Mathf.InverseLerp(minX, maxX, clampedX);

                progressHandler.UpdateProgress(progress);
            }
            downClick?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData) {
            upClick?.Invoke();
        }
    }
}
