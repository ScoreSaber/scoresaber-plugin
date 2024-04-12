using ScoreSaber.Core.ReplaySystem.Data;
using System.Collections.Generic;

namespace ScoreSaber.Core.ReplaySystem
{
    internal class ReplayState
    {
        // State management
        internal BeatmapLevel CurrentBeatmapLevel;
        internal BeatmapKey CurrentBeatmapKey;
        internal GameplayModifiers CurrentModifiers;
        internal string CurrentPlayerName;

        // Legacy 
        internal bool IsLegacyReplay;
        internal bool IsPlaybackEnabled;
        internal List<Z.Keyframe> LoadedLegacyKeyframes;

        // New
        internal ReplayFile LoadedReplayFile;
    }
}
