using ScoreSaber.Core.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSaber.Core.Http.Endpoints.Web {
    internal class WebLeaderboard : Endpoint {
        public WebLeaderboard(string leaderboardId) : base(ApiConfig.UrlBases.Web) {
            PathSegments.Add("leaderboard");
            PathSegments.Add(leaderboardId);
        }
    }
}
