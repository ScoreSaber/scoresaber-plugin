using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSaber.Core.Data.Models {
    public enum Scene {
        [JsonProperty("offline")]
        offline,
        [JsonProperty("online")]
        online,
        [JsonProperty("menu")]
        menu,
        [JsonProperty("playing")]
        playing
    }
}
