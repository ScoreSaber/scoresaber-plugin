using Newtonsoft.Json;

namespace ScoreSaber.Models;

internal sealed class LeaderboardDifficulty
{
    [JsonProperty("leaderboardId")]
    public int LeaderboardId { get; set; }

    [JsonProperty("difficulty")]
    public int Value { get; set; }

    [JsonProperty("gameMode")]
    public string GameMode { get; set; } = string.Empty;

    [JsonProperty("difficultyRaw")]
    public string DifficultyRaw { get; set; } = string.Empty;
}