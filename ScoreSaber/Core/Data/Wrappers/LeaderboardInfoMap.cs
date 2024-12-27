using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Utils;

namespace ScoreSaber.Core.Data.Wrappers {
    internal class LeaderboardInfoMap {
        internal LeaderboardInfo leaderboardInfo { get; set; }
        internal BeatmapLevel beatmapLevel { get; set; }
        internal BeatmapKey beatmapKey { get; set; }
        internal string songHash { get; set; }
        internal bool isOst { get; set; }

        internal LeaderboardInfoMap(LeaderboardInfo leaderboardInfo, BeatmapLevel beatmapLevel, BeatmapKey beatmapKey) {
            this.beatmapLevel = beatmapLevel;
            this.beatmapKey = beatmapKey;
            this.leaderboardInfo = leaderboardInfo;
            this.songHash = BeatmapUtils.GetHashFromLevelID(beatmapKey, out bool isOst);
            this.isOst = isOst;
        }
    }
}
