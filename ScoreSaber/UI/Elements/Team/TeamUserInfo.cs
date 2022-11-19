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
        [UIComponent("profile-image")] protected readonly ImageView _profilePictureComponent = null;

        private readonly string _profilePictureTemp;

        [UIComponent("username-text")] protected readonly CurvedTextMeshPro _usernameTextComponent = null;

        private string _discordLink;

        private string _githubLink;
        private bool _loaded;

        private string _twitchLink;

        private string _twitterLink;

        private string _usernameText;

        private string _youtubeLink;

        public int clickCounter;

        public TeamUserInfo(string _profilePicture, string _username, string _discord = null, string _github = null,
            string _twitch = null, string _twitter = null, string _youtube = null) {
            switch (_username) {
                case "williums":
                    _username =
                        "<color=#FF0000>w</color><color=#FF7F00>i</color><color=#FFFF00>l</color><color=#00FF00>l</color><color=#0000FF>i</color><color=#4B0082>u</color><color=#8B00FF>m</color><color=#FF0000>s</color>";
                    break;
            }

            _profilePictureTemp = _profilePicture;
            usernameText = _username;
            discordLink = _discord;
            githubLink = _github;
            twitchLink = _twitch;
            twitterLink = _twitter;
            youtubeLink = _youtube;
        }

        [UIValue("username")]
        protected string usernameText {
            get => _usernameText;
            set {
                _usernameText = value;
                NotifyPropertyChanged();
            }
        }

        protected string discordLink {
            get => _discordLink;
            set {
                switch (value) {
                    case null:
                        _discordLink = null;
                        break;
                    default:
                        _discordLink = value;
                        break;
                }

                NotifyPropertyChanged("hasDiscord");
            }
        }

        protected string githubLink {
            get => _githubLink;
            set {
                switch (value) {
                    case null:
                        _githubLink = null;
                        break;
                    default:
                        _githubLink = $"https://github.com/{value}";
                        break;
                }

                NotifyPropertyChanged("hasGithub");
            }
        }

        protected string twitchLink {
            get => _twitchLink;
            set {
                switch (value) {
                    case null:
                        _twitchLink = null;
                        break;
                    default:
                        _twitchLink = $"https://www.twitch.tv/{value}";
                        break;
                }

                NotifyPropertyChanged("hasTwitch");
            }
        }

        protected string twitterLink {
            get => _twitterLink;
            set {
                switch (value) {
                    case null:
                        _twitterLink = null;
                        break;
                    default:
                        _twitterLink = $"https://twitter.com/{value}";
                        break;
                }

                NotifyPropertyChanged("hasTwitter");
            }
        }

        protected string youtubeLink {
            get => _youtubeLink;
            set {
                switch (value) {
                    case null:
                        _youtubeLink = null;
                        break;
                    default:
                        _youtubeLink = $"https://www.youtube.com/channel/{value}";
                        break;
                }

                NotifyPropertyChanged("hasYoutube");
            }
        }

        [UIValue("discord")] private bool _hasDiscord => _discordLink != null;

        [UIValue("github")] private bool _hasGithub => _githubLink != null;

        [UIValue("twitch")] private bool _hasTwitch => _twitchLink != null;

        [UIValue("twitter")] private bool _hasTwitter => _twitterLink != null;

        [UIValue("youtube")] private bool _hasYoutube => _youtubeLink != null;

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadImage() {
            switch (_loaded) {
                case false: {
                    if (_profilePictureTemp != null) {
                        SetImage(_profilePictureTemp);
                    }

                    _loaded = true;
                    break;
                }
            }
        }

        private void SetImage(string image) {
            if (_profilePictureComponent != null) {
                _profilePictureComponent.SetImage(
                    $"https://raw.githubusercontent.com/Umbranoxio/ScoreSaber-Team/main/images/{image}");
            } else {
                Plugin.Log.Info("ProfilePictureComponent is null");
            }
        }

        [UIAction("username-click")]
        public void UsernameClick() {
            switch (usernameText) {
                case "Umbranox": {
                    switch (clickCounter < 5) {
                        case true:
                            clickCounter++;
                            break;
                    }

                    switch (clickCounter) {
                        case 5:
                            SetImage("r.jpg");
                            usernameText = "🌧 Rain ❤";
                            discordLink = "128460955272216576";
                            twitterLink = "VaporRain";
                            twitchLink = "inkierain";
                            NotifyPropertyChanged("profilePicture");
                            youtubeLink = null;
                            githubLink = null;
                            break;
                    }

                    break;
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

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}