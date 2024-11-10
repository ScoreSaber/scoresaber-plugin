using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using Newtonsoft.Json;
using ScoreSaber.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zenject;

namespace ScoreSaber.UI.PromoBanner {

    [HotReload(RelativePathToLayout = @"./PromoBannerViewController.bsml")]
    [ViewDefinition("ScoreSaber.UI.PromoBanner.PromoBannerViewController.bsml")]
    internal class PromoBannerView : BSMLAutomaticViewController {

        [Inject] private readonly PromoBanner _promoBanner = null;

        internal PromoInfo PromoInfo { get; set; }

        [UIComponent("container")]
        internal HorizontalLayoutGroup container = null;

        [UIComponent("BannerImage")]
        internal ClickableImage bannerImage = null;

        [UIObject("BannerLoading")]
        internal GameObject bannerLoading = null;

        [UIAction("BannerClicked")]
        private void BannerClicked() {
            Plugin.Log.Info("Banner clicked!");
            Application.OpenURL(PromoInfo.PromoUrl);
        }

        [UIAction("BannerDismissed")]
        private void BannerDismissed() {
            _promoBanner.Dismiss.Invoke();
        }

        [UIAction("#post-parse")]
        private void Setup() {
            bannerImage.gameObject.SetActive(false);
            bannerLoading.SetActive(true);
            GetBannerInfo().RunTask();
            bannerImage.material = Plugin.NoGlowMatRound;
        }

        public IEnumerator DownloadPromoImage(string url) {
            var handler = new DownloadHandlerTexture();
            var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            www.downloadHandler = handler;
            yield return www.SendWebRequest();

            while (!www.isDone) {
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError) {
                _promoBanner.Dismiss.Invoke();
                yield break;
            }
            if (!string.IsNullOrEmpty(www.error)) {
                _promoBanner.Dismiss.Invoke();
                yield break;
            }

            Sprite sprite = Sprite.Create(handler.texture, new Rect(0, 0, handler.texture.width, handler.texture.height), Vector2.one * 0.5f);
            bannerImage.sprite = sprite;
            bannerLoading.SetActive(false);
            yield break;
        }

        // this should contain actual logic to retrieve the current promo banner info and image
        public async Task GetBannerInfo() {
            await Task.Delay(1000); // remove this once you have actual logic to retrieve the promo banner info
            PromoInfo = new PromoInfo("https://scoresaber.com/fortnite", "Promo Title", "Promo Description", "https://scoresaber.com"); // replace this with actual promo banner info from the response eventually
            StartCoroutine(DownloadPromoImage(PromoInfo.ImageUrl));
        }
    }


    [Serializable]
    public class PromoInfo {

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
        [JsonProperty("promoTitle")]
        public string PromoTitle { get; set; }
        [JsonProperty("promoDescription")]
        public string PromoDescription { get; set; }
        [JsonProperty("promoUrl")]
        public string PromoUrl { get; set; }

        public PromoInfo(string imageUrl, string promoTitle, string promoDescription, string promoUrl) {
            ImageUrl = imageUrl;
            PromoTitle = promoTitle;
            PromoDescription = promoDescription;
            PromoUrl = promoUrl;
        }
    }
}
