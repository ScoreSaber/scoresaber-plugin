using HarmonyLib;

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteCutter), nameof(NoteCutter.Cut))]
    internal class SaberDoDisableCuttingPatch
    {
        internal static bool Prefix(ref Saber __instance) {

            if (Plugin.ReplayState.isPlaybackEnabled && !Plugin.ReplayState.isLegacyReplay) {
                return !__instance.disableCutting;
            }
            return true;
        }
    }
}
