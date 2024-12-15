#nullable enable
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScoreSaber.Core.Http.Endpoints;
using UnityEngine;
namespace ScoreSaber.Core.Http {
    internal class ScoreSaberHttpClient {
        private readonly HttpClient client;
        public ScoreSaberHttpClient(HttpClientOptions? options = null) {
            client = new HttpClient(options);
        }
        public void SetCookie(string cookie) {
            client.SetHeader("Cookie", cookie);
        }
        public async Task<T> GetAsync<T>(Endpoint endpoint) {
            var response = await client.GetAsync(endpoint.BuildUrl());
            return JsonConvert.DeserializeObject<T>(response)!;
        }
        public async Task<T> PostAsync<T>(Endpoint endpoint, WWWForm form) {
            var response = await client.PostAsync(endpoint.BuildUrl(), form);
            return JsonConvert.DeserializeObject<T>(response)!;
        }
        public async Task<byte[]> DownloadAsync(Endpoint endpoint) {
            return await client.DownloadAsync(endpoint.BuildUrl());
        }

        public async Task<string> GetRawAsync(string url) {
            return await client.GetAsync(url);
        }

        public async Task<string> PostRawAsync(string url, WWWForm form) {
            return await client.PostAsync(url, form);
        }
    }
}