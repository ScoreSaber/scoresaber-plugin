using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using ScoreSaber.Core.Services;
using ScoreSaber.UI.Main;
using System;
using Zenject;

// Used :0
namespace ScoreSaber.UI.ViewControllers {
    internal class MenuButtonView : IInitializable, IDisposable {

		private readonly MenuButton _scoresaberMenuButton;
		private readonly PlayerService _playerService;
		private readonly MainFlowCoordinator _mainFlowCoordinator;
		private readonly ScoreSaberFlowCoordinator _scoreSaberFlowCoordinator;
        private readonly MenuButtons _menuButtons;

        public Action<bool> MenuButtonVisibilityChanged;

        public MenuButtonView(PlayerService playerService, MainFlowCoordinator mainFlowCoordinator, ScoreSaberFlowCoordinator scoreSaberFlowCoordinator, MenuButtons menuButtons) {
			_playerService = playerService;
			_mainFlowCoordinator = mainFlowCoordinator;
			_scoreSaberFlowCoordinator = scoreSaberFlowCoordinator;
            _menuButtons = menuButtons;
            _scoresaberMenuButton = new MenuButton("ScoreSaber", "View the ScoreSaber Global Leaderboards, team members & more!", new Action(this.PresentScoreSaberFlow), true);
		}

        public void Initialize() {
            _playerService.LoginStatusChanged += playerService_LoginStatusChanged;
            MenuButtonVisibilityChanged += SetVisibility;
            MenuButtonVisibilityChanged?.Invoke(Plugin.Settings.showMainMenuButton);
        }

        public void SetVisibility(bool visible) {
            if(visible) {
                _menuButtons.RegisterButton(_scoresaberMenuButton);
            } else {
                _menuButtons.UnregisterButton(_scoresaberMenuButton);
            }
        }

        private void playerService_LoginStatusChanged(PlayerService.LoginStatus loginStatus, string loginStatusInfo) {
            _scoresaberMenuButton.Interactable = false;
            if (loginStatus == PlayerService.LoginStatus.Success) {
				_scoresaberMenuButton.Interactable = true;
            }
        }
		
		private void PresentScoreSaberFlow() {
            _scoreSaberFlowCoordinator.ShowMainFlowCoordinator(true);
		}

		public void Dispose() {
			_playerService.LoginStatusChanged -= playerService_LoginStatusChanged;
            MenuButtonVisibilityChanged -= SetVisibility;
            _menuButtons.UnregisterButton(this._scoresaberMenuButton);
		}
	}
}
