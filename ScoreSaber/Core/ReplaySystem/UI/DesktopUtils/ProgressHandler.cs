using HMUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;

namespace ScoreSaber.Core.ReplaySystem.UI {
    public class ProgressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

        public ImageView timebarActive;
        public ImageView timebarBackground;

        public event Action<float> OnProgressUpdated;

        public delegate void upClickEvent();
        public event upClickEvent upClick;

        public delegate void downClickEvent();
        public event downClickEvent downClick;

        private bool isDragging = false;
        private float minX = -19f;
        private float maxX = 19f;

        private Vector3 originalScale;
        private Vector3 hoverScale;
        private float scaleSpeed = 0.1f;

        public void UpdateProgress(float progress) {
            OnProgressUpdated?.Invoke(progress);
        }

        private void Start() {
            originalScale = timebarActive.transform.localScale;
            hoverScale = new Vector3(originalScale.x, originalScale.y * 1.2f, originalScale.z);
        }

        public void OnPointerClick(PointerEventData eventData) {
            UpdateTimebarPosition(eventData);
        }

        public void OnPointerDown(PointerEventData eventData) {
            isDragging = true;
            downClick?.Invoke();
            UpdateTimebarPosition(eventData);
        }

        public void OnPointerUp(PointerEventData eventData) {
            isDragging = false;
            upClick?.Invoke();
            UpdateTimebarPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData) {
            if (isDragging) {
                UpdateTimebarPosition(eventData);
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            StopAllCoroutines();
            StartCoroutine(SmoothScale(timebarActive.transform, hoverScale));
        }

        public void OnPointerExit(PointerEventData eventData) {
            StopAllCoroutines();
            StartCoroutine(SmoothScale(timebarActive.transform, originalScale));
        }

        private void UpdateTimebarPosition(PointerEventData eventData) {
            RectTransform timebarRect = timebarBackground.rectTransform;
            Vector2 localPoint;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(timebarRect, eventData.position, eventData.pressEventCamera, out localPoint)) {
                float clampedX = Mathf.Clamp(localPoint.x, minX, maxX);

                timebarActive.rectTransform.anchoredPosition = new Vector2(clampedX, 0);

                float progress = Mathf.InverseLerp(minX, maxX, clampedX);

                OnProgressUpdated?.Invoke(progress);
            }
        }

        private IEnumerator SmoothScale(Transform target, Vector3 targetScale)
        {
            while (Vector3.Distance(target.localScale, targetScale) > 0.01f)
            {
                target.localScale = Vector3.Lerp(target.localScale, targetScale, scaleSpeed * Time.deltaTime);
                yield return new WaitForFixedUpdate();
            }
            target.localScale = targetScale;
        }
    }
}
