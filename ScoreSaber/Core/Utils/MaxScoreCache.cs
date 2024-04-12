using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScoreSaber.Core.Utils {
    internal class MaxScoreCache {
        private readonly BeatmapLevelLoader _beatmapLevelLoader;
        private readonly BeatmapDataLoader _beatmapDataLoader;
 

        private Dictionary<BeatmapKey, int> cache = new Dictionary<BeatmapKey, int>();

        public MaxScoreCache(BeatmapLevelLoader beatmapLevelLoader, BeatmapDataLoader beatmapDataLoader) {
            _beatmapLevelLoader = beatmapLevelLoader;
            _beatmapDataLoader = beatmapDataLoader;
        }

        public async Task<int> GetMaxScore(BeatmapLevel beatmapLevel, BeatmapKey beatmapKey) {
            if (cache.ContainsKey(beatmapKey)) {
                return cache[beatmapKey];
            }

            var beatmapLevelData = (await _beatmapLevelLoader.LoadBeatmapLevelDataAsync(beatmapLevel, CancellationToken.None)).beatmapLevelData;
            var beatmapData = await _beatmapDataLoader.LoadBeatmapDataAsync(beatmapLevelData, beatmapKey, beatmapLevel.beatsPerMinute, false, null, null, null, false);
            int maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(beatmapData);

            cache[beatmapKey] = maxScore;
            return maxScore;
        }
    }
}
