#nullable enable
using ScoreSaber.Core.Http.Configuration;
using ScoreSaber.Core.Services;
using System.Net;
namespace ScoreSaber.Core.Http.Endpoints.API {
    internal class GlobalLeaderboardRequest : Endpoint {
        public GlobalLeaderboardRequest(GlobalLeaderboardService.GlobalPlayerScope scope, int page = 1)
            : base(ApiConfig.UrlBases.APIv1) {
            PathSegments.Add("players");
            switch (scope) {
                case GlobalLeaderboardService.GlobalPlayerScope.Global:
                    QueryParams["page"] = page.ToString();
                    break;
                case GlobalLeaderboardService.GlobalPlayerScope.AroundPlayer:
                    PathSegments.Add("around-player");
                    break;
                case GlobalLeaderboardService.GlobalPlayerScope.Friends:
                    PathSegments.Add("around-friends");
                    QueryParams["page"] = page.ToString();
                    break;
                case GlobalLeaderboardService.GlobalPlayerScope.Country:
                    PathSegments.Add("around-country");
                    QueryParams["page"] = page.ToString();
                    break;
                case GlobalLeaderboardService.GlobalPlayerScope.Region:
                    PathSegments.Add("around-region");
                    QueryParams["page"] = page.ToString();
                    break;
            }
        }
    }
}