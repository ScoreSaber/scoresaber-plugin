#region

using HarmonyLib;

#endregion

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches {
    [HarmonyPatch(typeof(PauseController), nameof(PauseController.HandleHMDUnmounted))]
    internal class PatchHandleHMDUnmounted {
        internal static bool Prefix() {
            switch (Plugin.ReplayState.IsPlaybackEnabled) {
                case true:
                    return false;
                default:
                    return true;
            }
        }
    }
}