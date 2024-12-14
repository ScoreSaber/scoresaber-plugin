using ScoreSaber.Core.Data.Models;

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
            if (beatmapLevel.hasPrecalculatedData) {
                this.songHash = "ost_" + beatmapLevel.levelID;
                this.isOst = true;
            } else {
                this.songHash = beatmapKey.levelId.Split('_')[2];
                this.isOst = false;
            }
        }
    }
}
