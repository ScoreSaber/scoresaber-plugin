#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
namespace ScoreSaber.Core.Http {
    internal class HttpClientOptions {
        public Dictionary<string, string> Headers { get; }
        public int DefaultTimeout { get; init; }
        public int UploadTimeout { get; init; }
        public HttpClientOptions(
            string? applicationName = null,
            Version? version = null,
            int defaultTimeout = 5,
            int uploadTimeout = 120) {
            DefaultTimeout = defaultTimeout;
            UploadTimeout = uploadTimeout;
            Headers = new Dictionary<string, string>();
            if ((applicationName != null && version == null) ||
                (applicationName == null && version != null)) {
                throw new ArgumentException("You must specify either both or none of ApplicationName and Version");
            }
            var userAgent = applicationName != null
                ? $"{applicationName}/{version}"
                : $"Default/{Assembly.GetExecutingAssembly().GetName().Version}";
            Headers.Add("User-Agent", userAgent);
        }
    }
}