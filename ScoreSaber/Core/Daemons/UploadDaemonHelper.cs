#region

using System;

#endregion

namespace ScoreSaber.Core.Daemons {
    internal static class UploadDaemonHelper {
        internal static Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> ThreeInstance;
        internal static Action<MultiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData> FourInstance;
    }
}