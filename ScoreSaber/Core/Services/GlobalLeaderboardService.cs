#region

using Newtonsoft.Json;
using ScoreSaber.Core.Data.Models;
using System.Threading.Tasks;

#endregion

namespace ScoreSaber.Core.Services {
    internal class GlobalLeaderboardService {
        public enum GlobalPlayerScope {
            Global,
            AroundPlayer,
            Friends,
            Country
        }

        public GlobalLeaderboardService() {
            Plugin.Log.Debug("GlobalLeaderboardService Setup");
        }

        public async Task<PlayerInfo[]> GetPlayerList(GlobalPlayerScope scope, int page) {
            string url = BuildUrl(scope, page);

            string response = await Plugin.HttpInstance.GetAsync(url);
            PlayerCollection globalLeaderboardData = JsonConvert.DeserializeObject<PlayerCollection>(response);
            return globalLeaderboardData.players;
        }

        private string BuildUrl(GlobalPlayerScope scope, int page) {
            string url = "/game/players";
            switch (scope) {
                case GlobalPlayerScope.Global:
                    url = $"{url}?page={page}";
                    break;
                case GlobalPlayerScope.AroundPlayer:
                    url = $"{url}/around-player";
                    break;
                case GlobalPlayerScope.Friends:
                    url = $"{url}/around-friends?page={page}";
                    break;
                case GlobalPlayerScope.Country:
                    url = $"{url}/around-country?page={page}";
                    break;
            }

            return url;
        }
    }
}