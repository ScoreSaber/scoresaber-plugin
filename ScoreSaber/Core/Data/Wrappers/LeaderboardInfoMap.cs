using ScoreSaber.Core.Data.Models;

namespace ScoreSaber.Core.Data.Wrappers {
    internal class LeaderboardInfoMap {
        internal LeaderboardInfo leaderboardInfo { get; set; }
        internal IDifficultyBeatmap DifficultyBeatmap { get; set; }
        internal string SongHash { get; set; }

        internal LeaderboardInfoMap(LeaderboardInfo leaderboardInfo, IDifficultyBeatmap difficultyBeatmap) {

            this.DifficultyBeatmap = difficultyBeatmap;
            this.leaderboardInfo = leaderboardInfo;
            SongHash = difficultyBeatmap.level.levelID.Split('_')[2];
        }
    }
}