using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities.Async;
using ScoreSaber.Core.Services;
using ScoreSaber.Core.Utils;
using ScoreSaber.UI.Leaderboard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Tweening;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace ScoreSaber.UI.Elements.Leaderboard {
    internal class ProfilePictureView {
        
        private readonly int index;

        public bool isLoading = false;

        private ICoroutineStarter coroutineStarter;
        private TweeningUtils _tweeningUtils;

        internal Sprite nullSprite => Utilities.ImageResources.BlankSprite;

        public ProfilePictureView(int index) {
            this.index = index;
        }

        [Inject]
        public void Init(ICoroutineStarter coroutineStarter, TweeningUtils tweeningUtils) {
            this.coroutineStarter = coroutineStarter;
            this._tweeningUtils = tweeningUtils;
        }

        [UIComponent("profileImage")]
        public ImageView profileImage = null;

        [UIObject("profileloading")]
        public GameObject loadingIndicator = null;

        [UIAction("#post-parse")]
        public void Parsed() {
            profileImage.material = Plugin.NoGlowMatRound;
            profileImage.gameObject.SetActive(true);
            loadingIndicator.gameObject.SetActive(false);
        }

        public void setProfileImage(string url, int pos, CancellationToken cancellationToken) {
            try {
                cancellationToken.ThrowIfCancellationRequested();
                if (SpriteCache.cachedSprites.ContainsKey(url)) {
                    profileImage.sprite = SpriteCache.cachedSprites[url];
                    _tweeningUtils.FadeImageView(profileImage, true, 0.2f);
                    loadingIndicator.gameObject.SetActive(false);
                    return;
                }
                loadingIndicator.gameObject.SetActive(true);
                coroutineStarter.StartCoroutine(GetSpriteAvatar(url, OnAvatarDownloadSuccess, OnAvatarDownloadFailure, cancellationToken, pos));
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
            yield return www.SendWebRequest();

            while (!www.isDone) {
                if (cancellationToken.IsCancellationRequested) {
                    onFailure?.Invoke("Cancelled", pos, cancellationToken);
                    yield break;
                }
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError) {
                onFailure?.Invoke(www.error, pos, cancellationToken);
                yield break;
            }
            if (!string.IsNullOrEmpty(www.error)) {
                onFailure?.Invoke(www.error, pos, cancellationToken);
                yield break;
            }

            Sprite sprite = Sprite.Create(handler.texture, new Rect(0, 0, handler.texture.width, handler.texture.height), Vector2.one * 0.5f);
            onSuccess?.Invoke(sprite, pos, url, cancellationToken);
            yield break;
        }

        internal void OnAvatarDownloadSuccess(Sprite a, int pos, string url, CancellationToken cancellationToken) {
            SpriteCache.AddSpriteToCache(url, a);
            if (cancellationToken != null) {
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }
            }
            profileImage.sprite = a;
            _tweeningUtils.FadeImageView(profileImage, true, 0.2f);
            loadingIndicator.gameObject.SetActive(false);
        }

        internal void OnAvatarDownloadFailure(string error, int pos, CancellationToken cancellationToken) {
            if (cancellationToken != null) {
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }
            }
            ClearSprite();
        }
        
        public void ClearSprite() {
            if (profileImage != null) {
                profileImage.sprite = nullSprite;
            }
            if(loadingIndicator != null) {
                loadingIndicator.gameObject.SetActive(false);
            }
        }

        public void Active(bool state) {
            if (profileImage != null) {
                profileImage.sprite = nullSprite;
            }
        }
    }



    
}
