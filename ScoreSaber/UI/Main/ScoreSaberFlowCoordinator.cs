#region

using BeatSaberMarkupLanguage;
using HMUI;
using ScoreSaber.UI.Main.ViewControllers;
using System.Linq;
using UnityEngine;
using Zenject;

#endregion

namespace ScoreSaber.UI.Main {
    internal class ScoreSaberFlowCoordinator : FlowCoordinator, IInitializable {
        private FAQViewController _faqViewController;
        private GlobalViewController _globalViewController;

        private FlowCoordinator _lastFlowCoordinator;
        private TeamViewController _teamViewController;

        public void Initialize() {
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            switch (firstActivation) {
                case true:
                    SetTitle("ScoreSaber");
                    showBackButton = true;
                    ProvideInitialViewControllers(_globalViewController, _teamViewController, _faqViewController);
                    break;
            }
        }

        [Inject]
        internal void Construct(FAQViewController faqViewController, TeamViewController teamViewController,
            GlobalViewController globalViewController) {
            _faqViewController = faqViewController;
            _teamViewController = teamViewController;
            _globalViewController = globalViewController;
            Plugin.Log.Debug("ScoreSaberFlowCoordinator Setup");
        }

        protected override void BackButtonWasPressed(ViewController topViewController) {
            SetLeftScreenViewController(null, ViewController.AnimationType.None);
            SetRightScreenViewController(null, ViewController.AnimationType.None);
            _lastFlowCoordinator.DismissFlowCoordinator(this);
        }

        internal static void ShowMainFlowCoordinator() {
            ScoreSaberFlowCoordinator flowCoordinator =
                Resources.FindObjectsOfTypeAll<ScoreSaberFlowCoordinator>().FirstOrDefault();
            if (flowCoordinator != null) {
                FlowCoordinator activeFlow = DeepestChildFlowCoordinator(BeatSaberUI.MainFlowCoordinator);
                activeFlow.PresentFlowCoordinator(flowCoordinator);
                flowCoordinator._lastFlowCoordinator = activeFlow;
            } else {
                Plugin.Log.Error("Unable to find flow coordinator! Cannot show ScoreSaber Flow Coordinator.");
            }
        }

        internal static FlowCoordinator DeepestChildFlowCoordinator(FlowCoordinator root) {
            FlowCoordinator flow = root.childFlowCoordinator;
            if (flow == null) {
                return root;
            }

            if (flow.childFlowCoordinator == null || flow.childFlowCoordinator == flow) {
                return flow;
            }

            return DeepestChildFlowCoordinator(flow);
        }
    }
}