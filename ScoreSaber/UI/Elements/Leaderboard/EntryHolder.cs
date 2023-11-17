using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ScoreSaber.UI.Elements.Leaderboard {
    internal class EntryHolder {

        #region BSML Components
        [UIParams]
        protected BSMLParserParams parserParams = null;
#if PPV3
        [UIComponent("buttonPPv3Replay")]
        protected Button buttonPPv3Replay = null;
#endif
        #endregion

        public event Action<int> infoButtonClicked;

        [UIAction("#post-parse")]
        public void Parsed() {
#if PPV3
            if (buttonPPv3Replay != null) {
                buttonPPv3Replay.transform.localScale *= buttonScale;
                buttonPPv3Replay.gameObject.SetActive(true);
            }
#endif
        }
        public void InfoButtonClicked(int index) {

            infoButtonClicked?.Invoke(index);
        }
    }
}
