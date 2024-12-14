using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSaber.Core.Data.Models {
    public class RichPresenceResponse {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("state")]
        public State state { get; set; }
    }
}
