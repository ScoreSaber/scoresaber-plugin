#region

using ScoreSaber.Core.Data.Models;

#endregion

namespace ScoreSaber.Core.Data.Wrappers {
    internal class LeaderboardInfoMap {
        internal LeaderboardInfoMap(LeaderboardInfo leaderboardInfo, IDifficultyBeatmap difficultyBeatmap) {
            this.difficultyBeatmap = difficultyBeatmap;
            this.leaderboardInfo = leaderboardInfo;
            songHash = difficultyBeatmap.level.levelID.Split('_')[2];
        }

        internal LeaderboardInfo leaderboardInfo { get; set; }
        internal IDifficultyBeatmap difficultyBeatmap { get; set; }
        internal string songHash { get; set; }
    }
}