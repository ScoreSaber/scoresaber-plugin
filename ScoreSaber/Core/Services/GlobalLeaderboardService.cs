using Newtonsoft.Json;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace ScoreSaber.Core.Services {

    internal class GlobalLeaderboardService {

        private readonly ScoreSaberHttpClient client;

        public enum GlobalPlayerScope {
            Global,
            AroundPlayer,
            Friends,
            Country,
            Region
        }

        public GlobalLeaderboardService(ScoreSaberHttpClient scoreSaberHttpClient) {
            client = scoreSaberHttpClient;
            Plugin.Log.Debug("GlobalLeaderboardService Setup");
        }

        public async Task<PlayerInfo[]> GetPlayerList(GlobalPlayerScope scope, int page) {
            var response = await client.GetAsync<PlayerCollection>(new Http.Endpoints.API.GlobalLeaderboardRequest(scope, page));
            return response.players;
        }
    }
}
