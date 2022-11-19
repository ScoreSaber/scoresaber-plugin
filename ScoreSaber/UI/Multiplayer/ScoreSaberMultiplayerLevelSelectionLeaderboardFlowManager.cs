#region

using HMUI;
using IPA.Utilities;
using IPA.Utilities.Async;
using System;
using System.Threading.Tasks;
using Zenject;

#endregion

namespace ScoreSaber.UI.Multiplayer {
    internal class ScoreSaberMultiplayerLevelSelectionLeaderboardFlowManager : IInitializable, IDisposable {
        private readonly LevelSelectionNavigationController _levelSelectionNavigationController;

        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly PlatformLeaderboardViewController _platformLeaderboardViewController;
        private readonly ServerPlayerListViewController _serverPlayerListViewController;

        private bool _currentlyInMulti;
        private bool _performingFirstActivation;

        public ScoreSaberMultiplayerLevelSelectionLeaderboardFlowManager(MainFlowCoordinator mainFlowCoordinator,
            ServerPlayerListViewController serverPlayerListViewController,
            PlatformLeaderboardViewController platformLeaderboardViewController,
            LevelSelectionNavigationController levelSelectionNavigationController) {
            _mainFlowCoordinator = mainFlowCoordinator;
            _serverPlayerListViewController = serverPlayerListViewController;
            _platformLeaderboardViewController = platformLeaderboardViewController;
            _levelSelectionNavigationController = levelSelectionNavigationController;
        }

        public void Dispose() {
            _levelSelectionNavigationController.didActivateEvent -= LevelSelectionNavigationController_didActivateEvent;
        }

        public void Initialize() {
            _levelSelectionNavigationController.didActivateEvent += LevelSelectionNavigationController_didActivateEvent;
            _levelSelectionNavigationController.didDeactivateEvent +=
                LevelSelectionNavigationController_didDeactivateEvent;
        }

        private void LevelSelectionNavigationController_didChangeLevelDetailContentEvent(
            LevelSelectionNavigationController controller, StandardLevelDetailViewController.ContentType contentType) {
            switch (controller.selectedDifficultyBeatmap) {
                case null:
                    HideLeaderboard();
                    return;
                default:
                    ShowLeaderboard();
                    break;
            }
        }

        private void LevelSelectionNavigationController_didChangeDifficultyBeatmapEvent(
            LevelSelectionNavigationController _, IDifficultyBeatmap beatmap) {
            ShowLeaderboard();
        }

        private void HideLeaderboard() {
            switch (_platformLeaderboardViewController.isInViewControllerHierarchy) {
                case true: {
                    FlowCoordinator currentFlowCoordinator = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
                    switch (currentFlowCoordinator is MultiplayerLevelSelectionFlowCoordinator) {
                        case false:
                            return;
                    }

                    currentFlowCoordinator.InvokeMethod<object, FlowCoordinator>("SetRightScreenViewController", null,
                        ViewController.AnimationType.Out);
                    break;
                }
            }
        }

        private void ShowLeaderboard() {
            if (!InMulti()) {
                return;
            }

            IDifficultyBeatmap selected = _levelSelectionNavigationController.selectedDifficultyBeatmap;
            switch (selected) {
                case null:
                    HideLeaderboard();
                    return;
            }

            _platformLeaderboardViewController.SetData(selected);
            FlowCoordinator currentFlowCoordinator = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            currentFlowCoordinator.InvokeMethod<object, FlowCoordinator>("SetRightScreenViewController",
                _platformLeaderboardViewController, ViewController.AnimationType.In);
            _serverPlayerListViewController.gameObject
                .SetActive(false); // This is a bandaid fix, first time startup it gets stuck while animating kinda like the issue we had before (TODO: Fix in 2024)

            switch (_performingFirstActivation) {
                // I am... very tired... it gets stuck in a loading loop on initialization sometimes.
                case true:
                    _performingFirstActivation = false;
                    _ = Task.Run(async () => {
                        await Task.Delay(250);
                        _ = UnityMainThreadTaskScheduler.Factory.StartNew(() => {
                            _platformLeaderboardViewController.Refresh(true, true);
                        });
                    });
                    break;
            }
        }

        private void LevelSelectionNavigationController_didActivateEvent(bool firstActivation, bool addedToHierarchy,
            bool screenSystemEnabling) {
            if (!InMulti()) {
                return;
            }

            switch (firstActivation) {
                case true:
                    _performingFirstActivation = true;
                    break;
            }

            _currentlyInMulti = true;
            _levelSelectionNavigationController.didChangeDifficultyBeatmapEvent +=
                LevelSelectionNavigationController_didChangeDifficultyBeatmapEvent;
            _levelSelectionNavigationController.didChangeLevelDetailContentEvent +=
                LevelSelectionNavigationController_didChangeLevelDetailContentEvent;
            ShowLeaderboard();
        }

        private void LevelSelectionNavigationController_didDeactivateEvent(bool removedFromHierarchy,
            bool screenSystemDisabling) {
            if (!InMulti()) {
                return;
            }

            _currentlyInMulti = false;
            _levelSelectionNavigationController.didChangeDifficultyBeatmapEvent -=
                LevelSelectionNavigationController_didChangeDifficultyBeatmapEvent;
            _levelSelectionNavigationController.didChangeLevelDetailContentEvent -=
                LevelSelectionNavigationController_didChangeLevelDetailContentEvent;
        }

        private bool InMulti() {
            switch (_currentlyInMulti) {
                case true:
                    return true;
            }

            FlowCoordinator currentFlowCoordinator = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            switch (currentFlowCoordinator is MultiplayerLevelSelectionFlowCoordinator) {
                case false:
                    return false;
                default:
                    return true;
            }
        }
    }
}