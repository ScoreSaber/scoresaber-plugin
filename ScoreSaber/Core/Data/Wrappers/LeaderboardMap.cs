#region

using ScoreSaber.Core.Data.Models;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace ScoreSaber.Core.Data.Wrappers {
    internal class LeaderboardMap {
        internal LeaderboardInfoMap LeaderboardInfoMap { get; set; }
        internal ScoreMap[] Scores { get; set; }

        internal LeaderboardMap(Leaderboard leaderboard, IDifficultyBeatmap difficultyBeatmap, IReadonlyBeatmapData beatmapData) {

            LeaderboardInfoMap = new LeaderboardInfoMap(leaderboard.LeaderboardInfo, difficultyBeatmap);
            Scores = new ScoreMap[leaderboard.Scores.Length];
            for (int i = 0; i < leaderboard.Scores.Length; i++) {
                Scores[i] = new ScoreMap(leaderboard.Scores[i], LeaderboardInfoMap, beatmapData);
            }
        }

        internal List<LeaderboardTableView.ScoreData> ToScoreData() {

            return Scores.Select(scoreMap => new LeaderboardTableView.ScoreData(scoreMap.Score.ModifiedScore,
                scoreMap.FormattedPlayerName, scoreMap.Score.Rank, false)).ToList();
        }
    }
}