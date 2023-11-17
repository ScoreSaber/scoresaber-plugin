using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ScoreSaber.UI.Elements.Leaderboard {
    internal class ProfilePictureView {

        [UIParams]
        protected BSMLParserParams parserParams = null;
        [UIComponent("pfpimageView1")]
        protected ImageView imageView1 = null;
        [UIComponent("pfpimageView2")]
        protected ImageView imageView2 = null;
        [UIComponent("pfpimageView3")]
        protected ImageView imageView3 = null;
        [UIComponent("pfpimageView4")]
        protected ImageView imageView4 = null;
        [UIComponent("pfpimageView5")]
        protected ImageView imageView5 = null;
        [UIComponent("pfpimageView6")]
        protected ImageView imageView6 = null;
        [UIComponent("pfpimageView7")]
        protected ImageView imageView7 = null;
        [UIComponent("pfpimageView8")]
        protected ImageView imageView8 = null;
        [UIComponent("pfpimageView9")]
        protected ImageView imageView9 = null;
        [UIComponent("pfpimageView10")]
        protected ImageView imageView10 = null;

        [UIObject("pfploadingIndicator1")]
        protected GameObject loadingIndicator1 = null;
        [UIObject("pfploadingIndicator2")]
        protected GameObject loadingIndicator2 = null;
        [UIObject("pfploadingIndicator3")]
        protected GameObject loadingIndicator3 = null;
        [UIObject("pfploadingIndicator4")]
        protected GameObject loadingIndicator4 = null;
        [UIObject("pfploadingIndicator5")]
        protected GameObject loadingIndicator5 = null;
        [UIObject("pfploadingIndicator6")]
        protected GameObject loadingIndicator6 = null;
        [UIObject("pfploadingIndicator7")]
        protected GameObject loadingIndicator7 = null;
        [UIObject("pfploadingIndicator8")]
        protected GameObject loadingIndicator8 = null;
        [UIObject("pfploadingIndicator9")]
        protected GameObject loadingIndicator9 = null;
        [UIObject("pfploadingIndicator10")]
        protected GameObject loadingIndicator10 = null;


        private List<ImageView> imageViews { get; set; }
        private List<GameObject> loadingIndicators { get; set; }

        [UIAction("#post-parse")]
        public void Parsed() {
            imageViews = new List<ImageView> {
                imageView1,
                imageView2,
                imageView3,
                imageView4,
                imageView5,
                imageView6,
                imageView7,
                imageView8,
                imageView9,
                imageView10
            };

            loadingIndicators = new List<GameObject> {
                loadingIndicator1,
                loadingIndicator2,
                loadingIndicator3,
                loadingIndicator4,
                loadingIndicator5,
                loadingIndicator6,
                loadingIndicator7,
                loadingIndicator8,
                loadingIndicator9,
                loadingIndicator10
            };

            Material[] allMaterials = Resources.FindObjectsOfTypeAll<Material>();
            Material cool = null;

            foreach (Material material in allMaterials) {
                if (material == null) continue;
                if (material.name.Contains("UINoGlowRoundEdge")) {
                    cool = material;
                    break;
                }
            }

            if (cool == null) {
                Plugin.Log.Error("Material 'UINoGlowRoundEdge' not found.");
                return;
            }

            foreach (ImageView imageView in imageViews) {
                imageView.material = cool;
            }
        }

        public void HideImageViews() {
            if (imageViews != null) {
                for (int i = 0; i < imageViews.Count; i++) {
                    imageViews[i].gameObject.SetActive(false);
                }
            }
        }

        public void SetImages(List<string> urls, CancellationToken cancellationToken) {
            for (int i = 0; i < urls.Count; i++) {
                setProfileImage(urls[i], i, cancellationToken);
            }
            for (int i = urls.Count + 1; i < imageViews.Count; i++) {
                imageViews[i].gameObject.SetActive(false);
            }
        }

        public void setProfileImage(string url, int pos, CancellationToken cancellationToken) {
            try {
                if (cachedSprites.ContainsKey(url)) {
                    imageViews[pos].gameObject.SetActive(true);
                    imageViews[pos].sprite = cachedSprites[url];
                    loadingIndicators[pos].gameObject.SetActive(false);
                    return;
                }
                loadingIndicators[pos].gameObject.SetActive(true);
                SharedCoroutineStarter.instance.StartCoroutine(GetSpriteAvatar(url, OnAvatarDownloadSuccess, OnAvatarDownloadFailure, cancellationToken, pos));
            } catch (OperationCanceledException) {
                OnAvatarDownloadFailure("Cancelled", pos);
            }
        }

        internal static IEnumerator GetSpriteAvatar(string url, Action<Sprite, int, string> onSuccess, Action<string, int> onFailure, CancellationToken cancellationToken, int pos) {
            var handler = new DownloadHandlerTexture();
            var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            www.downloadHandler = handler;
            cancellationToken.ThrowIfCancellationRequested();
            yield return www.SendWebRequest();

            while (!www.isDone) {
                if (cancellationToken.IsCancellationRequested) {
                    onFailure?.Invoke("Cancelled", pos);
                    yield break;
                }

                yield return null;
            }
            if (www.isHttpError || www.isNetworkError) {
                onFailure?.Invoke(www.error, pos);
                yield break;
            }
            if (!string.IsNullOrEmpty(www.error)) {
                onFailure?.Invoke(www.error, pos);
                yield break;
            }
            Sprite sprite = Sprite.Create(handler.texture, new Rect(0, 0, handler.texture.width, handler.texture.height), Vector2.one * 0.5f);
            onSuccess?.Invoke(sprite, pos, url);
        }

        internal void OnAvatarDownloadSuccess(Sprite a, int pos, string url) 
        {
            imageViews[pos].gameObject.SetActive(true);
            imageViews[pos].sprite = a;
            loadingIndicators[pos].gameObject.SetActive(false);
            cachedSprites.Add(url, a);
        }

        internal void OnAvatarDownloadFailure(string error, int pos) {
            loadingIndicators[pos].gameObject.SetActive(false);
            imageViews[pos].gameObject.SetActive(false);
        }

        internal static Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();

    }
}
