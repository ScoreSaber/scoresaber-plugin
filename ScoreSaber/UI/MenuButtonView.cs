#region

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using ScoreSaber.Core.Services;
using ScoreSaber.UI.Main;
using System;
using Zenject;

#endregion

// Unused
namespace ScoreSaber.UI {
    internal class MenuButtonView : IInitializable, IDisposable {
        private readonly MainFlowCoordinator _mainFlowCoordinator;

        private readonly MenuButton _menuButton;
        private readonly PlayerService _playerService;
        private readonly ScoreSaberFlowCoordinator _scoreSaberFlowCoordinator;

        public MenuButtonView(PlayerService playerService, MainFlowCoordinator mainFlowCoordinator,
            ScoreSaberFlowCoordinator scoreSaberFlowCoordinator) {
            _playerService = playerService;
            _mainFlowCoordinator = mainFlowCoordinator;
            _scoreSaberFlowCoordinator = scoreSaberFlowCoordinator;
            _menuButton = new MenuButton("ScoreSaber", "View the ScoreSaber Global Leaderboards, team members & more!",
                PresentScoreSaberFlow);
        }

        public void Dispose() {
            _playerService.LoginStatusChanged -= playerService_LoginStatusChanged;
            bool isSingletonAvailable = PersistentSingleton<MenuButtons>.IsSingletonAvailable;
            switch (isSingletonAvailable) {
                case true:
                    PersistentSingleton<MenuButtons>.instance.UnregisterButton(_menuButton);
                    break;
            }
        }

        public void Initialize() {
            _playerService.LoginStatusChanged += playerService_LoginStatusChanged;
            PersistentSingleton<MenuButtons>.instance.RegisterButton(_menuButton);
            _menuButton.Interactable = false;
        }

        private void playerService_LoginStatusChanged(PlayerService.LoginStatus loginStatus, string loginStatusInfo) {
            switch (loginStatus) {
                case PlayerService.LoginStatus.Success:
                    _menuButton.Interactable = true;
                    break;
            }
        }

        private void PresentScoreSaberFlow() {
            _mainFlowCoordinator.PresentFlowCoordinator(_scoreSaberFlowCoordinator);
        }
    }
}