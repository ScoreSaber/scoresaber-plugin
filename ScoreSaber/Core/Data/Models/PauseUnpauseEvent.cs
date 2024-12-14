using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSaber.Core.Data.Models {
    public class PauseUnpauseEvent {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonProperty("songTime")]
        public double SongTime { get; set; } // Time since start of song in seconds

        [JsonProperty("eventType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PauseType EventType { get; set; }
    }
}
