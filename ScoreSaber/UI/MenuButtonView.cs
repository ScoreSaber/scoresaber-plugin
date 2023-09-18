using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using ScoreSaber.Core.Services;
using ScoreSaber.UI.Main;
using System;
using Zenject;

// Unused
namespace ScoreSaber.UI.ViewControllers {
    internal class MenuButtonView : IInitializable, IDisposable {

		private readonly MenuButton _menuButton;
		private readonly PlayerService _playerService;
		private readonly MainFlowCoordinator _mainFlowCoordinator;
		private readonly ScoreSaberFlowCoordinator _scoreSaberFlowCoordinator;

		public MenuButtonView(PlayerService playerService, MainFlowCoordinator mainFlowCoordinator, ScoreSaberFlowCoordinator scoreSaberFlowCoordinator) {
			_playerService = playerService;
			_mainFlowCoordinator = mainFlowCoordinator;
			_scoreSaberFlowCoordinator = scoreSaberFlowCoordinator;
			_menuButton = new MenuButton("ScoreSaber", "View the ScoreSaber Global Leaderboards, team members & more!", new Action(this.PresentScoreSaberFlow), true);
		}

		public void Initialize() {
			_playerService.LoginStatusChanged += playerService_LoginStatusChanged;
            MenuButtons.instance.RegisterButton(_menuButton);
			_menuButton.Interactable = false;
		}

        private void playerService_LoginStatusChanged(PlayerService.LoginStatus loginStatus, string loginStatusInfo) {
            if (loginStatus == PlayerService.LoginStatus.Success) {
				_menuButton.Interactable = true;
			}
        }
		
		private void PresentScoreSaberFlow() {
			_mainFlowCoordinator.PresentFlowCoordinator(_scoreSaberFlowCoordinator, null, ViewController.AnimationDirection.Horizontal, false, false);
		}

		public void Dispose() {
			_playerService.LoginStatusChanged -= playerService_LoginStatusChanged;
            MenuButtons.instance.UnregisterButton(this._menuButton);
		}
	}
}
