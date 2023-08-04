using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private List<ImageView> imageViews { get; set; }

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
        }

        public void HideImageViews() {
            if (imageViews != null) {
                for (int i = 0; i < imageViews.Count; i++) {
                    imageViews[i].gameObject.SetActive(false);
                }
            }
        }

        public void SetImages(List<string> urls) {
            for (int i = 0; i < urls.Count; i++) {
                imageViews[i].gameObject.SetActive(true);
                imageViews[i].SetImage(urls[i]);
            }
        }
    }
}
