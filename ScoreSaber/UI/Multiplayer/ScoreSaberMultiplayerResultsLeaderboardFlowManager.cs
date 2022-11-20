#region

using HMUI;
using IPA.Utilities;
using SiraUtil.Services;
using System;
using Zenject;

#endregion

namespace ScoreSaber.UI.Multiplayer {
    internal class ScoreSaberMultiplayerResultsLeaderboardFlowManager : IInitializable, IDisposable {
        private readonly ILevelFinisher _levelFinisher;
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly MultiplayerResultsViewController _multiplayerResultsViewController;
        private readonly PlatformLeaderboardViewController _platformLeaderboardViewController;

        private IDifficultyBeatmap _lastCompletedBeatmap;

        public ScoreSaberMultiplayerResultsLeaderboardFlowManager(ILevelFinisher levelFinisher,
            MainFlowCoordinator mainFlowCoordinator, MultiplayerResultsViewController multiplayerResultsViewController,
            PlatformLeaderboardViewController platformLeaderboardViewController) {

            _levelFinisher = levelFinisher;
            _mainFlowCoordinator = mainFlowCoordinator;
            _multiplayerResultsViewController = multiplayerResultsViewController;
            _platformLeaderboardViewController = platformLeaderboardViewController;
        }

        public void Dispose() {

            _multiplayerResultsViewController.didDeactivateEvent -= MultiplayerResultsViewController_didDeactivateEvent;
            _multiplayerResultsViewController.didActivateEvent -= MultiplayerResultsViewController_didActivateEvent;
            _levelFinisher.MultiplayerLevelDidFinish -= LevelFinisher_MultiplayerLevelDidFinish;
        }

        public void Initialize() {

            _levelFinisher.MultiplayerLevelDidFinish += LevelFinisher_MultiplayerLevelDidFinish;
            _multiplayerResultsViewController.didActivateEvent += MultiplayerResultsViewController_didActivateEvent;
            _multiplayerResultsViewController.didDeactivateEvent += MultiplayerResultsViewController_didDeactivateEvent;
        }

        private void MultiplayerResultsViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy,
            bool screenSystemEnabling) {

            var currentFlowCoordinator = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            if (currentFlowCoordinator is GameServerLobbyFlowCoordinator) {
                _platformLeaderboardViewController.SetData(_lastCompletedBeatmap);
                currentFlowCoordinator.InvokeMethod<object, FlowCoordinator>("SetRightScreenViewController",
                    _platformLeaderboardViewController, ViewController.AnimationType.In);
            }
        }

        private void MultiplayerResultsViewController_didDeactivateEvent(bool removedFromHierarchy,
            bool screenSystemDisabling) {

            if (removedFromHierarchy || screenSystemDisabling) {
                _lastCompletedBeatmap = null;
            }
        }

        private void LevelFinisher_MultiplayerLevelDidFinish(
            MultiplayerLevelScenesTransitionSetupDataSO transitionSetupData, MultiplayerResultsData _) {

            _lastCompletedBeatmap = transitionSetupData.difficultyBeatmap;
        }
    }
}