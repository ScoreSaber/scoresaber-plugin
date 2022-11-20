#region

using Newtonsoft.Json;

#endregion

namespace ScoreSaber.Core.Data.Models {

    internal class Metadata {
        [JsonProperty("total")]
        internal int Total { get; set; }
        [JsonProperty("page")]
        internal int Page { get; set; }
        [JsonProperty("itemsPerPage")]
        internal int ItemsPerPage { get; set; }
    }

    internal class PlayerCollection {
        [JsonProperty("players")]
        internal PlayerInfo[] Players { get; set; }
        [JsonProperty("metadata")]
        internal Metadata Metadata { get; set; }
    }

    internal class PlayerInfo {
        [JsonProperty("id")]
        internal string Id { get; set; }
        [JsonProperty("name")]
        internal string Name { get; set; }
        [JsonProperty("profilePicture")]
        internal string ProfilePicture { get; set; }
        [JsonProperty("country")]
        internal string Country { get; set; }
        [JsonProperty("pp")]
        internal double PP { get; set; }
        [JsonProperty("rank")]
        internal int Rank { get; set; }
        [JsonProperty("countryRank")]
        internal int CountryRank { get; set; }
        [JsonProperty("role")]
        internal string Role { get; set; }
        [JsonProperty("badges")]
        internal Badge[] Badges { get; set; }
        [JsonProperty("histories")]
        internal string Histories { get; set; }
        [JsonProperty("permissions")]
        internal int Permissions { get; set; }
        [JsonProperty("banned")]
        internal bool Banned { get; set; }
        [JsonProperty("inactive")]
        internal bool Inactive { get; set; }
        [JsonProperty("scoreStats")]
        internal ScoreStats ScoreStats { get; set; }
    }

    internal class Badge {
        [JsonProperty("description")]
        internal string Description { get; set; }
        [JsonProperty("image")]
        internal string Image { get; set; }
    }

    internal class ScoreStats {
        [JsonProperty("totalScore")]
        internal long TotalScore { get; set; }
        [JsonProperty("totalRankedScore")]
        internal long TotalRankedScore { get; set; }
        [JsonProperty("averageRankedAccuracy")]
        internal double AverageRankedAccuracy { get; set; }
        [JsonProperty("totalPlayCount")]
        internal int TotalPlayCount { get; set; }
        [JsonProperty("rankedPlayCount")]
        internal int RankedPlayCount { get; set; }
    }
}