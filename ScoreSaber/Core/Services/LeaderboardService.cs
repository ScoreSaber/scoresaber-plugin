#nullable enable
using Newtonsoft.Json;
using System.Threading.Tasks;
using ScoreSaber.Core.Data.Wrappers;
using ScoreSaber.Core.Data.Models;
using System;
using System.Linq;
using ScoreSaber.UI.Leaderboard;
using System.Collections.Generic;
using ScoreSaber.Core.Http;
using ScoreSaber.Core.Utils;

namespace ScoreSaber.Core.Services {
    internal class LeaderboardService {

        private readonly ScoreSaberHttpClient client;
        public LeaderboardMap? currentLoadedLeaderboard;
        public CustomLevelLoader? customLevelLoader = null;
        public BeatmapDataLoader? beatmapDataLoader = null;

        public LeaderboardService(ScoreSaberHttpClient client) {
            this.client = client;
            Plugin.Log.Debug("LeaderboardService Setup");
        }

        public async Task<LeaderboardMap> GetLeaderboardData(
            int maxMultipliedScore,
            BeatmapLevel beatmapLevel,
            BeatmapKey beatmapKey,
            ScoreSaberLeaderboardViewController.ScoreSaberScoresScope scope,
            int page) {
            var leaderboardData = await client.GetAsync<Leaderboard>(new Http.Endpoints.API.LeaderboardRequest(
                leaderboardId: BeatmapUtils.GetHashFromLevelID(beatmapKey.levelId, out _),
                gameMode: $"Solo{beatmapKey.beatmapCharacteristic.serializedName}",
                difficulty: BeatmapDifficultyMethods.DefaultRating(beatmapKey.difficulty).ToString(),
                scope: scope,
                page: page,
                hideNA: Plugin.Settings.hideNAScoresFromLeaderboard
            ));
            Plugin.Log.Debug($"Current leaderboard set to: {beatmapKey.levelId}:{beatmapLevel.songName}");
            currentLoadedLeaderboard = new LeaderboardMap(leaderboardData, beatmapLevel, beatmapKey, maxMultipliedScore);
            AddLeaderboardInfoMapToCache(currentLoadedLeaderboard.leaderboardInfoMap.beatmapKey, currentLoadedLeaderboard.leaderboardInfoMap);
            return currentLoadedLeaderboard;
        }


        public async Task<Leaderboard?> GetCurrentLeaderboard(BeatmapKey beatmapKey, BeatmapLevel beatmapLevel) {
            const int maxAttempts = 4;
            const int retryDelayMs = 1000;
            var request = new Http.Endpoints.API.LeaderboardRequest(
                leaderboardId: BeatmapUtils.GetHashFromLevelID(beatmapKey, out _),
                gameMode: $"Solo{beatmapKey.beatmapCharacteristic.serializedName}",
                difficulty: BeatmapDifficultyMethods.DefaultRating(beatmapKey.difficulty).ToString(),
                scope: ScoreSaberLeaderboardViewController.ScoreSaberScoresScope.Global
            );
            for (int attempt = 0; attempt < maxAttempts; attempt++) {
                try {
                    return await client.GetAsync<Leaderboard>(request);
                } catch (Exception) {
                    if (attempt < maxAttempts - 1) {
                        await Task.Delay(retryDelayMs);
                    }
                }
            }
            return null;
        }

        internal Dictionary<BeatmapKey, LeaderboardInfoMap> cachedLeaderboardInfoMaps = new Dictionary<BeatmapKey, LeaderboardInfoMap>();
        private int MaxLBInfoCacheSize = 100;
        internal Queue<BeatmapKey> LBInfoCacheQueue = new Queue<BeatmapKey>();
        internal void MaintainLeaderboardInfoMapCache() {
            while (cachedLeaderboardInfoMaps.Count > MaxLBInfoCacheSize) {
                var oldestUrl = LBInfoCacheQueue.Dequeue();
                cachedLeaderboardInfoMaps.Remove(oldestUrl);
            }
        }

        internal void AddLeaderboardInfoMapToCache(BeatmapKey url, LeaderboardInfoMap LeaderboardInfoMap) {
            if (cachedLeaderboardInfoMaps.ContainsKey(url)) {
                return;
            }
            cachedLeaderboardInfoMaps.Add(url, LeaderboardInfoMap);
            LBInfoCacheQueue.Enqueue(url);
            MaintainLeaderboardInfoMapCache();
        }

        internal LeaderboardInfoMap? GetLeaderboardInfoMapFromCache(BeatmapKey url) {
            return cachedLeaderboardInfoMaps.TryGetValue(url, out var map) ? map : null;
        }
    }
}
