#nullable enable
using ScoreSaber.Core.Http.Endpoints;

namespace ScoreSaber.Core.Http.Configuration {
    internal static class ApiConfig {
        private const string Domain = "scoresaber.com";
        internal static class Protocols {
            internal const string Https = "https://";
            internal const string WebSocket = "wss://";
        }
        internal static class Subdomains {
            internal const string CDN = "cdn";
            internal const string Realtime = "realtime";
        }
        internal static class UrlBases {
            internal static readonly UrlBase APIv1 = new(Protocols.Https, string.Empty, Domain + "/api/game");
            internal static readonly UrlBase APIv1Public = new(Protocols.Https, string.Empty, Domain + "/api");
            internal static readonly UrlBase Web = new(Protocols.Https, string.Empty, Domain);
            internal static readonly UrlBase CDN = new(Protocols.Https, Subdomains.CDN, Domain);
            internal static readonly UrlBase Realtime = new(Protocols.Https, Subdomains.Realtime, Domain);
            internal static readonly UrlBase RealtimeWS = new(Protocols.WebSocket, Subdomains.Realtime, Domain);
        }
    }
}