using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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
                    QueryParams.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
                url += $"?{queryString}";
            }

            return url;
        }
    }
}