using ScoreSaber.Core.Services;
using System;
using Zenject;

namespace ScoreSaber.UI.Multiplayer {
    internal class ScoreSaberMultiplayerInitializer : IInitializable, IDisposable {

        private readonly PlayerService _playerService;
        private readonly GameServerLobbyFlowCoordinator _gameServerLobbyFlowCoordinator;

        public ScoreSaberMultiplayerInitializer(PlayerService playerService, GameServerLobbyFlowCoordinator gameServerLobbyFlowCoordinator) {
            _playerService = playerService;
            _gameServerLobbyFlowCoordinator = gameServerLobbyFlowCoordinator;
        }

        public void Initialize() {

            _gameServerLobbyFlowCoordinator.didSetupEvent += GameServerLobbyFlowCoordinator_didSetupEvent;
        }

        private void GameServerLobbyFlowCoordinator_didSetupEvent() {

            _playerService.GetLocalPlayerInfo();
        }

        public void Dispose() {

            _gameServerLobbyFlowCoordinator.didSetupEvent -= GameServerLobbyFlowCoordinator_didSetupEvent;
        }
    }
}