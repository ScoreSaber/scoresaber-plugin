using HMUI;
using LeaderboardCore.Managers;
using LeaderboardCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSaber.UI.Leaderboard {
    internal class ScoreSaberLeaderboard : CustomLeaderboard, IDisposable {

        private readonly CustomLeaderboardManager _manager;
        private readonly ScoreSaberLeaderboardViewController _leaderboardView;

        public override bool ShowForLevel(BeatmapKey? selectedLevel) {
            if (selectedLevel.HasValue) {
                if (!selectedLevel.Value.levelId.Contains("custom_level_")) {
                    return false;
                }
            }
            return true;
        }
        protected override string leaderboardId => "ScoreSaber";

        internal ScoreSaberLeaderboard(CustomLeaderboardManager customLeaderboardManager, PanelView panelView, ScoreSaberLeaderboardViewController leaderboardView) {
            panelViewController = panelView;
            _leaderboardView = leaderboardView;
            _manager = customLeaderboardManager;
            _manager.Register(this);
        }

        protected override ViewController panelViewController { get; }
        protected override ViewController leaderboardViewController => _leaderboardView;

        public void Dispose() {
            _manager.Unregister(this);
        }
    }
}
