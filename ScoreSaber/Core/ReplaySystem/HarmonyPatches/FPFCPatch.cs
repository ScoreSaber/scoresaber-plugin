#region

using SiraUtil.Affinity;
using SiraUtil.Attributes;

#endregion

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches {
    [Bind]
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
    }
}