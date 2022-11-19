#region

using Newtonsoft.Json;

#endregion

namespace ScoreSaber.Core.Data.Models {
    internal class ScoreSaberError {
        [JsonProperty("errorMessage")] internal string errorMessage { get; set; }
    }
}