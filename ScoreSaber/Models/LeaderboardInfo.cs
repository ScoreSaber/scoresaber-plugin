using Newtonsoft.Json;
using System;

namespace ScoreSaber.Models;

internal sealed class LeaderboardInfo
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("songHash")]
    public string SongHash { get; set; } = string.Empty;

    [JsonProperty("songName")]
    public string SongName { get; set; } = string.Empty;

    [JsonProperty("songSubName")]
    public string SongSubName { get; set; } = string.Empty;

    [JsonProperty("songAuthorName")]
    public string SongAuthorName { get; set; } = string.Empty;

    [JsonProperty("levelAuthorName")]
    public string LevelAuthorName { get; set; } = string.Empty;

    [JsonProperty("difficulty")]
    public LeaderboardDifficulty Difficulty { get; set; } = null!;

    [JsonProperty("maxScore")]
    public int MaxScore { get; set; }

    [JsonProperty("createdDate")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonProperty("rankedDate")]
    public DateTimeOffset? RankedAt { get; set; }

    [JsonProperty("qualifiedDate")]
    public DateTimeOffset? QualifiedAt { get; set; }

    [JsonProperty("lovedDate")]
    public DateTimeOffset? LovedAt { get; set; }

    [JsonProperty("ranked")]
    public bool Ranked { get; set; }

    [JsonProperty("qualified")]
    public bool Qualified { get; set; }

    [JsonProperty("loved")]
    public bool Loved { get; set; }

    [JsonProperty("maxPP")]
    public double MaxPP { get; set; }

    [JsonProperty("stars")]
    public double Stars { get; set; }

    [JsonProperty("plays")]
    public int Plays { get; set; }

    [JsonProperty("dailyPlays")]
    public int DailyPlays { get; set; }

    [JsonProperty("coverImage")]
    public string CoverImage { get; set; } = string.Empty;

    [JsonProperty("positiveModifiers")]
    public bool PositiveModifiers { get; set; }

    [JsonProperty("playerScore")]
    public LeaderboardScore? PlayerScore { get; set; }
}
