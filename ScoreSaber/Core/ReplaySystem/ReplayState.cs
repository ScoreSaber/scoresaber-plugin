using ScoreSaber.Core.ReplaySystem.Data;
using System.Collections.Generic;

namespace ScoreSaber.Core.ReplaySystem
{
    internal class ReplayState
    {
        internal string currentRequestId;

        internal bool isLegacyReplay;
        internal bool isPlaybackEnabled;

        internal byte[] serializedReplay;
        internal ReplayFile file;
        internal List<Z.Keyframe> legacyKeyframes;

        internal IDifficultyBeatmap currentLevel;
        internal GameplayModifiers currentModifiers;
        internal string currentPlayerName;
    }
}
