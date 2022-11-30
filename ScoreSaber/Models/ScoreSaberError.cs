using Newtonsoft.Json;

namespace ScoreSaber.Models;

internal sealed class ScoreSaberError
{
    [JsonProperty("errorMessage")]
    public string Message { get; set; } = string.Empty;
}