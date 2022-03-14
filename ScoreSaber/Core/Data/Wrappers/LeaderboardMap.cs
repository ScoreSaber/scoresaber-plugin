using ScoreSaber.Core.Data.Models;
using System.Collections.Generic;

namespace ScoreSaber.Core.Data.Wrappers {
    internal class LeaderboardMap {
        internal LeaderboardInfoMap leaderboardInfoMap { get; set; }
        internal ScoreMap[] scores { get; set; }

        internal LeaderboardMap(Leaderboard leaderboard, IDifficultyBeatmap difficultyBeatmap) {
            this.leaderboardInfoMap = new LeaderboardInfoMap(leaderboard.leaderboardInfo, difficultyBeatmap);
            this.scores = new ScoreMap[leaderboard.scores.Length];
            for (int i = 0; i < leaderboard.scores.Length; i++) {
                this.scores[i] = new ScoreMap(leaderboard.scores[i], this.leaderboardInfoMap);
            }
        }

        internal List<LeaderboardTableView.ScoreData> ToScoreData() {

            List<LeaderboardTableView.ScoreData> leaderboardTableScoreData = new List<LeaderboardTableView.ScoreData>();
            foreach (ScoreMap scoreMap in this.scores) {
                leaderboardTableScoreData.Add(new LeaderboardTableView.ScoreData(scoreMap.score.modifiedScore, scoreMap.formattedPlayerName, scoreMap.score.rank, false));
            }
            return leaderboardTableScoreData;
        }
    }
}
