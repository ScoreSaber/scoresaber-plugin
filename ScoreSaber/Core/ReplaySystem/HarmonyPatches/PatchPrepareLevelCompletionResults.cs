#region

using HarmonyLib;

#endregion

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches {
    [HarmonyPatch(typeof(PrepareLevelCompletionResults), nameof(PrepareLevelCompletionResults.FillLevelCompletionResults))]
    internal class PatchPrepareLevelCompletionResults {
        internal static void Prefix(ref LevelCompletionResults.LevelEndStateType levelEndStateType) {

            if (Plugin.ReplayState.IsPlaybackEnabled) {
                levelEndStateType = LevelCompletionResults.LevelEndStateType.Incomplete;
            }
        }
    }
}