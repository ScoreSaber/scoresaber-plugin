
namespace ScoreSaber.Core.Http.Endpoints {
    internal record UrlBase(string Protocol, string Subdomain, string Domain) {
        private string SubdomainPrefix => string.IsNullOrEmpty(Subdomain) ? "" : $"{Subdomain}.";
        public string BaseUrl => $"{Protocol}{SubdomainPrefix}{Domain}";
        public string BuildUrl(params string[] segments) {
            var path = string.Join("/", segments);
            return $"{BaseUrl}/{path}";
        }

        public static string operator +(UrlBase urlBase, string path) {
            return urlBase.BuildUrl(path);
        }
    }
}
