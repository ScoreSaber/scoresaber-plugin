#region

using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

#endregion

namespace ScoreSaber.UI.Elements.Leaderboard {
    internal class InfoButtonsView {

        #region BSML Components
        [UIParams]
        protected BSMLParserParams parserParams = null;
        [UIComponent("button1")]
        protected Button button1 = null;
        [UIComponent("button2")]
        protected Button button2 = null;
        [UIComponent("button3")]
        protected Button button3 = null;
        [UIComponent("button4")]
        protected Button button4 = null;
        [UIComponent("button5")]
        protected Button button5 = null;
        [UIComponent("button6")]
        protected Button button6 = null;
        [UIComponent("button7")]
        protected Button button7 = null;
        [UIComponent("button8")]
        protected Button button8 = null;
        [UIComponent("button9")]
        protected Button button9 = null;
        [UIComponent("button10")]
        protected Button button10 = null;
#if PPV3
        [UIComponent("buttonPPv3Replay")]
        protected Button buttonPPv3Replay = null;
#endif
        //Info modal click handlers
        [UIAction("b-1-click")] private void B1Clicked() => InfoButtonClicked(0);
        [UIAction("b-2-click")] private void B2Clicked() => InfoButtonClicked(1);
        [UIAction("b-3-click")] private void B3Clicked() => InfoButtonClicked(2);
        [UIAction("b-4-click")] private void B4Clicked() => InfoButtonClicked(3);
        [UIAction("b-5-click")] private void B5Clicked() => InfoButtonClicked(4);
        [UIAction("b-6-click")] private void B6Clicked() => InfoButtonClicked(5);
        [UIAction("b-7-click")] private void B7Clicked() => InfoButtonClicked(6);
        [UIAction("b-8-click")] private void B8Clicked() => InfoButtonClicked(7);
        [UIAction("b-9-click")] private void B9Clicked() => InfoButtonClicked(8);
        [UIAction("b-10-click")] private void B10Clicked() => InfoButtonClicked(9);
        #endregion

        private List<Button> buttons { get; set; }
        public event Action<int> infoButtonClicked;


        [UIAction("#post-parse")]
        public void Parsed() {
            const float buttonScale = .425f;
            buttons = new List<Button>();
            ChangeButtonScale(button1, buttonScale);
            ChangeButtonScale(button2, buttonScale);
            ChangeButtonScale(button3, buttonScale);
            ChangeButtonScale(button4, buttonScale);
            ChangeButtonScale(button5, buttonScale);
            ChangeButtonScale(button6, buttonScale);
            ChangeButtonScale(button7, buttonScale);
            ChangeButtonScale(button8, buttonScale);
            ChangeButtonScale(button9, buttonScale);
            ChangeButtonScale(button10, buttonScale);

#if PPV3
            if (buttonPPv3Replay != null) {
                buttonPPv3Replay.transform.localScale *= buttonScale;
                buttonPPv3Replay.gameObject.SetActive(true);
            }
#endif
        }

        private void ChangeButtonScale(Button button, float scale) {
            button.transform.localScale *= scale;
            buttons.Add(button);
        }

        private void InfoButtonClicked(int index) {

            infoButtonClicked?.Invoke(index);
        }

        public void UpdateInfoButtonState(int enabled) {

            int enabledButtons = 0;
            for (int i = 0; i < enabled; i++) {
                buttons[i].gameObject.SetActive(true);
                enabledButtons = i;
            }
            enabledButtons++;
            for (int x = enabledButtons; x < 10; x++) {
                buttons[x].gameObject.SetActive(false);
            }
        }

        public void HideInfoButtons() {

            if (buttons != null) {
                foreach (var button in buttons) {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }
}