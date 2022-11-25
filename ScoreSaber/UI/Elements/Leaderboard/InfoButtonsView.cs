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
        [UIAction("b-1-click")] private void B1Clicked() => InfoButtonClick(0);
        [UIAction("b-2-click")] private void B2Clicked() => InfoButtonClick(1);
        [UIAction("b-3-click")] private void B3Clicked() => InfoButtonClick(2);
        [UIAction("b-4-click")] private void B4Clicked() => InfoButtonClick(3);
        [UIAction("b-5-click")] private void B5Clicked() => InfoButtonClick(4);
        [UIAction("b-6-click")] private void B6Clicked() => InfoButtonClick(5);
        [UIAction("b-7-click")] private void B7Clicked() => InfoButtonClick(6);
        [UIAction("b-8-click")] private void B8Clicked() => InfoButtonClick(7);
        [UIAction("b-9-click")] private void B9Clicked() => InfoButtonClick(8);
        [UIAction("b-10-click")] private void B10Clicked() => InfoButtonClick(9);
        #endregion

        private List<Button> Buttons { get; set; }
        public event Action<int> InfoButtonClicked;


        [UIAction("#post-parse")]
        public void Parsed() {
            const float buttonScale = .425f;
            Buttons = new List<Button>();
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
            Buttons.Add(button);
        }

        private void InfoButtonClick(int index) {

            InfoButtonClicked?.Invoke(index);
        }

        public void UpdateInfoButtonState(int enabled) {

            int enabledButtons = 0;
            for (int i = 0; i < enabled; i++) {
                Buttons[i].gameObject.SetActive(true);
                enabledButtons = i;
            }
            enabledButtons++;
            for (int x = enabledButtons; x < 10; x++) {
                Buttons[x].gameObject.SetActive(false);
            }
        }

        public void HideInfoButtons() {

            if (Buttons != null) {
                foreach (var button in Buttons) {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }
}