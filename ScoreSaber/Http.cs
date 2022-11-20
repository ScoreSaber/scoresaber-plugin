#region

using Newtonsoft.Json;
using ScoreSaber.Core.Data.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

#endregion

namespace ScoreSaber {
    internal struct HttpOptions {
        public string ApplicationName { get; set; }
        public Version Version { get; set; }
        public string BaseURL { get; set; }
    }

    internal sealed class Http {
        internal Dictionary<string, string> PersistentRequestHeaders { get; }
        internal HttpOptions Options { get; set; }

        internal Http(HttpOptions _options = new HttpOptions()) {

            Options = _options;
            PersistentRequestHeaders = new Dictionary<string, string>();

            if ((_options.ApplicationName != null && _options.Version == null) ||
                (_options.ApplicationName == null && _options.Version != null)) {
                throw new ArgumentException("You must specify either both or none of ApplicationName and Version");
            }

            string libVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string userAgent = $"Default/{libVersion}";

            if (_options.ApplicationName != null) {
                userAgent = $"{_options.ApplicationName}/{_options.Version}";
            }

            PersistentRequestHeaders.Add("User-Agent", userAgent);
        }

        internal async Task SendHttpAsyncRequest(UnityWebRequest request) {

            foreach (KeyValuePair<string, string> header in PersistentRequestHeaders) {
                request.SetRequestHeader(header.Key, header.Value);
            }

            AsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone) {
                await Task.Delay(100);
            }
        }

        internal async Task<string> GetRawAsync(string url) {

            using (var request = UnityWebRequest.Get(url)) {
                request.timeout = 5;
                await SendHttpAsyncRequest(request);
                if (request.isNetworkError || request.isHttpError) {
                    throw ThrowHttpException(request);
                }

                return Encoding.UTF8.GetString(request.downloadHandler.data);
            }
        }

        internal async Task<string> GetAsync(string url) {

            url = $"{Options.BaseURL}{url}";
            using (var request = UnityWebRequest.Get(url)) {
                request.timeout = 5;
                await SendHttpAsyncRequest(request);
                if (request.isNetworkError || request.isHttpError) {
                    throw ThrowHttpException(request);
                }

                return Encoding.UTF8.GetString(request.downloadHandler.data);
            }
        }

        internal async Task<byte[]> DownloadAsync(string url) {

            url = $"{Options.BaseURL}{url}";
            using (var request = UnityWebRequest.Get(url)) {
                await SendHttpAsyncRequest(request);
                if (request.isNetworkError || request.isHttpError) {
                    throw ThrowHttpException(request);
                }

                return request.downloadHandler.data;
            }
        }

        internal async Task<string> PostAsync(string url, WWWForm form) {

            url = $"{Options.BaseURL}{url}";
            using (var request = UnityWebRequest.Post(url, form)) {
                request.timeout = 120;
                await SendHttpAsyncRequest(request);
                if (request.isNetworkError || request.isHttpError) {
                    throw ThrowHttpException(request);
                }

                return Encoding.UTF8.GetString(request.downloadHandler.data);
            }
        }

        internal HttpErrorException ThrowHttpException(UnityWebRequest request) {

            if (request.downloadHandler.data != null) {
                return new HttpErrorException(request.isNetworkError, request.isHttpError, Encoding.UTF8.GetString(request.downloadHandler.data));
            }

            return new HttpErrorException(request.isNetworkError, request.isHttpError);
        }
    }

    internal class HttpErrorException : Exception {
        internal bool IsNetworkError { get; set; }
        internal bool IsHttpError { get; set; }
        internal bool IsScoreSaberError { get; set; }
        internal ScoreSaberError ScoreSaberError { get; set; }
        internal HttpErrorException(bool _isNetworkError, bool _isHttpError, string _scoreSaberErrorMessage = "") {
            IsNetworkError = _isNetworkError;
            IsHttpError = _isHttpError;
            if (_scoreSaberErrorMessage != string.Empty) {
                try {
                    ScoreSaberError = JsonConvert.DeserializeObject<ScoreSaberError>(_scoreSaberErrorMessage);
                    IsScoreSaberError = true;
                } catch (Exception) { }
            }
        }
    }
}