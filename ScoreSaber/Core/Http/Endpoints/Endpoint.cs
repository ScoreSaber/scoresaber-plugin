using System.Collections.Generic;
using System.Linq;
using ScoreSaber.Core.Http.Configuration;
using static ScoreSaber.Core.Http.Configuration.ApiConfig;
namespace ScoreSaber.Core.Http.Endpoints {
    internal abstract class Endpoint {
        protected readonly UrlBase UrlBase;
        protected readonly List<string> PathSegments;
        protected readonly Dictionary<string, string> QueryParams;
        protected Endpoint(UrlBase urlBase) {
            UrlBase = urlBase;
            PathSegments = new List<string>();
            QueryParams = new Dictionary<string, string>();
        }
        public string BuildUrl() {
            var url = UrlBase.BuildUrl(PathSegments.ToArray());
            if (QueryParams.Any()) {
                var queryString = string.Join("&",
                    QueryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                url += $"?{queryString}";
            }
            return url;
        }
    }
}