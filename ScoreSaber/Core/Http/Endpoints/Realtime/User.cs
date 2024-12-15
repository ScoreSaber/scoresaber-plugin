using ScoreSaber.Core.Http.Configuration;
using System.Net;
namespace ScoreSaber.Core.Http.Endpoints.Realtime {
    internal class UserRequest : Endpoint {
        public UserRequest(string playerId)
            : base(ApiConfig.UrlBases.Realtime) {
            PathSegments.Add("user");
            PathSegments.Add(playerId);
        }
    }
}