using HMUI;
using IPA.Utilities;
using IPA.Utilities.Async;
using System;
using System.Threading.Tasks;
using Zenject;

namespace ScoreSaber.UI.Multiplayer {
    internal class ScoreSaberMultiplayerLevelSelectionLeaderboardFlowManager : IInitializable, IDisposable {

        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly ServerPlayerListViewController _serverPlayerListViewController;
        private readonly PlatformLeaderboardViewController _platformLeaderboardViewController;
        private readonly LevelSelectionNavigationController _levelSelectionNavigationController;

        private bool _currentlyInMulti;
        private bool _performingFirstActivation;

        public ScoreSaberMultiplayerLevelSelectionLeaderboardFlowManager(MainFlowCoordinator mainFlowCoordinator, ServerPlayerListViewController serverPlayerListViewController, PlatformLeaderboardViewController platformLeaderboardViewController, LevelSelectionNavigationController levelSelectionNavigationController) {
            
            _mainFlowCoordinator = mainFlowCoordinator;
            _serverPlayerListViewController = serverPlayerListViewController;
            _platformLeaderboardViewController = platformLeaderboardViewController;
            _levelSelectionNavigationController = levelSelectionNavigationController;
        }

        public void Initialize() {

            _levelSelectionNavigationController.didActivateEvent += LevelSelectionNavigationController_didActivateEvent;
            _levelSelectionNavigationController.didDeactivateEvent += LevelSelectionNavigationController_didDeactivateEvent;
        }

        private void LevelSelectionNavigationController_didChangeLevelDetailContentEvent(LevelSelectionNavigationController controller, StandardLevelDetailViewController.ContentType contentType) {
            
            ShowLeaderboard();
        }

        private void LevelSelectionNavigationController_didChangeDifficultyBeatmapEvent(LevelSelectionNavigationController _) {

            ShowLeaderboard();
        }

        private void HideLeaderboard() {

            if (_platformLeaderboardViewController.isInViewControllerHierarchy) {

                var currentFlowCoordinator = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
                if (!(currentFlowCoordinator is MultiplayerLevelSelectionFlowCoordinator))
                    return;

                ReflectionUtil.InvokeMethod<object, FlowCoordinator>(currentFlowCoordinator, "SetRightScreenViewController", null, ViewController.AnimationType.Out);
            }
        }

        private void ShowLeaderboard() {

            if (!InMulti())
                return;

            if(!_levelSelectionNavigationController.beatmapKey.IsValid()) {
                HideLeaderboard();
                return;
            }

            _platformLeaderboardViewController.SetData(_levelSelectionNavigationController.beatmapKey);
            var currentFlowCoordinator = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            ReflectionUtil.InvokeMethod<object, FlowCoordinator>(currentFlowCoordinator, "SetRightScreenViewController", _platformLeaderboardViewController, ViewController.AnimationType.In);
            _serverPlayerListViewController.gameObject.SetActive(false); // This is a bandaid fix, first time startup it gets stuck while animating kinda like the issue we had before (TODO: Fix in 2024)
            
            // I am... very tired... it gets stuck in a loading loop on initialization sometimes.
            if (_performingFirstActivation) {
                _performingFirstActivation = false;
                _ = Task.Run(async () => {
                    await Task.Delay(250);
                    _ = UnityMainThreadTaskScheduler.Factory.StartNew(() => {
                        _platformLeaderboardViewController.Refresh(true, true);
                    });
                });
            }
        }

        private void LevelSelectionNavigationController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {

            if (!InMulti())
                return;

            if (firstActivation)
                _performingFirstActivation = true;

            _currentlyInMulti = true;
            _levelSelectionNavigationController.didChangeDifficultyBeatmapEvent += LevelSelectionNavigationController_didChangeDifficultyBeatmapEvent;
            _levelSelectionNavigationController.didChangeLevelDetailContentEvent += LevelSelectionNavigationController_didChangeLevelDetailContentEvent;
            ShowLeaderboard();
        }

        private void LevelSelectionNavigationController_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling) {

            if (!InMulti())
                return;

            _currentlyInMulti = false;
            _levelSelectionNavigationController.didChangeDifficultyBeatmapEvent -= LevelSelectionNavigationController_didChangeDifficultyBeatmapEvent;
            _levelSelectionNavigationController.didChangeLevelDetailContentEvent -= LevelSelectionNavigationController_didChangeLevelDetailContentEvent;
        }

        public void Dispose() {

            _levelSelectionNavigationController.didActivateEvent -= LevelSelectionNavigationController_didActivateEvent;
        }

        private bool InMulti() {

            if (_currentlyInMulti)
                return true;

            var currentFlowCoordinator = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            if (!(currentFlowCoordinator is MultiplayerLevelSelectionFlowCoordinator))
                return false;
            return true;
        }
    }
}
