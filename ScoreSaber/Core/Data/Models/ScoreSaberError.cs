using Newtonsoft.Json;

namespace ScoreSaber.Core.Data.Models {
    internal class ScoreSaberError {
        [JsonProperty("errorMessage")]
        internal string ErrorMessage { get; set; }
    }
}