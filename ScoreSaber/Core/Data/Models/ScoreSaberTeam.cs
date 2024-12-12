#pragma warning disable IDE1006 // Naming Styles
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ScoreSaber.Core.Data.Models {
    internal class ScoreSaberTeam {
        [JsonProperty("TeamMembers")]
        [JsonConverter(typeof(TeamMembersJsonConverter))]
        public Dictionary<TeamType, List<TeamMember>> TeamMembers { get; set; }
    }

    internal class TeamMember {
        [JsonProperty("Name")]
        internal string Name { get; set; }
        [JsonProperty("ProfilePicture")]
        internal string ProfilePicture { get; set; }
        [JsonProperty("Discord")]
        internal string Discord { get; set; }
        [JsonProperty("GitHub")]
        internal string GitHub { get; set; }
        [JsonProperty("Twitch")]
        internal string Twitch { get; set; }
        [JsonProperty("Twitter")]
        internal string Twitter { get; set; }
        [JsonProperty("YouTube")]
        internal string YouTube { get; set; }
    }

    internal class TeamType {
        private readonly string _value;

        private TeamType(string value) => _value = value;

        public static readonly TeamType Backend = new TeamType("Backend");
        public static readonly TeamType Frontend = new TeamType("Frontend");
        public static readonly TeamType Mod = new TeamType("Mod");
        public static readonly TeamType PPv3 = new TeamType("PPv3");
        public static readonly TeamType Admin = new TeamType("Admin");
        public static readonly TeamType RT = new TeamType("RT");
        public static readonly TeamType NAT = new TeamType("NAT");
        public static readonly TeamType QAT = new TeamType("QAT");
        public static readonly TeamType CAT = new TeamType("CAT");
        public static readonly TeamType CCT = new TeamType("CCT");


        // holds the known team types as of building the plugin, can handle unknown team types
        public static TeamType FromString(string value) => value switch {
            "Backend" => Backend,
            "Frontend" => Frontend,
            "Mod" => Mod,
            "PPv3" => PPv3,
            "Admin" => Admin,
            "RT" => RT,
            "NAT" => NAT,
            "QAT" => QAT,
            "CAT" => CAT,
            "CCT" => CCT,
            _ => new TeamType(value)
        };

        public override string ToString() => _value;

        public override bool Equals(object obj) => obj is TeamType other && _value == other._value;

        public override int GetHashCode() => _value.GetHashCode();

        public static implicit operator string(TeamType teamType) => teamType._value;
    }

    internal class TeamMembersJsonConverter : JsonConverter<Dictionary<TeamType, List<TeamMember>>> {
        public override Dictionary<TeamType, List<TeamMember>> ReadJson(JsonReader reader, Type objectType, Dictionary<TeamType, List<TeamMember>> existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var dictionary = new Dictionary<TeamType, List<TeamMember>>();

            var jsonObject = JObject.Load(reader);

            foreach (var property in jsonObject.Properties()) {
                var teamType = TeamType.FromString(property.Name);

                var teamMembers = property.Value.ToObject<List<TeamMember>>(serializer);

                dictionary[teamType] = teamMembers;
            }

            return dictionary;
        }

        public override void WriteJson(JsonWriter writer, Dictionary<TeamType, List<TeamMember>> value, JsonSerializer serializer) {
            writer.WriteStartObject();
            foreach (var kvp in value) {
                writer.WritePropertyName(kvp.Key.ToString());
                serializer.Serialize(writer, kvp.Value);
            }
            writer.WriteEndObject();
        }
    }

}
