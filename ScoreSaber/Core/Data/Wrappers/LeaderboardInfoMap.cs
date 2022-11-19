using ScoreSaber.Core.Data.Models;

namespace ScoreSaber.Core.Data.Wrappers {
    internal class LeaderboardInfoMap {
        internal LeaderboardInfo leaderboardInfo { get; set; }
        internal IDifficultyBeatmap difficultyBeatmap { get; set; }
        internal string songHash { get; set; }

        internal LeaderboardInfoMap(LeaderboardInfo leaderboardInfo, IDifficultyBeatmap difficultyBeatmap) {
            this.difficultyBeatmap = difficultyBeatmap;
            this.leaderboardInfo = leaderboardInfo;
            this.songHash = difficultyBeatmap.level.levelID.Split('_')[2];
        }
    }
}