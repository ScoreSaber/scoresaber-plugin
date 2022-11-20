#region

using HarmonyLib;
using System;

#endregion

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches {
    [HarmonyPatch(typeof(RelativeScoreAndImmediateRankCounter), nameof(RelativeScoreAndImmediateRankCounter.UpdateRelativeScoreAndImmediateRank))]
    internal class ImmediateRankReinitializer {
        internal static bool Prefix(RelativeScoreAndImmediateRankCounter __instance, int score, int maxPossibleScore, ref Action ___relativeScoreOrImmediateRankDidChangeEvent) {

            if (!Plugin.ReplayState.IsPlaybackEnabled || Plugin.ReplayState.IsLegacyReplay) {
                return true;
            }

            if (score != 0 || maxPossibleScore != 0) {
                return true;
            }

            Accessors.RelativeScore(ref __instance, 1f);
            Accessors.ImmediateRank(ref __instance, RankModel.Rank.SS);
            ___relativeScoreOrImmediateRankDidChangeEvent.Invoke();
            return false;
        }
    }
}