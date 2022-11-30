using Newtonsoft.Json;
using System;

namespace ScoreSaber.Models;

internal sealed class LeaderboardScore
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("leaderboardPlayerInfo")]
    public LeaderboardPlayer PlayerInfo { get; set; } = null!;

    [JsonProperty("rank")]
    public int Rank { get; set; }

    [JsonProperty("baseScore")]
    public int BaseScore { get; set; }

    [JsonProperty("modifiedScore")]
    public int ModifiedScore { get; set; }

    [JsonProperty("pp")]
    public double PP { get; set; }

    [JsonProperty("weight")]
    public double Weight { get; set; }

    [JsonProperty("modifiers")]
    public string Modifiers { get; set; } = string.Empty;

    [JsonProperty("multiplier")]
    public double Multiplier { get; set; }

    [JsonProperty("badCuts")]
    public int BadCuts { get; set; }

    [JsonProperty("missedNotes")]
    public int MissedNotes { get; set; }

    [JsonProperty("maxCombo")]
    public int MaxCombo { get; set; }

    [JsonProperty("fullCombo")]
    public bool FullCombo { get; set; }

    [JsonProperty("hmd")]
    public int HMD { get; set; }

    [JsonProperty("timeSet")]
    public DateTime TimeSet { get; set; }

    [JsonProperty("hasReplay")]
    public bool HasReplay { get; set; }
}