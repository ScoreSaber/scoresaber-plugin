using ScoreSaber.Core.Http.Configuration;
using System.Net;
namespace ScoreSaber.Core.Http.Endpoints.CDN {
    internal class Flags : Endpoint {
        public Flags(string country)
            : base(ApiConfig.UrlBases.CDN) {
            PathSegments.Add("flags");
            PathSegments.Add($"{country.ToLower()}.png");
        }
    }
}