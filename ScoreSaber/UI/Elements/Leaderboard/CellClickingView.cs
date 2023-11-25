using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ScoreSaber.UI.Elements.Leaderboard {
    internal class CellClickingView {
        int index;

        public CellClickingView(int index) {
            this.index = index;
        }

        internal Sprite nullSprite = BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite;

        [UIComponent("cellClickerImage")]
        public ImageView cellClickerImage = null;

        [UIAction("post-parse")]
        public void Parsed() {
            cellClickerImage.sprite = nullSprite;
            cellClickerImage.material = Plugin.NoGlowMatRound;
        }
    }
}
