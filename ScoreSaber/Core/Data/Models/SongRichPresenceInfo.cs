using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSaber.Core.Data.Models {
    public class SongRichPresenceInfo {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonProperty("mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public GameMode Mode { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("subName")]
        public string SubName { get; set; } = string.Empty;

        [JsonProperty("authorName")]
        public string AuthorName { get; set; } = string.Empty;

        [JsonProperty("artist")]
        public string Artist { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty; // Standard, Lawless, OneSaber etc

        [JsonProperty("hash")]
        public string Hash { get; set; } = string.Empty;

        [JsonProperty("duration")]
        public int Duration { get; set; } // Song duration in seconds

        [JsonProperty("difficulty")]
        public int Difficulty { get; set; } = -1; // Difficulty, 0-9, odd numba

        [JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
        public int? StartTime { get; set; } // Start time if in practice mode

        [JsonProperty("playSpeed", NullValueHandling = NullValueHandling.Ignore)]
        public double? PlaySpeed { get; set; } // Playback speed, from either practice mode or speed modifies


        public SongRichPresenceInfo(string timestamp, GameMode mode, string name, string subName, string authorName, string artist, string type, string hash, int duration, int difficulty, int? startTime, double? playSpeed) {
            Timestamp = timestamp;
            Mode = mode;
            Name = name;
            SubName = subName;
            AuthorName = authorName;
            Artist = artist;
            Type = type;
            Hash = hash;
            Duration = duration;
            Difficulty = difficulty;
            StartTime = startTime;
            PlaySpeed = playSpeed;
        }
    }
}
