using ScoreSaber.Core.Http.Configuration;
using ScoreSaber.UI.Leaderboard;
using System.Net;
namespace ScoreSaber.Core.Http.Endpoints.API {
    internal class LeaderboardRequest : Endpoint {
        public LeaderboardRequest(string leaderboardId,
                                string gameMode,
                                string difficulty,
                                ScoreSaberLeaderboardViewController.ScoreSaberScoresScope scope,
                                int page = 1,
                                bool hideNA = false) : base(ApiConfig.UrlBases.APIv1) {

            PathSegments.Add("leaderboard");
            // Add scope-specific path segments
            switch (scope) {
                case ScoreSaberLeaderboardViewController.ScoreSaberScoresScope.Player:
                    PathSegments.Add("around-player");
                    break;
                case ScoreSaberLeaderboardViewController.ScoreSaberScoresScope.Friends:
                    PathSegments.Add("around-friends");
                    break;
                case ScoreSaberLeaderboardViewController.ScoreSaberScoresScope.Area:
                    PathSegments.Add($"around-{Plugin.Settings.locationFilterMode.ToLower()}");
                    break;
            }
            PathSegments.Add(leaderboardId);
            PathSegments.Add("mode");
            PathSegments.Add(gameMode);
            PathSegments.Add("difficulty");
            PathSegments.Add(difficulty);
            if (scope != ScoreSaberLeaderboardViewController.ScoreSaberScoresScope.Player) {
                QueryParams["page"] = page.ToString();
            }
            if (hideNA) {
                QueryParams["hideNA"] = "1";
            }
        }
    }
}