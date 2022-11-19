#region

using System;

#endregion

namespace ScoreSaber.Core.Daemons {
    internal static class UploadDaemonHelper {
        internal static Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> StandardSceneTransitionInstance;
        internal static Action<MultiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData> MultiplayerSceneTransitionInstance;
    }
}