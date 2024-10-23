using ScoreSaber.Core.Data.Models;
using System.Collections.Generic;
using UnityEngine;

namespace ScoreSaber.Core.Data.Wrappers {
    internal class LeaderboardMap {
        internal LeaderboardInfoMap leaderboardInfoMap { get; set; }
        internal ScoreMap[] scores { get; set; }

        internal LeaderboardMap(Leaderboard leaderboard, BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, int maxMultipliedScore) {
            this.leaderboardInfoMap = new LeaderboardInfoMap(leaderboard.leaderboardInfo, beatmapLevel, beatmapKey);
            this.scores = new ScoreMap[leaderboard.scores.Length];
            for (int i = 0; i < leaderboard.scores.Length; i++) {
                this.scores[i] = new ScoreMap(leaderboard.scores[i], this.leaderboardInfoMap, maxMultipliedScore);
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
