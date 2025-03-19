#nullable enable
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace ScoreSaber.Core.Http {
    internal class HttpClient {
        private readonly HttpClientOptions options;
        public HttpClient(HttpClientOptions? options = null) {
            this.options = options ?? new HttpClientOptions();
        }
        private async Task<UnityWebRequestAsyncOperation> SendRequestAsync(UnityWebRequest request) {
            foreach (var header in options.Headers) {
                request.SetRequestHeader(header.Key, header.Value);
            }
            var operation = request.SendWebRequest();
            while (!operation.isDone) {
                await Task.Delay(100);
            }
            if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError) {
                var errorMessage = request.downloadHandler.data != null
                    ? Encoding.UTF8.GetString(request.downloadHandler.data)
                    : request.error;
                throw new HttpRequestException(
                    errorMessage,
                    request.responseCode,
                    isNetworkError: request.result == UnityWebRequest.Result.ConnectionError,
                    isHttpError: request.result == UnityWebRequest.Result.ProtocolError
                );
            }
            return operation;
        }
        public async Task<string> GetAsync(string url) {
            using var request = UnityWebRequest.Get(url);
            request.timeout = options.DefaultTimeout;
            await SendRequestAsync(request);
            string toReturn = Encoding.UTF8.GetString(request.downloadHandler.data);
            request.Dispose();
            return toReturn;
        }
        public async Task<string> PostAsync(string url, WWWForm form) {
            using var request = UnityWebRequest.Post(url, form);
            request.timeout = options.UploadTimeout;
            await SendRequestAsync(request);
            string toReturn = Encoding.UTF8.GetString(request.downloadHandler.data);
            request.Dispose();
            return toReturn;
        }
        public async Task<byte[]> DownloadAsync(string url) {
            using var request = UnityWebRequest.Get(url);
            request.timeout = options.DefaultTimeout;
            await SendRequestAsync(request);
            byte[] toReturn = request.downloadHandler.data;
            request.Dispose();
            return toReturn;
        }
        public void SetHeader(string key, string value) {
            options.Headers[key] = value;
        }
    }
}