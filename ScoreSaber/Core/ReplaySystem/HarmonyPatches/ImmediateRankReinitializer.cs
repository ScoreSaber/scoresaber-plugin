#region

using HarmonyLib;
using System;

#endregion

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches {
    [HarmonyPatch(typeof(RelativeScoreAndImmediateRankCounter),
        nameof(RelativeScoreAndImmediateRankCounter.UpdateRelativeScoreAndImmediateRank))]
    internal class ImmediateRankReinitializer {
        internal static bool Prefix(RelativeScoreAndImmediateRankCounter __instance, int score, int maxPossibleScore,
            ref Action ___relativeScoreOrImmediateRankDidChangeEvent) {
            switch (Plugin.ReplayState.IsPlaybackEnabled) {
                case true when !Plugin.ReplayState.IsLegacyReplay: {
                    switch (score) {
                        case 0 when maxPossibleScore == 0:
                            Accessors.RelativeScore(ref __instance, 1f);
                            Accessors.ImmediateRank(ref __instance, RankModel.Rank.SS);
                            ___relativeScoreOrImmediateRankDidChangeEvent.Invoke();
                            return false;
                        default:
                            return true;
                    }
                }
                default:
                    return true;
            }
        }
    }
}