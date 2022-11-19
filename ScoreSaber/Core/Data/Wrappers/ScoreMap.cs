#region

using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Utils;
using System;

#endregion

namespace ScoreSaber.Core.Data.Wrappers {
    internal class ScoreMap {
        internal ScoreMap(Score _score, LeaderboardInfoMap customLeaderboardInfo, IReadonlyBeatmapData beatmapData) {
            score = _score;

            GameplayModifiersMap replayMods = new GameplayModifiersMap();
            switch (string.IsNullOrEmpty(score.modifiers)) {
                case false when score.modifiers.Contains(","):
                    replayMods = LeaderboardUtils.GetModifierFromStrings(score.modifiers.Split(','), false);
                    break;
                case false:
                    replayMods = LeaderboardUtils.GetModifierFromStrings(new[] { score.modifiers }, false);
                    break;
            }

            double maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(beatmapData) * replayMods.totalMultiplier;

            parent = customLeaderboardInfo;
            hasLocalReplay = LeaderboardUtils.LocalReplayExists(customLeaderboardInfo.difficultyBeatmap, this);
            score.weight = Math.Round(score.weight * 100, 2);
            score.pp = Math.Round(score.pp, 2);
            accuracy = Math.Round(score.modifiedScore / maxScore * 100, 2);
            gameplayModifiers = replayMods.gameplayModifiers;
            switch (hasLocalReplay) {
                case true:
                    score.hasReplay = true;
                    break;
            }

            formattedPlayerName = LeaderboardUtils.GetFormattedName(this);
        }

        internal Score score { get; }

        //Extra
        internal LeaderboardInfoMap parent { get; set; }
        internal bool hasLocalReplay { get; set; }
        internal double accuracy { get; set; }
        internal GameplayModifiers gameplayModifiers { get; set; }
        internal string formattedPlayerName { get; set; }
    }
}