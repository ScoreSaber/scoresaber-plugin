using ScoreSaber.Core.Data.Models;

namespace ScoreSaber.Core.Data.Wrappers {
    internal class LeaderboardInfoMap {
        internal LeaderboardInfo LeaderboardInfo { get; set; }
        internal IDifficultyBeatmap DifficultyBeatmap { get; set; }
        internal string SongHash { get; set; }

        internal LeaderboardInfoMap(LeaderboardInfo leaderboardInfo, IDifficultyBeatmap difficultyBeatmap) {

            DifficultyBeatmap = difficultyBeatmap;
            LeaderboardInfo = leaderboardInfo;
            SongHash = difficultyBeatmap.level.levelID.Split('_')[2];
        }
    }
}