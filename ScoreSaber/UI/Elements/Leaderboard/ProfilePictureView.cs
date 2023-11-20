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
        
        private readonly int index;

        public bool isLoading = false;

        public ProfilePictureView(int index) {
            this.index = index;
        }

        [UIComponent("profileImage")]
        public ImageView profileImage = null;

        [UIObject("profileloading")]
        public GameObject loadingIndicator = null;


        [UIAction("#post-parse")]
        public void Parsed() {
            profileImage.material = Plugin.NoGlowMatRound;
            profileImage.gameObject.SetActive(false);
            loadingIndicator.gameObject.SetActive(false);
        }

        public void setProfileImage(string url, int pos, CancellationToken cancellationToken) {
            try {
                if (SpriteCache.cachedSprites.ContainsKey(url)) {
                    profileImage.gameObject.SetActive(true);
                    profileImage.sprite = SpriteCache.cachedSprites[url];
                    loadingIndicator.gameObject.SetActive(false);
                    return;
                }

                loadingIndicator.gameObject.SetActive(true);
                SharedCoroutineStarter.instance.StartCoroutine(GetSpriteAvatar(url, OnAvatarDownloadSuccess, OnAvatarDownloadFailure, cancellationToken, pos));
            } catch (OperationCanceledException) {
                OnAvatarDownloadFailure("Cancelled", pos, cancellationToken);
            } finally {
                SpriteCache.MaintainSpriteCache();
            }
        }

        internal static IEnumerator GetSpriteAvatar(string url, Action<Sprite, int, string, CancellationToken> onSuccess, Action<string, int, CancellationToken> onFailure, CancellationToken cancellationToken, int pos) {
            var handler = new DownloadHandlerTexture();
            var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            www.downloadHandler = handler;
            cancellationToken.ThrowIfCancellationRequested();
            yield return www.SendWebRequest();

            while (!www.isDone) {
                if (cancellationToken.IsCancellationRequested) {
                    onFailure?.Invoke("Cancelled", pos, cancellationToken);
                    yield break;
                }

                yield return null;
            }
            if (www.isHttpError || www.isNetworkError) {
                onFailure?.Invoke(www.error, pos, cancellationToken);
                yield break;
            }
            if (!string.IsNullOrEmpty(www.error)) {
                onFailure?.Invoke(www.error, pos, cancellationToken);
                yield break;
            }
            Sprite sprite = Sprite.Create(handler.texture, new Rect(0, 0, handler.texture.width, handler.texture.height), Vector2.one * 0.5f);
            onSuccess?.Invoke(sprite, pos, url, cancellationToken);
        }

        internal void OnAvatarDownloadSuccess(Sprite a, int pos, string url, CancellationToken cancellationToken) {
            if (cancellationToken == null || cancellationToken.IsCancellationRequested) {
                Clear();
                return;
            }
            profileImage.gameObject.SetActive(true);
            profileImage.sprite = a;
            loadingIndicator.gameObject.SetActive(false);
            SpriteCache.AddSpriteToCache(url, a);
        }

        internal void OnAvatarDownloadFailure(string error, int pos, CancellationToken cancellationToken) {
            Clear();
        }

        internal void Clear() {
            profileImage.sprite = BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite;
            loadingIndicator.gameObject.SetActive(false);
        }
        
    }

    internal static class SpriteCache {
        internal static Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();
        private static int MaxSpriteCacheSize = 150;
        internal static Queue<string> spriteCacheQueue = new Queue<string>();
        internal static void MaintainSpriteCache() {
            while (cachedSprites.Count > MaxSpriteCacheSize) {
                string oldestUrl = spriteCacheQueue.Dequeue();
                cachedSprites.Remove(oldestUrl);
            }
        }

        internal static void AddSpriteToCache(string url, Sprite sprite) {
            if(cachedSprites.ContainsKey(url)) {
                return;
            }
            cachedSprites.Add(url, sprite);
            spriteCacheQueue.Enqueue(url);
        }
    }
}
