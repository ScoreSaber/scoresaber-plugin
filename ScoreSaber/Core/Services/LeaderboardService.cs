using Newtonsoft.Json;
using System.Threading.Tasks;
using ScoreSaber.Core.Data.Wrappers;
using ScoreSaber.Core.Data.Models;
using System;
using System.Linq;
using ScoreSaber.UI.Leaderboard;
using System.Collections.Generic;

namespace ScoreSaber.Core.Services {
    internal class LeaderboardService {

        public LeaderboardMap currentLoadedLeaderboard = null;
        public CustomLevelLoader customLevelLoader = null;
        public BeatmapDataLoader beatmapDataLoader = null;

        public LeaderboardService() {
            Plugin.Log.Debug("LeaderboardService Setup");
        }

        public async Task<LeaderboardMap> GetLeaderboardData(int maxMultipliedScore, BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, ScoreSaber.UI.Leaderboard.ScoreSaberLeaderboardViewController.ScoreSaberScoresScope scope, int page, PlayerSpecificSettings playerSpecificSettings) {

            string leaderboardUrl = GetLeaderboardUrl(beatmapKey, beatmapLevel, scope, page);
            if (leaderboardUrl == null) {
                currentLoadedLeaderboard = null;
                return null;
            }
            string leaderboardRawData = await Plugin.HttpInstance.GetAsync(leaderboardUrl);
            Leaderboard leaderboardData = JsonConvert.DeserializeObject<Leaderboard>(leaderboardRawData);

            Plugin.Log.Debug($"Current leaderboard set to: {beatmapKey.levelId}:{beatmapLevel.songName}");
            currentLoadedLeaderboard = new LeaderboardMap(leaderboardData, beatmapLevel, beatmapKey, maxMultipliedScore);
            AddLeaderboardInfoMapToCache(currentLoadedLeaderboard.leaderboardInfoMap.beatmapKey, currentLoadedLeaderboard.leaderboardInfoMap);
            return currentLoadedLeaderboard;
        }

        public async Task<Leaderboard> GetCurrentLeaderboard(BeatmapKey beatmapKey, BeatmapLevel beatmapLevel) {

            string leaderboardUrl = GetLeaderboardUrl(beatmapKey, beatmapLevel, ScoreSaberLeaderboardViewController.ScoreSaberScoresScope.Global, 1);

            int attempts = 0;
            while (attempts < 4) {
                try {
                    string leaderboardRawData = await Plugin.HttpInstance.GetAsync(leaderboardUrl);
                    Leaderboard leaderboardData = JsonConvert.DeserializeObject<Leaderboard>(leaderboardRawData);
                    return leaderboardData;
                } catch (Exception) {
                }
                attempts++;
                await Task.Delay(1000);
            }
            return null;
        }
      
        private string GetLeaderboardUrl(BeatmapKey beatmapKey, BeatmapLevel beatmapLevel, ScoreSaberLeaderboardViewController.ScoreSaberScoresScope scope, int page) {
            
            string url = "/game/leaderboard";

            string leaderboardId = string.Empty;

            if (beatmapLevel.hasPrecalculatedData) {
                leaderboardId = beatmapLevel.levelID;
            } else {
                leaderboardId = beatmapKey.levelId.Split('_')[2];
            }

            string gameMode = $"Solo{beatmapKey.beatmapCharacteristic.serializedName}";
            string difficulty = BeatmapDifficultyMethods.DefaultRating(beatmapKey.difficulty).ToString();

            bool hasPage = true;

            switch (scope) {
                case ScoreSaberLeaderboardViewController.ScoreSaberScoresScope.Global:
                    url = $"{url}/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                    break;
                case ScoreSaberLeaderboardViewController.ScoreSaberScoresScope.Player:
                    url = $"{url}/around-player/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}";
                    hasPage = false;
                    break;
                case ScoreSaberLeaderboardViewController.ScoreSaberScoresScope.Friends:
                    url = $"{url}/around-friends/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                    break;
                case ScoreSaberLeaderboardViewController.ScoreSaberScoresScope.Area:
                    if (Plugin.Settings.locationFilterMode.ToLower() == "region") {
                        url = $"{url}/around-region/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                    } else if (Plugin.Settings.locationFilterMode.ToLower() == "country") {
                        url = $"{url}/around-country/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                    } else {
                        Plugin.Log.Error("Invalid location filter mode, falling back to country");
                        url = $"{url}/around-country/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                    }
                    break;
            }

            if (Plugin.Settings.hideNAScoresFromLeaderboard) {
                if (hasPage)
                    url = $"{url}&hideNA=1";
                else 
                    url = $"{url}?hideNA=1";
            }

            return url;
        }

        internal Dictionary<BeatmapKey, LeaderboardInfoMap> cachedLeaderboardInfoMaps = new Dictionary<BeatmapKey, LeaderboardInfoMap>();
        private int MaxLBInfoCacheSize = 100;
        internal Queue<BeatmapKey> LBInfoCacheQueue = new Queue<BeatmapKey>();
        internal void MaintainLeaderboardInfoMapCache() {
            while (cachedLeaderboardInfoMaps.Count > MaxLBInfoCacheSize) {
                BeatmapKey oldestUrl = LBInfoCacheQueue.Dequeue();
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

        internal LeaderboardInfoMap GetLeaderboardInfoMapFromCache(BeatmapKey url) {
            if (cachedLeaderboardInfoMaps.ContainsKey(url)) {
                return cachedLeaderboardInfoMaps[url];
            }
            return null;
        }
    }
}
