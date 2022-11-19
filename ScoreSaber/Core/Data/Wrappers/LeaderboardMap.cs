#region

using ScoreSaber.Core.Data.Models;
using System.Collections.Generic;

#endregion

namespace ScoreSaber.Core.Data.Wrappers {
    internal class LeaderboardMap {
        internal LeaderboardMap(Leaderboard leaderboard, IDifficultyBeatmap difficultyBeatmap,
            IReadonlyBeatmapData beatmapData) {
            leaderboardInfoMap = new LeaderboardInfoMap(leaderboard.leaderboardInfo, difficultyBeatmap);
            scores = new ScoreMap[leaderboard.scores.Length];
            for (int i = 0; i < leaderboard.scores.Length; i++) {
                scores[i] = new ScoreMap(leaderboard.scores[i], leaderboardInfoMap, beatmapData);
            }
        }

        internal LeaderboardInfoMap leaderboardInfoMap { get; set; }
        internal ScoreMap[] scores { get; set; }

        internal List<LeaderboardTableView.ScoreData> ToScoreData() {
            List<LeaderboardTableView.ScoreData> leaderboardTableScoreData = new List<LeaderboardTableView.ScoreData>();
            foreach (ScoreMap scoreMap in scores) {
                leaderboardTableScoreData.Add(new LeaderboardTableView.ScoreData(scoreMap.score.modifiedScore,
                    scoreMap.formattedPlayerName, scoreMap.score.rank, false));
            }

            return leaderboardTableScoreData;
        }
    }
}