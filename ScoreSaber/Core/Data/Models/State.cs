using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSaber.Core.Data.Models {
    public class State {
        [JsonProperty("scene")]
        public Scene Scene { get; set; } = Scene.menu;

        [JsonProperty("currentMap")]
        public SongRichPresenceInfo currentMap { get; set; }
    }
}
