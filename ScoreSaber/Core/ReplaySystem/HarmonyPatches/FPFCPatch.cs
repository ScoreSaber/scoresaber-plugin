using SiraUtil.Affinity;
using SiraUtil.Attributes;

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches {
    [Bind]
    internal class FPFCPatch : IAffinity {

        private readonly bool _needsPatching;
        private readonly IVRPlatformHelper _vrPlatformHelper;

        public FPFCPatch(IVRPlatformHelper vrPlatformHelper) {
            _vrPlatformHelper = vrPlatformHelper;
            _needsPatching = _vrPlatformHelper is OculusVRHelper || _vrPlatformHelper is UnityXRHelper;
        }

        [AffinityPatch(typeof(OculusVRHelper), nameof(OculusVRHelper.hasInputFocus), AffinityMethodType.Getter)]
        protected void ForceInputFocusOculusVR(ref bool __result) {
            if (_needsPatching && Plugin.ReplayState.IsPlaybackEnabled)
                __result = true;
        }

        [AffinityPatch(typeof(UnityXRHelper), nameof(UnityXRHelper.hasInputFocus), AffinityMethodType.Getter)]
        protected void ForceInputFocusUnityXR(ref bool __result) {
            if (_needsPatching && Plugin.ReplayState.IsPlaybackEnabled)
                __result = true;
        }
    }
}