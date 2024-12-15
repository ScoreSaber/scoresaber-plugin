using ScoreSaber.Core.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ScoreSaber.Core.Http.Endpoints.API {
    internal class UploadRequest : Endpoint {
        public UploadRequest() : base(ApiConfig.UrlBases.APIv1) {
            PathSegments.Add("Upload");

        }
    }
}
