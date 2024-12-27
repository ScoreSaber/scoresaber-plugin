using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSaber.Core.Utils {
    internal class BeatmapUtils {

        public static string FriendlyLevelAuthorName(string[] mappers, string[] lighters) {
            List<string> mappersAndLighters = new List<string>();
            mappersAndLighters.AddRange(mappers);
            mappersAndLighters.AddRange(lighters);

            if (mappersAndLighters.Count <= 1) {
                return mappersAndLighters.FirstOrDefault();
            }
            return $"{string.Join(", ", mappersAndLighters.Take(mappersAndLighters.Count - 1))} & {mappersAndLighters.Last()}";
        }

        public static string GetLevelIDFromHash(string hash, out bool isOst) {
            if (hash.Length == 40) {
                isOst = false;
                return "custom_level_" + hash;
            }
            isOst = true;
            return hash;
        }

        public static string GetHashFromLevelID(string levelID, out bool isOst) {
            if (levelID.StartsWith("custom_level_")) {
                isOst = false;
                return levelID.Split('_')[2];
            }
            isOst = true;
            return "ost_" + levelID;
        }

        public static string GetHashFromLevelID(BeatmapLevel beatmapLevel, out bool isOst) {
            return GetHashFromLevelID(beatmapLevel.levelID, out isOst);
        }

        public static string GetHashFromLevelID(BeatmapKey beatmapKey, out bool isOst) {
            return GetHashFromLevelID(beatmapKey.levelId, out isOst);
        }

        public static string GetLevelIDFromHash(BeatmapLevel beatmapLevel, out bool isOst) {
            return GetLevelIDFromHash(beatmapLevel.levelID, out isOst);
        }

        public static string GetLevelIDFromHash(BeatmapKey beatmapKey, out bool isOst) {
            return GetLevelIDFromHash(beatmapKey.levelId, out isOst);
        }
    }
}
