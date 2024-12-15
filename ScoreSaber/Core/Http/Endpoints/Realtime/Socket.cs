using ScoreSaber.Core.Http.Configuration;
using System.Net;
namespace ScoreSaber.Core.Http.Endpoints.Realtime {
    internal class SocketPath : Endpoint {
        public SocketPath()
            : base(ApiConfig.UrlBases.RealtimeWS) {
            PathSegments.Add("socket");
        }
    }
}