using HarmonyLib;

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches
{
    [HarmonyPatch(typeof(PauseController), nameof(PauseController.HandleHMDUnmounted))]
    internal class PatchHandleHMDUnmounted
    {
        internal static bool Prefix() {

            if (Plugin.ReplayState.isPlaybackEnabled) {
                return false;
            }
            return true;
        }
    }
}