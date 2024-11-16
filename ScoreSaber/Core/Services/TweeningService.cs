using BeatmapSaveDataCommon;
using HMUI;
using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ScoreSaber.Core.Services {
    internal class TweeningService {
        [Inject] private TimeTweeningManager _tweeningManager = null;
        private HashSet<Transform> activeRotationTweens = new HashSet<Transform>();

        public void RotateTransform(Transform transform, float rotationAmount, float time, Action callback = null) {
            if (activeRotationTweens.Contains(transform)) return;
            float startRotation = transform.rotation.eulerAngles.z;
            float endRotation = startRotation + rotationAmount;

            Tween tween = new FloatTween(startRotation, endRotation, (float u) => {
                transform.rotation = Quaternion.Euler(0f, 0f, u);
            }, 0.1f, EaseType.Linear, 0f);
            tween.onCompleted = () => {
                callback?.Invoke();
                activeRotationTweens.Remove(transform);
            };
            tween.onKilled = () => {
                if (transform != null) transform.rotation = Quaternion.Euler(0f, 0f, endRotation);
                callback?.Invoke();
                activeRotationTweens.Remove(transform);
            };
            activeRotationTweens.Add(transform);
            _tweeningManager.AddTween(tween, transform);
        }

        public void FadeText(TextMeshProUGUI text, bool fadeIn, float time) {
            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;

            bool originalActiveState = text.gameObject.activeSelf;

            Tween tween = new FloatTween(startAlpha, endAlpha, (float u) => {
                if (text == null || text.gameObject.activeSelf != originalActiveState) return;
                text.color = text.color.ColorWithAlpha(u);
            }, 0.4f, EaseType.Linear, 0f);
            tween.onCompleted = () => {
                if (text == null || text.gameObject.activeSelf != originalActiveState) return;
                text.gameObject.SetActive(fadeIn);
            };
            tween.onKilled = () => {
                if (text == null || text.gameObject.activeSelf != originalActiveState) return;
                text.gameObject.SetActive(fadeIn);
                text.color = text.color.ColorWithAlpha(endAlpha);
            };
            text.gameObject.SetActive(true);
            _tweeningManager.AddTween(tween, text);
        }

        public void FadeImageView(ImageView currentImageView, bool fadeIn, float time) {
            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;

            bool originalActiveState = currentImageView.gameObject.activeSelf;

            Tween tween = new FloatTween(startAlpha, endAlpha, (float u) => {
                if (currentImageView == null || currentImageView.gameObject.activeSelf != originalActiveState) return;
                currentImageView.color = currentImageView.color.ColorWithAlpha(u);
                currentImageView.color0 = currentImageView.color.ColorWithAlpha(u);
                currentImageView.color1 = currentImageView.color.ColorWithAlpha(u);
            }, 0.4f, EaseType.Linear, 0f);
            tween.onCompleted = () => {
                if (currentImageView == null || currentImageView.gameObject.activeSelf != originalActiveState) return;
                currentImageView.color = currentImageView.color.ColorWithAlpha(endAlpha);
                currentImageView.color0 = currentImageView.color.ColorWithAlpha(endAlpha);
                currentImageView.color1 = currentImageView.color.ColorWithAlpha(endAlpha);
            };
            tween.onKilled = () => {
                if (currentImageView == null || currentImageView.gameObject.activeSelf != originalActiveState) return;
                currentImageView.color = currentImageView.color.ColorWithAlpha(endAlpha);
                currentImageView.color0 = currentImageView.color.ColorWithAlpha(endAlpha);
                currentImageView.color1 = currentImageView.color.ColorWithAlpha(endAlpha);
            };
            currentImageView.gameObject.SetActive(true);
            _tweeningManager.AddTween(tween, currentImageView);
        }

        public void LerpColor(ImageView currentImageView, Color newColor, float time = 0.0f) {

            Tween tween = new ColorTween(currentImageView.color, newColor, (Color u) => {
                currentImageView.color = u;
                currentImageView.color0 = u;
                currentImageView.color1 = u;
            }, time == 0.0f ? 0.2f : time, EaseType.Linear, 0f);
            tween.onCompleted = () => {
                if (currentImageView == null) return;
                currentImageView.color = newColor;
                currentImageView.color0 = newColor;
                currentImageView.color1 = newColor;
            };
            tween.onKilled = () => {
                if (currentImageView == null) return;
                currentImageView.color = newColor;
                currentImageView.color0 = newColor;
                currentImageView.color1 = newColor;
            };
            _tweeningManager.AddTween(tween, currentImageView);
        }

        public void FadeLayoutGroup(HorizontalOrVerticalLayoutGroup layoutGroup, bool fadeIn, float time, GameObject gameobjecttodisable = null) {
            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;

            List<CanvasRenderer> canvasRenderers = new List<CanvasRenderer>();
            GetCanvasRenderersRecursively(layoutGroup.transform, canvasRenderers);
            if(gameobjecttodisable != null) {
                GetCanvasRenderersRecursively(gameobjecttodisable.transform, canvasRenderers);
            }

            foreach (CanvasRenderer canvasRenderer in canvasRenderers) {
                canvasRenderer.SetAlpha(startAlpha);
            }

            Tween tween = new Tweening.FloatTween(startAlpha, endAlpha, (float u) => {
                foreach (CanvasRenderer canvasRenderer in canvasRenderers) {
                    canvasRenderer.SetAlpha(u);
                }
            }, time, EaseType.Linear, 0f);

            tween.onCompleted = () => {
                if (layoutGroup == null) return;
                layoutGroup.gameObject.SetActive(fadeIn);
            };
            tween.onKilled = () => {
                if (layoutGroup == null) return;
                layoutGroup.gameObject.SetActive(fadeIn);
                foreach (CanvasRenderer canvasRenderer in canvasRenderers) {
                    canvasRenderer.SetAlpha(endAlpha);
                }
            };

            layoutGroup.gameObject.SetActive(true);
            _tweeningManager.AddTween(tween, layoutGroup);
        }

        private void GetCanvasRenderersRecursively(Transform parent, List<CanvasRenderer> canvasRenderers) {
            foreach (Transform child in parent) {
                CanvasRenderer canvasRenderer = child.GetComponent<CanvasRenderer>();
                if (canvasRenderer != null) {
                    canvasRenderers.Add(canvasRenderer);
                }

                GetCanvasRenderersRecursively(child, canvasRenderers);
            }
        }
    }
}
