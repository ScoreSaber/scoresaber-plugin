#region

using ScoreSaber.Core.Data.Models;
using System.Collections.Generic;

#endregion

namespace ScoreSaber.Core.Data.Wrappers {
    internal class LeaderboardMap {
        internal LeaderboardInfoMap leaderboardInfoMap { get; set; }
        internal ScoreMap[] scores { get; set; }

        internal LeaderboardMap(Leaderboard leaderboard, IDifficultyBeatmap difficultyBeatmap, IReadonlyBeatmapData beatmapData) {
            this.leaderboardInfoMap = new LeaderboardInfoMap(leaderboard.leaderboardInfo, difficultyBeatmap);
            this.scores = new ScoreMap[leaderboard.scores.Length];
            for (int i = 0; i < leaderboard.scores.Length; i++) {
                this.scores[i] = new ScoreMap(leaderboard.scores[i], this.leaderboardInfoMap, beatmapData);
            }
        }

        internal List<LeaderboardTableView.ScoreData> ToScoreData() {

            var leaderboardTableScoreData = new List<LeaderboardTableView.ScoreData>();
            foreach (ScoreMap scoreMap in this.scores) {
                leaderboardTableScoreData.Add(new LeaderboardTableView.ScoreData(scoreMap.score.modifiedScore, scoreMap.formattedPlayerName, scoreMap.score.rank, false));
            }
            return leaderboardTableScoreData;
        }
    }
}