#region

using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Utils;
using System;

#endregion

namespace ScoreSaber.Core.Data.Wrappers {
    internal class ScoreMap {

        internal Score Score { get; private set; }

        //Extra
        internal LeaderboardInfoMap Parent { get; set; }
        internal bool HasLocalReplay { get; set; }
        internal double Accuracy { get; set; }
        internal GameplayModifiers GameplayModifiers { get; set; }
        internal string FormattedPlayerName { get; set; }

        internal ScoreMap(Score _score, LeaderboardInfoMap customLeaderboardInfo, IReadonlyBeatmapData beatmapData) {

            Score = _score;

            var replayMods = new GameplayModifiersMap();
            if (!string.IsNullOrEmpty(Score.Modifiers)) {
                replayMods = LeaderboardUtils.GetModifierFromStrings(
                    Score.Modifiers.Contains(",") ? Score.Modifiers.Split(',') : new[] { Score.Modifiers }, false);
            }

            double maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(beatmapData) * replayMods.TotalMultiplier;

            Parent = customLeaderboardInfo;
            HasLocalReplay = LeaderboardUtils.LocalReplayExists(customLeaderboardInfo.DifficultyBeatmap, this);
            Score.Weight = Math.Round(Score.Weight * 100, 2);
            Score.PP = Math.Round(Score.PP, 2);
            Accuracy = Math.Round((Score.ModifiedScore / maxScore) * 100, 2);
            GameplayModifiers = replayMods.GameplayModifiers;
            if (HasLocalReplay) {
                Score.HasReplay = true;
            }
            FormattedPlayerName = LeaderboardUtils.GetFormattedName(this);
        }

    }
}