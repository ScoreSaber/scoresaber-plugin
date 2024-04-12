using Newtonsoft.Json;
using System.Threading.Tasks;
using ScoreSaber.Core.Data.Wrappers;
using ScoreSaber.Core.Data.Models;
using System;
using System.Linq;

namespace ScoreSaber.Core.Services {
    internal class LeaderboardService {

        public LeaderboardMap currentLoadedLeaderboard = null;
        public CustomLevelLoader customLevelLoader = null;
        public BeatmapDataLoader beatmapDataLoader = null;

        public LeaderboardService() {
            Plugin.Log.Debug("LeaderboardService Setup");
        }

        public async Task<LeaderboardMap> GetLeaderboardData(int maxMultipliedScore, BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, PlatformLeaderboardsModel.ScoresScope scope, int page, PlayerSpecificSettings playerSpecificSettings, bool filterAroundCountry = false) {

            string leaderboardUrl = GetLeaderboardUrl(beatmapKey, scope, page, filterAroundCountry);
            string leaderboardRawData = await Plugin.HttpInstance.GetAsync(leaderboardUrl);
            Leaderboard leaderboardData = JsonConvert.DeserializeObject<Leaderboard>(leaderboardRawData);

            Plugin.Log.Debug($"Current leaderboard set to: {beatmapKey.levelId}:{beatmapLevel.songName}");
            currentLoadedLeaderboard = new LeaderboardMap(leaderboardData, beatmapLevel, beatmapKey, maxMultipliedScore);
            return currentLoadedLeaderboard;
        }

        public async Task<Leaderboard> GetCurrentLeaderboard(BeatmapKey beatmapKey) {

            string leaderboardUrl = GetLeaderboardUrl(beatmapKey, PlatformLeaderboardsModel.ScoresScope.Global, 1, false);

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
      
        private string GetLeaderboardUrl(BeatmapKey beatmapKey, PlatformLeaderboardsModel.ScoresScope scope, int page, bool filterAroundCountry) {

            string url = "/game/leaderboard";
            string leaderboardId = beatmapKey.levelId.Split('_')[2];
            string gameMode = $"Solo{beatmapKey.beatmapCharacteristic.serializedName}";
            string difficulty = BeatmapDifficultyMethods.DefaultRating(beatmapKey.difficulty).ToString();

            bool hasPage = true;

            if (!filterAroundCountry) {
                switch (scope) {
                    case PlatformLeaderboardsModel.ScoresScope.Global:
                        url = $"{url}/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                        break;
                    case PlatformLeaderboardsModel.ScoresScope.AroundPlayer:
                        url = $"{url}/around-player/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}";
                        hasPage = false;
                        break;
                    case PlatformLeaderboardsModel.ScoresScope.Friends:
                        url = $"{url}/around-friends/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                        break;
                }
            } else {
                if(Plugin.Settings.locationFilterMode.ToLower() == "region") {
                    url = $"{url}/around-region/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                }
                else if(Plugin.Settings.locationFilterMode.ToLower() == "country") {
                    url = $"{url}/around-country/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                } else {
                    Plugin.Log.Error("Invalid location filter mode, falling back to country");
                    url = $"{url}/around-country/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                }
            }

            if (Plugin.Settings.hideNAScoresFromLeaderboard) {
                if (hasPage)
                    url = $"{url}&hideNA=1";
                else 
                    url = $"{url}?hideNA=1";
            }

            return url;
        }
    }
}
