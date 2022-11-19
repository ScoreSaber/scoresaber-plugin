using Zenject;

namespace ScoreSaber.Core.ReplaySystem {
    internal abstract class TimeSynchronizer {
        [Inject]
        protected readonly AudioTimeSyncController audioTimeSyncController = null;
    }
}