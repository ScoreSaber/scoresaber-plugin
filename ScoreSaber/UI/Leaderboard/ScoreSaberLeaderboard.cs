using HMUI;
using LeaderboardCore.Managers;
using LeaderboardCore.Models;
using ScoreSaber.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSaber.UI.Leaderboard {
    internal class ScoreSaberLeaderboard : CustomLeaderboard, IDisposable {

        private readonly CustomLeaderboardManager _manager;
        private readonly ScoreSaberLeaderboardViewController _leaderboardView;
        private readonly PlayerService _playerService;

        public override bool ShowForLevel(BeatmapKey? selectedLevel) {
            return true;
        }
        protected override string leaderboardId => "ScoreSaber";

        internal ScoreSaberLeaderboard(CustomLeaderboardManager customLeaderboardManager, PanelView panelView, ScoreSaberLeaderboardViewController leaderboardView, PlayerService playerService) {
            panelViewController = panelView;
            _leaderboardView = leaderboardView;
            _manager = customLeaderboardManager;
            _manager.Register(this);
            _playerService = playerService;
            _playerService.SignIn();
        }

        protected override ViewController panelViewController { get; }
        protected override ViewController leaderboardViewController => _leaderboardView;

        public void Dispose() {
            _manager.Unregister(this);
        }
    }
}
