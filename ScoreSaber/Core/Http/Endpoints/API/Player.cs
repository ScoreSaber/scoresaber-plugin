#nullable enable
using ScoreSaber.Core.Http.Configuration;
using System.Net;
using UnityEngine;
namespace ScoreSaber.Core.Http.Endpoints.API.Player {
    internal class AuthenticateRequest : Endpoint {
        public AuthenticateRequest(string playerId,
                                   string authType,
                                   string nonce,
                                   string friends,
                                   string name) : base(ApiConfig.UrlBases.APIv1) {

            PathSegments.Add("auth");
            Form = new WWWForm();
            Form.AddField("at", authType);
            Form.AddField("playerId", playerId);
            Form.AddField("nonce", nonce);
            Form.AddField("friends", friends);
            Form.AddField("name", name);
        }
        public WWWForm Form { get; }
    }
    internal class ProfileRequest : Endpoint {
        public ProfileRequest(string playerId, bool full = false)
            : base(ApiConfig.UrlBases.APIv1Public) {
            PathSegments.Add("player");
            PathSegments.Add(playerId);
            PathSegments.Add(full ? "full" : "basic");
        }
    }
    internal class ReplayRequest : Endpoint {
        public ReplayRequest(string playerId, string leaderboardId)
            : base(ApiConfig.UrlBases.APIv1) {
            PathSegments.Add("telemetry");
            PathSegments.Add("downloadReplay");
            QueryParams["playerId"] = playerId;
            QueryParams["leaderboardId"] = leaderboardId;
        }
    }
}