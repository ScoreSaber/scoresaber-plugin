#region

using SiraUtil.Affinity;

#endregion

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches {
    internal class CancelSaberCuttingPatch : IAffinity {
        private readonly SaberManager _saberManager;

        public CancelSaberCuttingPatch(SaberManager saberManager) {
            _saberManager = saberManager;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(NoteCutter), nameof(NoteCutter.Cut))]
        private bool CancelCut(Saber saber) {
            return !(saber == _saberManager.leftSaber || saber == _saberManager.rightSaber);
        }
    }
}