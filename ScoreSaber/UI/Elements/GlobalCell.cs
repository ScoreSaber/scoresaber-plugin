#region

using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using TMPro;
using UnityEngine;

#endregion

namespace ScoreSaber.UI.Elements {
    internal class GlobalCell {
        private readonly string _identifier;
        private readonly Action<string, string> _profileClicked;

        private readonly int _weeklyChange;

        public GlobalCell(string id, string avatarUrl, string username, string country, string rank, int weeklyChange,
            double pp, Action<string, string> onActivateProfile = null) {
            _identifier = id;
            _avatarUrl = avatarUrl;
            _ppText = $"<color=#6772E5>{pp:n0}pp</color>";
            _username = username;
            _globalRank = rank;
            _weeklyChange = weeklyChange;
            _profileClicked = onActivateProfile;
            _countryText = $"{country}";
            _flagUrl = $"https://cdn.scoresaber.com/flags/{country.ToLower()}.png";
            switch (id) {
                case PlayerIDs.Williums:
                    _username =
                        "<color=#FF0000>w</color><color=#FF7F00>i</color><color=#FFFF00>l</color><color=#00FF00>l</color><color=#0000FF>i</color><color=#4B0082>u</color><color=#8B00FF>m</color><color=#FF0000>s</color>";
                    break;
            }
        }

        [UIAction("profile-clicked")]
        private void ProfileClicked() {
            _profileClicked?.Invoke(_identifier, _username);
        }

        [UIAction("#post-parse")]
        private void Parsed() {
            _imageView.material = Plugin.NoGlowMatRound;
            switch (_weeklyChange > 0) {
                case true:
                    _weeklyText.text = "+" + _weeklyChange;
                    _weeklyText.color = Color.green;
                    break;
                default: {
                    switch (_weeklyChange < 0) {
                        case true:
                            _weeklyText.text = _weeklyChange.ToString();
                            _weeklyText.color = Color.red;
                            break;
                        default:
                            _weeklyText.text = _weeklyChange.ToString();
                            break;
                    }

                    break;
                }
            }
        }

        #region BSML Components

        [UIComponent("profile-image")] private readonly ImageView _imageView = null;

        [UIComponent("weekly-text")] private readonly TextMeshProUGUI _weeklyText = null;

        #endregion

        #region BSML Values

        [UIValue("pfp-url")] private readonly string _avatarUrl;

        [UIValue("username")] private readonly string _username;

        [UIValue("rank")] private readonly string _globalRank;

        [UIValue("pp")] private readonly string _ppText;

        [UIValue("flag-url")] private readonly string _flagUrl;

        [UIValue("country")] private readonly string _countryText;

        #endregion
    }
}