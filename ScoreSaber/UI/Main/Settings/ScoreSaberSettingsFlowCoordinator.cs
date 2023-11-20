using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using ScoreSaber.Core.Data;
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

            var iconSegmented = _scoresaberLeaderboardViewController._platformLeaderboardViewController.GetComponentInChildren<IconSegmentedControl>();
            DataItem[] dataItems = iconSegmented.GetField<DataItem[], IconSegmentedControl>("_dataItems");
            Texture2D countryTexture = new Texture2D(64, 64);
            countryTexture.LoadImage(Utilities.GetResource(Assembly.GetExecutingAssembly(), "ScoreSaber.Resources.country.png"));
            countryTexture.Apply();

            Sprite _countryIcon = Sprite.Create(countryTexture, new Rect(0, 0, countryTexture.width, countryTexture.height), Vector2.zero);
            iconSegmented.SetData(new DataItem[] {
                    new DataItem(dataItems[0].icon, "Global"),
                    new DataItem(dataItems[1].icon, "Around You"),
                    new DataItem(dataItems[2].icon, "Friends"),
                    Plugin.Settings.locationFilterMode.ToLower() == "country" ? new DataItem(_countryIcon, "Country") : Plugin.Settings.locationFilterMode.ToLower() == "region" ? new DataItem(_countryIcon, "Region") : new DataItem(_countryIcon, "Country")
                });
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