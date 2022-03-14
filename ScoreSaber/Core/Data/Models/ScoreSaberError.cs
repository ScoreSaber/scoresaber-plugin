using Newtonsoft.Json;

namespace ScoreSaber.Core.Data.Models {
     internal class ScoreSaberError {
        [JsonProperty("error")]
        internal Error error { get; set; }
    }

    internal class Error {
        [JsonProperty("message")]
        internal string message { get; set; }
    }
}
