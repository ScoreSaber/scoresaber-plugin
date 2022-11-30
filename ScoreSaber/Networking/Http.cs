using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using ScoreSaber.Models;
using Version = Hive.Versioning.Version;

namespace ScoreSaber.Networking;

/// <summary>
/// HTTP Options
/// </summary>
internal struct HttpOptions
{
    /// <summary>
    /// Application Name
    /// </summary>
    public string ApplicationName { get; set; }

    /// <summary>
    /// Application Version
    /// </summary>
    public Version Version { get; set; }

    /// <summary>
    /// The BaseURL
    /// </summary>
    public string BaseURL { get; set; }

}

internal sealed class Http
{
    private readonly HttpOptions _options;
    public Dictionary<string, string> PersistentRequestHeaders { get; private set; }

    public Http(HttpOptions options)
    {
        _options = options;
        PersistentRequestHeaders = new Dictionary<string, string>();

        if ((_options.ApplicationName != null && _options.Version == null) || (_options.ApplicationName == null && _options.Version != null))
        {
            throw new ArgumentException("You must specify either both or none of ApplicationName and Version");
        }

        string libVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        string userAgent = $"Default/{libVersion}";

        if (_options.ApplicationName != null)
        {
            userAgent = $"{_options.ApplicationName}/{_options.Version}";
        }

        PersistentRequestHeaders.Add("User-Agent", userAgent);
    }

    public async Task SendHttpAsyncRequest(UnityWebRequest request)
    {
        foreach (var header in PersistentRequestHeaders)
        {
            request.SetRequestHeader(header.Key, header.Value);
        }

        AsyncOperation asyncOperation = request.SendWebRequest();
        while (!asyncOperation.isDone)
        {
            await Task.Delay(100);
        }
    }

    public async Task<string> GetRawAsync(string url)
    {
        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 5;
        await SendHttpAsyncRequest(request);
        if (request.isNetworkError || request.isHttpError)
        {
            throw ThrowHttpException(request);
        }
        else
        {
            return Encoding.UTF8.GetString(request.downloadHandler.data);
        }
    }

    public async Task<string> GetAsync(string url)
    {
        url = $"{_options.BaseURL}{url}";
        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 5;
        await SendHttpAsyncRequest(request);
        if (request.isNetworkError || request.isHttpError)
        {
            throw ThrowHttpException(request);
        }
        else
        {
            return Encoding.UTF8.GetString(request.downloadHandler.data);
        }
    }

    public async Task<T> GetAsJsonAsync<T>(string url)
    {
        var response = await GetAsync(url);
        return JsonConvert.DeserializeObject<T>(response);
    }

    public async Task<byte[]> DownloadAsync(string url)
    {
        url = $"{_options.BaseURL}{url}";
        using UnityWebRequest request = UnityWebRequest.Get(url);
        await SendHttpAsyncRequest(request);
        if (request.isNetworkError || request.isHttpError)
        {
            throw ThrowHttpException(request);
        }
        else
        {
            return request.downloadHandler.data;
        }
    }

    public async Task<string> PostAsync(string url, WWWForm form)
    {
        url = $"{_options.BaseURL}{url}";
        using UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.timeout = 120;
        await SendHttpAsyncRequest(request);
        if (request.isNetworkError || request.isHttpError)
        {
            throw ThrowHttpException(request);
        }
        else
        {
            return Encoding.UTF8.GetString(request.downloadHandler.data);
        }
    }

    public async Task<T> PostIntoJsonAsync<T>(string url, WWWForm form)
    {
        var response = await PostAsync(url, form);
        return JsonConvert.DeserializeObject<T>(response);
    }

    public HttpErrorException ThrowHttpException(UnityWebRequest request)
    {
        if (request.downloadHandler.data != null)
        {
            return new HttpErrorException(request.isNetworkError, request.isHttpError, Encoding.UTF8.GetString(request.downloadHandler.data)); // Epic
        }
        else
        {
            return new HttpErrorException(request.isNetworkError, request.isHttpError); // Epic
        }
    }
}

internal class HttpErrorException : Exception
{
    public bool IsNetworkError { get; set; }
    public bool IsHttpError { get; set; }
    public bool IsScoreSaberError { get; set; }
    public ScoreSaberError? Error { get; set; }

    public HttpErrorException(bool isNetworkError, bool isHttpError, string errorMessage = "")
    {
        IsNetworkError = isNetworkError;
        IsHttpError = isHttpError;
        if (errorMessage != string.Empty)
        {
            try
            {
                Error = JsonConvert.DeserializeObject<ScoreSaberError>(errorMessage);
                IsScoreSaberError = true;
            }
            catch (Exception) { }
        }
    }
}