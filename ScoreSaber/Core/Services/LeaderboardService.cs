#region

using Newtonsoft.Json;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Data.Wrappers;
using System;
using System.Threading.Tasks;

#endregion

namespace ScoreSaber.Core.Services {
    internal class LeaderboardService {
        public LeaderboardMap currentLoadedLeaderboard;

        public LeaderboardService() {
            Plugin.Log.Debug("LeaderboardService Setup");
        }

        public async Task<LeaderboardMap> GetLeaderboardData(IDifficultyBeatmap difficultyBeatmap,
            PlatformLeaderboardsModel.ScoresScope scope, int page, PlayerSpecificSettings playerSpecificSettings,
            bool filterAroundCountry = false) {
            string leaderboardUrl = GetLeaderboardUrl(difficultyBeatmap, scope, page, filterAroundCountry);
            string leaderboardRawData = await Plugin.HttpInstance.GetAsync(leaderboardUrl);
            Leaderboard leaderboardData = JsonConvert.DeserializeObject<Leaderboard>(leaderboardRawData);

            IReadonlyBeatmapData beatmapData =
                await difficultyBeatmap.GetBeatmapDataAsync(difficultyBeatmap.GetEnvironmentInfo(),
                    playerSpecificSettings);

            Plugin.Log.Debug(
                $"Current leaderboard set to: {difficultyBeatmap.level.levelID}:{difficultyBeatmap.level.songName}");
            currentLoadedLeaderboard = new LeaderboardMap(leaderboardData, difficultyBeatmap, beatmapData);
            return currentLoadedLeaderboard;
        }

        public async Task<Leaderboard> GetCurrentLeaderboard(IDifficultyBeatmap difficultyBeatmap) {
            string leaderboardUrl =
                GetLeaderboardUrl(difficultyBeatmap, PlatformLeaderboardsModel.ScoresScope.Global, 1, false);

            int attempts = 0;
            while (attempts < 4) {
                try {
                    string leaderboardRawData = await Plugin.HttpInstance.GetAsync(leaderboardUrl);
                    Leaderboard leaderboardData = JsonConvert.DeserializeObject<Leaderboard>(leaderboardRawData);
                    return leaderboardData;
                } catch (Exception) {
                    // ignored
                }

                attempts++;
                await Task.Delay(1000);
            }

            return null;
        }

        private string GetLeaderboardUrl(IDifficultyBeatmap difficultyBeatmap,
            PlatformLeaderboardsModel.ScoresScope scope, int page, bool filterAroundCountry) {
            string url = "/game/leaderboard";
            string leaderboardId = difficultyBeatmap.level.levelID.Split('_')[2];
            string gameMode =
                $"Solo{difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}";
            string difficulty = difficultyBeatmap.difficulty.DefaultRating().ToString();

            switch (filterAroundCountry) {
                case false:
                    switch (scope) {
                        case PlatformLeaderboardsModel.ScoresScope.Global:
                            url = $"{url}/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                            break;
                        case PlatformLeaderboardsModel.ScoresScope.AroundPlayer:
                            url = $"{url}/around-player/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}";
                            break;
                        case PlatformLeaderboardsModel.ScoresScope.Friends:
                            url =
                                $"{url}/around-friends/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                            break;
                    }

                    break;
                default:
                    url = $"{url}/around-country/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                    break;
            }

            switch (Plugin.Settings.hideNAScoresFromLeaderboard) {
                case true:
                    url = $"{url}&hideNA=1";
                    break;
            }

            return url;
        }
    }
}