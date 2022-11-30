using Newtonsoft.Json;
using System;

namespace ScoreSaber.Models;

internal sealed class Leaderboard
{
    [JsonProperty("leaderboardInfo")]
    public LeaderboardInfo Info { get; set; } = null!;

    [JsonProperty("scores")]
    public LeaderboardScore[] Scores { get; set; } = Array.Empty<LeaderboardScore>();
}