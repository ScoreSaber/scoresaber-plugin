#region

using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using UnityEngine;

#endregion

namespace ScoreSaber.UI.Main.ViewControllers {
    [HotReload]
    internal class FAQViewController : BSMLAutomaticViewController {
        [UIComponent("bsmg-image")] protected readonly ImageView _bsmgImageView = null;

        private int _bsmgCounter;

        private string _bsmgImage = "ScoreSaber.Resources.bsmg.jpg";

        private int _scoreSaberCounter;

        private string _scoreSaberImage = "ScoreSaber.Resources.logo-large.png";

        [UIValue("scoresaber-image")]
        public string scoreSaberImage {
            get => _scoreSaberImage;
            set {
                _scoreSaberImage = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("bsmg-image")]
        public string bsmgImage {
            get => _bsmgImage;
            set {
                _bsmgImage = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("website-clicked")]
        protected void WebsiteClicked() {
            Application.OpenURL("https://bit.ly/37Zp5Fq");
        }

        [UIAction("discord-clicked")]
        protected void DiscordClicked() {
            Application.OpenURL("https://bit.ly/350Fd7Y");
        }

        [UIAction("twitter-clicked")]
        protected void TwitterClicked() {
            Application.OpenURL("https://bit.ly/3b0aN9x");
        }

        [UIAction("patreon-clicked")]
        protected void PatreonClicked() {
            Application.OpenURL("https://bit.ly/3nXRT6S");
        }

        [UIAction("bsmg-discord-clicked")]
        protected void BSMGDiscordClicked() {
            Application.OpenURL("https://bit.ly/3pP8F91");
        }

        [UIAction("bsmg-wiki-clicked")]
        protected void BSMGWikiClicked() {
            Application.OpenURL("https://bit.ly/3rGGsme");
        }

        [UIAction("bsmg-patreon-clicked")]
        protected void BSMGPatreonClicked() {
            Application.OpenURL("https://bit.ly/34ZRmdb");
        }

        [UIAction("scoresaber-image-clicked")]
        public void ScoreSaberImageClicked() {
            _scoreSaberCounter++;
            switch (_scoreSaberCounter) {
                case 5:
                    scoreSaberImage = "ScoreSaber.Resources.logo-flushed.png";
                    break;
            }

            switch (_scoreSaberCounter) {
                case 10:
                    scoreSaberImage = "ScoreSaber.Resources.logo-large.png";
                    _scoreSaberCounter = 0;
                    break;
            }
        }

        [UIAction("bsmg-image-clicked")]
        public void BsmgImageClicked() {
            _bsmgCounter++;
            switch (_bsmgCounter) {
                case 5:
                    bsmgImage = "ScoreSaber.Resources.cmb.png";
                    break;
            }

            switch (_bsmgCounter) {
                case 10:
                    bsmgImage = "ScoreSaber.Resources.cmb-blush.png";
                    break;
            }

            switch (_bsmgCounter) {
                case 15:
                    bsmgImage = "ScoreSaber.Resources.bsmg.jpg";
                    _bsmgCounter = 0;
                    break;
            }
        }

        [UIAction("#post-parse")]
        private void Parsed() {
            _bsmgImageView.material = Plugin.NoGlowMatRound;
        }
    }
}