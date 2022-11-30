using Newtonsoft.Json;

namespace ScoreSaber.Models;

internal sealed class LeaderboardPlayer
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("profilePicture")]
    public string ProfilePicture { get; set; } = string.Empty;

    [JsonProperty("country")]
    public string Country { get; set; } = string.Empty;

    [JsonProperty("permissions")]
    public int Permissions { get; set; }

    [JsonProperty("role")]
    public string Role { get; set; } = string.Empty;
}