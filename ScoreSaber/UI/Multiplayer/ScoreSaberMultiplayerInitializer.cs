#region

using ScoreSaber.Core.Services;
using ScoreSaber.UI.Leaderboard;
using System;
using Zenject;

#endregion

namespace ScoreSaber.UI.Multiplayer {
    internal class ScoreSaberMultiplayerInitializer : IInitializable, IDisposable {
        private readonly GameServerLobbyFlowCoordinator _gameServerLobbyFlowCoordinator;

        private readonly PlayerService _playerService;
        private readonly ScoreSaberLeaderboardViewController _scoreSaberLeaderboardViewController;

        public ScoreSaberMultiplayerInitializer(PlayerService playerService,
            GameServerLobbyFlowCoordinator gameServerLobbyFlowCoordinator,
            ScoreSaberLeaderboardViewController scoreSaberLeaderboardViewController) {
            _playerService = playerService;
            _gameServerLobbyFlowCoordinator = gameServerLobbyFlowCoordinator;
            _scoreSaberLeaderboardViewController = scoreSaberLeaderboardViewController;
        }

        public void Dispose() {
            _gameServerLobbyFlowCoordinator.didSetupEvent -= GameServerLobbyFlowCoordinator_didSetupEvent;
            _gameServerLobbyFlowCoordinator.didFinishEvent -= GameServerLobbyFlowCoordinator_didFinishEvent;
        }

        public void Initialize() {
            _gameServerLobbyFlowCoordinator.didSetupEvent += GameServerLobbyFlowCoordinator_didSetupEvent;
            _gameServerLobbyFlowCoordinator.didFinishEvent += GameServerLobbyFlowCoordinator_didFinishEvent;
        }

        private void GameServerLobbyFlowCoordinator_didSetupEvent() {
            _playerService.GetLocalPlayerInfo();
            _scoreSaberLeaderboardViewController.AllowReplayWatching(false);
        }

        private void GameServerLobbyFlowCoordinator_didFinishEvent() {
            _scoreSaberLeaderboardViewController.AllowReplayWatching(true);
        }
    }
}