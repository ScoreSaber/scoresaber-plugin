#nullable enable
using System;
using Newtonsoft.Json;
using ScoreSaber.Core.Data.Models;
namespace ScoreSaber.Core.Http {
    internal class HttpRequestException : Exception {
        public bool IsNetworkError { get; }
        public bool IsHttpError { get; }
        public bool IsScoreSaberError { get; }
        public ScoreSaberError? ScoreSaberError { get; }
        public long StatusCode { get; }
        public HttpRequestException(
            string message,
            long statusCode,
            bool isNetworkError = false,
            bool isHttpError = false)
            : base(message) {
            StatusCode = statusCode;
            IsNetworkError = isNetworkError;
            IsHttpError = isHttpError;
            if (!string.IsNullOrEmpty(message)) {
                try {
                    ScoreSaberError = JsonConvert.DeserializeObject<ScoreSaberError>(message);
                    IsScoreSaberError = true;
                } catch (Exception) {
                    IsScoreSaberError = false;
                    ScoreSaberError = null;
                }
            }
        }
    }
}