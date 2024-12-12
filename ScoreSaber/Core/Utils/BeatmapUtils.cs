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
    }
}
