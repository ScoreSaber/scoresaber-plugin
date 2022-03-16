using Newtonsoft.Json;
using System.Threading.Tasks;
using ScoreSaber.Core.Data.Wrappers;
using ScoreSaber.Core.Data.Models;

namespace ScoreSaber.Core.Services {
    internal class LeaderboardService {

        public LeaderboardMap currentLoadedLeaderboard = null;

        public bool isRanked {
            get {
                if (currentLoadedLeaderboard == null) { return false; }
                return currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo.ranked;
            }
        }

        public LeaderboardService() {
            Plugin.Log.Debug("LeaderboardService Setup");
        }

        public async Task<LeaderboardMap> GetLeaderboardData(IDifficultyBeatmap difficultyBeatmap, PlatformLeaderboardsModel.ScoresScope scope, int page, bool filterAroundCountry = false) {
            string leaderboardUrl = GetLeaderboardUrl(difficultyBeatmap, scope, page, filterAroundCountry);
            string leaderboardRawData = await Plugin.HttpInstance.GetAsync(leaderboardUrl);
            Leaderboard leaderboardData = JsonConvert.DeserializeObject<Leaderboard>(leaderboardRawData);

            var beatmapData = await difficultyBeatmap.GetBeatmapDataAsync(difficultyBeatmap.GetEnvironmentInfo());

            currentLoadedLeaderboard = new LeaderboardMap(leaderboardData, difficultyBeatmap, beatmapData);
            return currentLoadedLeaderboard;
        }
      
        private string GetLeaderboardUrl(IDifficultyBeatmap difficultyBeatmap, PlatformLeaderboardsModel.ScoresScope scope, int page, bool filterAroundCountry) {

            string url = "/game/leaderboard";
            string leaderboardId = difficultyBeatmap.level.levelID.Split('_')[2];
            string gameMode = $"Solo{difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}";
            string difficulty = BeatmapDifficultyMethods.DefaultRating(difficultyBeatmap.difficulty).ToString();

            if (!filterAroundCountry) {
                switch (scope) {
                    case PlatformLeaderboardsModel.ScoresScope.Global:
                        url = $"{url}/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                        break;
                    case PlatformLeaderboardsModel.ScoresScope.AroundPlayer:
                        url = $"{url}/around-player/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}";
                        break;
                    case PlatformLeaderboardsModel.ScoresScope.Friends:
                        url = $"{url}/around-friends/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
                        break;
                }
            } else {
                url = $"{url}/around-country/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";
            }

            if (Plugin.Settings.hideNAScoresFromLeaderboard) {
                url = $"{url}&hideNA=1";
            }

            return url;
        }
    }
}
