using SiraUtil.Affinity;

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches {
    internal class CancelSaberCuttingPatch : IAffinity {

        private readonly SaberManager _saberManager;

        public CancelSaberCuttingPatch(SaberManager saberManager) {
            
            _saberManager = saberManager;
        }

        [AffinityPrefix, AffinityPatch(typeof(NoteCutter), nameof(NoteCutter.Cut))]
        private bool CancelCut(ref Saber __instance) {

            return !(__instance == _saberManager.leftSaber || __instance == _saberManager.rightSaber);
        }
    }
}