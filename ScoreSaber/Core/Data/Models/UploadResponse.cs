using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSaber.Core.Data.Models {
    internal class UploadResponse {
        [JsonProperty("Message")]
        public string Message { get; set; }
    }
}
