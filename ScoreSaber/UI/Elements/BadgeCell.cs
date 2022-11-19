#region

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#endregion

namespace ScoreSaber.UI.Elements {
    internal class BadgeCell : INotifyPropertyChanged {
        [UIComponent("image")] protected readonly ImageView _image = null;

        private string _hoverHint = "";

        [UIValue("hover-hint")]
        protected string hoverHint {
            get => _hoverHint;
            set {
                _hoverHint = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetData(string imageURL, string hoverHintText) {
            _image.SetImage(imageURL);
            hoverHint = hoverHintText;
        }

        public void SetActive(bool value) {
            _image.gameObject.SetActive(value);
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}