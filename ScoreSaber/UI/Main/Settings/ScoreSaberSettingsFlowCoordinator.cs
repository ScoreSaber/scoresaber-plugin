using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using ScoreSaber.Core.Data;
using ScoreSaber.Extensions;
using ScoreSaber.UI.Leaderboard;
using ScoreSaber.UI.Main.Settings.ViewControllers;
using ScoreSaber.UI.Main.ViewControllers;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Zenject;
using static HMUI.IconSegmentedControl;

namespace ScoreSaber.UI.Main {
    internal class ScoreSaberSettingsFlowCoordinator : FlowCoordinator, IInitializable {

        private FlowCoordinator _lastFlowCoordinator;
        private MainSettingsViewController _mainSettingsHandlerViewController;
        private ScoreSaberLeaderboardViewController _scoresaberLeaderboardViewController;
        private PanelView _panelView;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {

            if (firstActivation) {
                SetTitle("ScoreSaber Settings");
                showBackButton = true;
                ProvideInitialViewControllers(_mainSettingsHandlerViewController);
            }
        }

        [Inject]
        internal void Construct(MainSettingsViewController mainSettingsViewController, ScoreSaberLeaderboardViewController scoreSaberLeaderboardViewController, PanelView panelView) {
            
            _mainSettingsHandlerViewController = mainSettingsViewController;
            _scoresaberLeaderboardViewController = scoreSaberLeaderboardViewController;
            _panelView = panelView;
            Plugin.Log.Debug("ScoreSaberSettingsFlowCoordinator Setup");
        }

        protected override void BackButtonWasPressed(ViewController topViewController) {

            SetLeftScreenViewController(null, ViewController.AnimationType.None);
            SetRightScreenViewController(null, ViewController.AnimationType.None);
            Core.Data.Settings.SaveSettings(Plugin.Settings);
            _lastFlowCoordinator.DismissFlowCoordinator(this);
            _scoresaberLeaderboardViewController.RefreshLeaderboard();
            if (Plugin.Settings.showLocalPlayerRank) {
                _panelView.SetGlobalRanking($"#{string.Format("{0:n0}", _panelView._currentPlayerInfo.rank)}<size=75%> (<color=#6772E5>{string.Format("{0:n0}", _panelView._currentPlayerInfo.pp)}pp</color>)");
            } else {
                _panelView.SetGlobalRanking("Hidden");
            }
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