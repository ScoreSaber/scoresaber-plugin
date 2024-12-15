using ScoreSaber.Core.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSaber.Core.Http.Endpoints.Web {
    internal class WebUser : Endpoint {
        public WebUser(string playerId) : base(ApiConfig.UrlBases.Web) {
            PathSegments.Add("u");
            PathSegments.Add(playerId);
        }
    }
}
