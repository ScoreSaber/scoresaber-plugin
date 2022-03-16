using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Utils;
using System;

namespace ScoreSaber.Core.Data.Wrappers {
    internal class ScoreMap {

        internal Score score { get; private set; }

        //Extra
        internal LeaderboardInfoMap parent { get; set; }
        internal bool hasLocalReplay { get; set; }
        internal double accuracy { get; set; }
        internal GameplayModifiers gameplayModifiers { get; set; }
        internal string formattedPlayerName { get; set; }

        internal ScoreMap(Score _score, LeaderboardInfoMap customLeaderboardInfo, IReadonlyBeatmapData beatmapData) {
            score = _score;

            GameplayModifiersMap replayMods = new GameplayModifiersMap();
            if (!string.IsNullOrEmpty(score.modifiers)) {
                if (score.modifiers.Contains(",")) {
                    replayMods = LeaderboardUtils.GetModifierFromStrings(score.modifiers.Split(','), false);
                } else {
                    replayMods = LeaderboardUtils.GetModifierFromStrings(new string[] { score.modifiers }, false);
                }
            }

            double maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(beatmapData) * replayMods.totalMultiplier;

            this.parent = customLeaderboardInfo;
            this.hasLocalReplay = LeaderboardUtils.LocalReplayExists(customLeaderboardInfo.difficultyBeatmap, this);
            this.score.weight = Math.Round(score.weight * 100, 2);
            this.score.pp = Math.Round(score.pp, 2);
            this.accuracy = Math.Round((score.modifiedScore / maxScore) * 100, 2);
            this.gameplayModifiers = replayMods.gameplayModifiers;
            if (this.hasLocalReplay) {
                this.score.hasReplay = true;
            }
            this.formattedPlayerName = LeaderboardUtils.GetFormattedName(this);
        }
      
    }
}
