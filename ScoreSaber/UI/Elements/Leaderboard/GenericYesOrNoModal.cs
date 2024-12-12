using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
namespace ScoreSaber.UI.Elements.Leaderboard {
    internal class GenericYesOrNoModal {
        #region BSML Components
        [UIComponent("question-text")]
        protected CurvedTextMeshPro _questionText = null;
        [UIComponent("yes-button")]
        protected UnityEngine.UI.Button _yesButton = null;
        [UIComponent("no-button")]
        protected UnityEngine.UI.Button _noButton = null;
        [UIComponent("more-info-yes-no-modal")]
        protected CurvedTextMeshPro _moreInfoText= null;
        [UIParams]
        protected BSMLParserParams _parserParams = null;
        #endregion

        private Action _yesAction;
        private Action _noAction;

        [UIAction("yes-clicked")]
        public void YesClicked() {
            _yesAction?.Invoke();
            _parserParams.EmitEvent("close-modals");
        }

        [UIAction("no-clicked")]
        public void NoClicked() {
            _noAction?.Invoke();
            _parserParams.EmitEvent("close-modals");
        }

        public void Show(YesOrNoModalInfo yesOrNoModalInfo) {
            _questionText.text = yesOrNoModalInfo.Question;
            _yesAction = yesOrNoModalInfo.YesAction;
            _noAction = yesOrNoModalInfo.NoAction;
            _moreInfoText.text = yesOrNoModalInfo.MoreInfo;
            _parserParams.EmitEvent("present-yes-no-modal");
        }

        public class YesOrNoModalInfo {
            public string Question { get; set; }
            public Action YesAction { get; set; }
            public Action NoAction { get; set; }
            public string MoreInfo { get; set; }

            public YesOrNoModalInfo(string question, Action yesAction, Action noAction, string moreInfo) {
                Question = question;
                YesAction = yesAction;
                NoAction = noAction;
                MoreInfo = moreInfo;
            }
        }
    }
}
