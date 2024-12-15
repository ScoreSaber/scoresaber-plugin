using ScoreSaber.Core.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSaber.Core.Http.Endpoints.CDN {
    internal class SongCover : Endpoint {
        public SongCover(string hash)
            : base(ApiConfig.UrlBases.CDN) {
            PathSegments.Add("covers");
            PathSegments.Add($"{hash}.png");
        }
    }
}
