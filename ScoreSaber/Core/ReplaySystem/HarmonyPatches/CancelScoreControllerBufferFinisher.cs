using HarmonyLib;

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches
{
    [HarmonyPatch(typeof(ScoreController), nameof(ScoreController.HandleCutScoreBufferDidFinish))]
    internal class CancelScoreControllerBufferFinisher
    {
        internal static bool Prefix(ScoreController __instance, CutScoreBuffer cutScoreBuffer, MemoryPoolContainer<CutScoreBuffer> ____cutScoreBufferMemoryPoolContainer) {
            
            if (Plugin.ReplayState.isPlaybackEnabled && !Plugin.ReplayState.isLegacyReplay) {
                cutScoreBuffer.didFinishEvent.Remove(__instance);
                ____cutScoreBufferMemoryPoolContainer.Despawn(cutScoreBuffer);
                return false;
            }
            return true;
        }
    }
}
