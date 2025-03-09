using SiraUtil.Affinity;
using SiraUtil.Attributes;

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches {
    internal class FPFCPatch : IAffinity {

        private readonly bool _isOculus;
        private readonly IVRPlatformHelper _vrPlatformHelper;

        public FPFCPatch(IVRPlatformHelper vrPlatformHelper) {
            _vrPlatformHelper = vrPlatformHelper;
            _isOculus = _vrPlatformHelper is OculusVRHelper;
        }

        [AffinityPatch(typeof(OculusVRHelper), nameof(OculusVRHelper.hasInputFocus), AffinityMethodType.Getter)]
        protected void ForceInputFocus(ref bool __result) {
            if (_isOculus && Plugin.ReplayState.IsPlaybackEnabled)
                __result = true;
        }

        // we can stop the pause menu, because we control pauses through audiotimesynccontroller
        // this fixes the pause menu showing up when theres an hmd connected while in fpfc
        [AffinityPatch(typeof(PauseController), nameof(PauseController.Pause))]
        [AffinityPrefix]
        protected bool Pause() {
            if (Plugin.ReplayState.IsPlaybackEnabled)
                return false;

            return true;
        }
    }
}