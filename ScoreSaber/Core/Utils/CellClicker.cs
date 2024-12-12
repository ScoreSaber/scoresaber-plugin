using BeatSaberMarkupLanguage;
using HMUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;

namespace ScoreSaber.Core.Utils {
    public class CellClicker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
        public Action<int> onClick;
        public int index;
        public ImageView seperator;
        public Vector3 originalScale = new Vector3(1, 1, 1);
        private bool isScaled = false;
        public bool isCool = false;

        private Color origColour = new Color(1, 1, 1, 1);
        private Color origColour0 = new Color(1, 1, 1, 0.2509804f);
        private Color origColour1 = new Color(1, 1, 1, 0);

        public bool clickable = false;
        private void Start() {
            seperator.transform.localScale = originalScale;
        }

        public void OnPointerClick(PointerEventData data) {
            if (!clickable) return;
            BeatSaberUI.BasicUIAudioManager.HandleButtonClickEvent();
            onClick(index);
        }

        public void Update() {
            if (isCool) {
                float hue = Mathf.PingPong(Time.time * 0.5f, 1);
                Color rainbowColor = Color.HSVToRGB(hue, 1, 1);
                seperator.color = rainbowColor;
                seperator.color0 = rainbowColor;
                seperator.color1 = rainbowColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (!clickable) return;
            if (!isScaled) {
                seperator.transform.localScale = originalScale * 1.8f;
                isScaled = true;
            }

            Color targetColor = Color.white;
            Color targetColor0 = Color.white;
            Color targetColor1 = new Color(1, 1, 1, 0);

            float lerpDuration = 0.15f;

            StopAllCoroutines();
            StartCoroutine(LerpColors(seperator, seperator.color, targetColor, seperator.color0, targetColor0, seperator.color1, targetColor1, lerpDuration));
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (!clickable) return;
            if (isScaled) {
                seperator.transform.localScale = originalScale;
                isScaled = false;
            }

            float lerpDuration = 0.05f;

            StopAllCoroutines();
            StartCoroutine(LerpColors(seperator, seperator.color, origColour, seperator.color0, origColour0, seperator.color1, origColour1, lerpDuration));
        }

        public void ResetColourAndSize() {
            seperator.transform.localScale = originalScale;
            isScaled = false;
            StopAllCoroutines();
            seperator.color = origColour;
            seperator.color0 = origColour0;
            seperator.color1 = origColour1;
        }

        private IEnumerator LerpColors(ImageView target, Color startColor, Color endColor, Color startColor0, Color endColor0, Color startColor1, Color endColor1, float duration) {
            float elapsedTime = 0f;
            while (elapsedTime < duration) {
                float t = elapsedTime / duration;
                target.color = Color.Lerp(startColor, endColor, t);
                target.color0 = Color.Lerp(startColor0, endColor0, t);
                target.color1 = Color.Lerp(startColor1, endColor1, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            target.color = endColor;
            target.color0 = endColor0;
            target.color1 = endColor1;
        }

        private void OnDestroy() {
            StopAllCoroutines();
            onClick = null;
            seperator.color = origColour;
            seperator.color0 = origColour0;
            seperator.color1 = origColour1;
        }
    }
}
