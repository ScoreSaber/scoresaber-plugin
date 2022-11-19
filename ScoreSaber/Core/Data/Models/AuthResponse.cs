using Newtonsoft.Json;

namespace ScoreSaber.Core.Data.Models {

    internal class AuthResponse {
        [JsonProperty("a")]
        internal string PlayerKey { get; set; }
        [JsonProperty("e")]
        internal string ServerKey { get; set; }
    }
}