#region

using ScoreSaber.Core.ReplaySystem.Data;
using System.Collections.Generic;

#endregion

namespace ScoreSaber.Core.ReplaySystem {
    internal class ReplayState {
        // State management
        internal IDifficultyBeatmap CurrentLevel;
        internal GameplayModifiers CurrentModifiers;
        internal string CurrentPlayerName;

        // Legacy 
        internal bool IsLegacyReplay;
        internal bool IsPlaybackEnabled;
        internal List<LegacyReplayFile.Keyframe> LoadedLegacyKeyframes;

        // New
        internal ReplayFile LoadedReplayFile;
    }
}