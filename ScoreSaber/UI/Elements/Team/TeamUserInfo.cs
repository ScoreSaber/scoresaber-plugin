#region

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

#endregion

namespace ScoreSaber.UI.Elements.Team {
    internal class TeamUserInfo : INotifyPropertyChanged {

        private string _usernameText;
        [UIValue("username")]
        protected string UsernameText {
            get => _usernameText;
            set {
                _usernameText = value;
                NotifyPropertyChanged();
            }
        }

        private string _discordLink;
        protected string DiscordLink {
            get => _discordLink;
            set {
                _discordLink = value ?? null;
                NotifyPropertyChanged("hasDiscord");
            }
        }

        private string _githubLink;
        protected string GithubLink {
            get => _githubLink;
            set {
                if (value == null) {
                    _githubLink = null;
                } else {
                    _githubLink = $"https://github.com/{value}";
                }
                NotifyPropertyChanged("hasGithub");
            }
        }

        private string _twitchLink;
        protected string TwitchLink {
            get => _twitchLink;
            set {
                _twitchLink = value == null ? null : $"https://www.twitch.tv/{value}";
                NotifyPropertyChanged("hasTwitch");
            }
        }

        private string _twitterLink;
        protected string TwitterLink {
            get => _twitterLink;
            set {
                _twitterLink = value == null ? null : $"https://twitter.com/{value}";
                NotifyPropertyChanged("hasTwitter");
            }
        }

        private string _youtubeLink;
        protected string YoutubeLink {
            get => _youtubeLink;
            set {
                _youtubeLink = value == null ? null : $"https://www.youtube.com/channel/{value}";
                NotifyPropertyChanged("hasYoutube");
            }
        }

        private readonly string _profilePictureTemp;
        private bool _loaded;

        [UIValue("discord")]
        private bool _hasDiscord => _discordLink != null;
        [UIValue("github")]
        private bool _hasGithub => _githubLink != null;
        [UIValue("twitch")]
        private bool _hasTwitch => _twitchLink != null;
        [UIValue("twitter")]
        private bool _hasTwitter => _twitterLink != null;
        [UIValue("youtube")]
        private bool _hasYoutube => _youtubeLink != null;

        [UIComponent("username-text")]
        protected readonly CurvedTextMeshPro _usernameTextComponent = null;

        [UIComponent("profile-image")]
        protected readonly ImageView _profilePictureComponent = null;

        public TeamUserInfo(string _profilePicture, string _username, string _discord = null, string _github = null, string _twitch = null, string _twitter = null, string _youtube = null) {

            _profilePictureTemp = _profilePicture;
            UsernameText = _username;
            DiscordLink = _discord;
            GithubLink = _github;
            TwitchLink = _twitch;
            TwitterLink = _twitter;
            YoutubeLink = _youtube;
        }

        public void LoadImage() {

            if (!_loaded) {
                if (_profilePictureTemp != null) {
                    SetImage(_profilePictureTemp);
                }
                _loaded = true;
            }
        }

        private void SetImage(string image) {

            if (_profilePictureComponent != null) {
                _profilePictureComponent.SetImage($"https://raw.githubusercontent.com/Umbranoxio/ScoreSaber-Team/main/images/{image}");
            } else {
                Plugin.Log.Info("ProfilePictureComponent is null");
            }
        }

        public int clickCounter;
        [UIAction("username-click")]
        public void UsernameClick() {

            if (UsernameText == "Umbranox") {
                if (clickCounter < 5) {
                    clickCounter++;
                }
                if (clickCounter == 5) {
                    SetImage("r.jpg");
                    UsernameText = "🌧 Rain ❤";
                    DiscordLink = "128460955272216576";
                    TwitterLink = "VaporRain";
                    TwitchLink = "inkierain";
                    NotifyPropertyChanged("profilePicture");
                    YoutubeLink = null;
                    GithubLink = null;
                }
            }
        }

        [UIAction("#post-parse")]
        protected void Parsed() {

            _profilePictureComponent.material = Plugin.NoGlowMatRound;
            _usernameTextComponent.fontSizeMax = 5.5f;
            _usernameTextComponent.fontSizeMin = 2.5f;
            _usernameTextComponent.enableAutoSizing = true;
        }

        [UIAction("discord-clicked")]
        protected void DiscordClicked() {
            Application.OpenURL(_discordLink);
        }

        [UIAction("github-clicked")]
        protected void GitHubClicked() {
            Application.OpenURL(_githubLink);
        }

        [UIAction("twitter-clicked")]
        protected void TwitchClicked() {
            Application.OpenURL(_twitchLink);
        }

        [UIAction("twitch-clicked")]
        protected void TwitterClicked() {
            Application.OpenURL(_twitterLink);
        }


        [UIAction("youtube-clicked")]
        protected void YoutubeClicked() {
            Application.OpenURL(_youtubeLink);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}