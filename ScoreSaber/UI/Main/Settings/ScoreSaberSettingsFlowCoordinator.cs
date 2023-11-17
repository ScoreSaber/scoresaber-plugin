using BeatSaberMarkupLanguage;
using HMUI;
using ScoreSaber.Core.Data;
using ScoreSaber.UI.Leaderboard;
using ScoreSaber.UI.Main.Settings.ViewControllers;
using ScoreSaber.UI.Main.ViewControllers;
using System.Linq;
using UnityEngine;
using Zenject;

namespace ScoreSaber.UI.Main {
    internal class ScoreSaberSettingsFlowCoordinator : FlowCoordinator, IInitializable {

        private FlowCoordinator _lastFlowCoordinator;
        private MainSettingsViewController _mainSettingsViewController;
        private ScoreSaberLeaderboardViewController _scoresaberLeaderboardViewController;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {

            if (firstActivation) {
                SetTitle("ScoreSaber Settings");
                showBackButton = true;
                ProvideInitialViewControllers(_mainSettingsViewController);
            }
        }

        [Inject]
        internal void Construct(MainSettingsViewController mainSettingsViewController, ScoreSaberLeaderboardViewController scoreSaberLeaderboardViewController) {
            
            _mainSettingsViewController = mainSettingsViewController;
            _scoresaberLeaderboardViewController = scoreSaberLeaderboardViewController;
            Plugin.Log.Debug("ScoreSaberSettingsFlowCoordinator Setup");
        }

        protected override void BackButtonWasPressed(ViewController topViewController) {

            SetLeftScreenViewController(null, ViewController.AnimationType.None);
            SetRightScreenViewController(null, ViewController.AnimationType.None);
            Core.Data.Settings.SaveSettings(Plugin.Settings);
            _lastFlowCoordinator.DismissFlowCoordinator(this);
            _scoresaberLeaderboardViewController.RefreshLeaderboard();
        }

        internal static void ShowSettingsFlowCoordinator() {

            ScoreSaberSettingsFlowCoordinator flowCoordinator = Resources.FindObjectsOfTypeAll<ScoreSaberSettingsFlowCoordinator>().FirstOrDefault();
            if (flowCoordinator != null) {
                var activeFlow = DeepestChildFlowCoordinator(BeatSaberUI.MainFlowCoordinator);
                activeFlow.PresentFlowCoordinator(flowCoordinator);
                flowCoordinator._lastFlowCoordinator = activeFlow;
            } else {
                Plugin.Log.Error("Unable to find flow coordinator! Cannot show ScoreSaber Flow Coordinator.");
            }
        }

        internal static FlowCoordinator DeepestChildFlowCoordinator(FlowCoordinator root) {

            var flow = root.childFlowCoordinator;
            if (flow == null) return root;
            if (flow.childFlowCoordinator == null || flow.childFlowCoordinator == flow) {
                return flow;
            }
            return DeepestChildFlowCoordinator(flow);
        }

        public void Initialize() {

        }
    }
}