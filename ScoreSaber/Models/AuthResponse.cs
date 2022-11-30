using Newtonsoft.Json;

namespace ScoreSaber.Models;

internal class AuthResponse
{
    [JsonProperty("playerKey")]
    public string PlayerKey { get; set; } = string.Empty;

    [JsonProperty("serverKey")]
    public string ServerKey { get; set; } = string.Empty;
}