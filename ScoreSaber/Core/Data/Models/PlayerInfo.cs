#region

using Newtonsoft.Json;

#endregion

namespace ScoreSaber.Core.Data.Models {
    internal class Metadata {
        [JsonProperty("total")] internal int total { get; set; }

        [JsonProperty("page")] internal int page { get; set; }

        [JsonProperty("itemsPerPage")] internal int itemsPerPage { get; set; }
    }

    internal class PlayerCollection {
        [JsonProperty("players")] internal PlayerInfo[] players { get; set; }

        [JsonProperty("metadata")] internal Metadata metdata { get; set; }
    }

    internal class PlayerInfo {
        [JsonProperty("id")] internal string id { get; set; }

        [JsonProperty("name")] internal string name { get; set; }

        [JsonProperty("profilePicture")] internal string profilePicture { get; set; }

        [JsonProperty("country")] internal string country { get; set; }

        [JsonProperty("pp")] internal double pp { get; set; }

        [JsonProperty("rank")] internal int rank { get; set; }

        [JsonProperty("countryRank")] internal int countryRank { get; set; }

        [JsonProperty("role")] internal string role { get; set; }

        [JsonProperty("badges")] internal Badge[] badges { get; set; }

        [JsonProperty("histories")] internal string histories { get; set; }

        [JsonProperty("permissions")] internal int permissions { get; set; }

        [JsonProperty("banned")] internal bool banned { get; set; }

        [JsonProperty("inactive")] internal bool inactive { get; set; }

        [JsonProperty("scoreStats")] internal ScoreStats scoreStats { get; set; }
    }

    internal class Badge {
        [JsonProperty("description")] internal string description { get; set; }

        [JsonProperty("image")] internal string image { get; set; }
    }

    internal class ScoreStats {
        [JsonProperty("totalScore")] internal long totalScore { get; set; }

        [JsonProperty("totalRankedScore")] internal long totalRankedScore { get; set; }

        [JsonProperty("averageRankedAccuracy")]
        internal double averageRankedAccuracy { get; set; }

        [JsonProperty("totalPlayCount")] internal int totalPlayCount { get; set; }

        [JsonProperty("rankedPlayCount")] internal int rankedPlayCount { get; set; }
    }
}