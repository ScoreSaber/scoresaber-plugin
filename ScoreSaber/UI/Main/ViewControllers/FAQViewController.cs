#region

using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using UnityEngine;

#endregion

namespace ScoreSaber.UI.Main.ViewControllers {
    [HotReload]
    internal class FAQViewController : BSMLAutomaticViewController {
        [UIAction("website-clicked")]
        protected void WebsiteClicked() => Application.OpenURL("https://bit.ly/37Zp5Fq");

        [UIAction("discord-clicked")]
        protected void DiscordClicked() => Application.OpenURL("https://bit.ly/350Fd7Y");

        [UIAction("twitter-clicked")]
        protected void TwitterClicked() => Application.OpenURL("https://bit.ly/3b0aN9x");

        [UIAction("patreon-clicked")]
        protected void PatreonClicked() => Application.OpenURL("https://bit.ly/3nXRT6S");

        [UIAction("bsmg-discord-clicked")]
        protected void BSMGDiscordClicked() => Application.OpenURL("https://bit.ly/3pP8F91");

        [UIAction("bsmg-wiki-clicked")]
        protected void BSMGWikiClicked() => Application.OpenURL("https://bit.ly/3rGGsme");

        [UIAction("bsmg-patreon-clicked")]
        protected void BSMGPatreonClicked() => Application.OpenURL("https://bit.ly/34ZRmdb");

        [UIComponent("bsmg-image")]
        protected readonly ImageView _bsmgImageView = null;

        private string _scoreSaberImage = "ScoreSaber.Resources.logo-large.png";
        [UIValue("scoresaber-image")]
        public string ScoreSaberImage {
            get { return _scoreSaberImage; }
            set {
                _scoreSaberImage = value;
                NotifyPropertyChanged();
            }
        }

        private string _bsmgImage = "ScoreSaber.Resources.bsmg.jpg";
        [UIValue("bsmg-image")]
        public string BSMGImage {
            get { return _bsmgImage; }
            set {
                _bsmgImage = value;
                NotifyPropertyChanged();
            }
        }

        private int _scoreSaberCounter;
        [UIAction("scoresaber-image-clicked")]
        public void ScoreSaberImageClicked() {

            _scoreSaberCounter++;

            if (_scoreSaberCounter == 5) {
                ScoreSaberImage = "ScoreSaber.Resources.logo-flushed.png";
            }

            if (_scoreSaberCounter == 10) {
                ScoreSaberImage = "ScoreSaber.Resources.logo-large.png";
                _scoreSaberCounter = 0;
            }
        }

        private int _bsmgCounter;
        [UIAction("bsmg-image-clicked")]
        public void BsmgImageClicked() {

            _bsmgCounter++;
            if (_bsmgCounter == 5) {
                BSMGImage = "ScoreSaber.Resources.cmb.png";
            }
            if (_bsmgCounter == 10) {
                BSMGImage = "ScoreSaber.Resources.cmb-blush.png";
            }
            if (_bsmgCounter == 15) {
                BSMGImage = "ScoreSaber.Resources.bsmg.jpg";
                _bsmgCounter = 0;
            }
        }

        [UIAction("#post-parse")]
        private void Parsed() {
            _bsmgImageView.material = Plugin.NoGlowMatRound;
        }
    }
}