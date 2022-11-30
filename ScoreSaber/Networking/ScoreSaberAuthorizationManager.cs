using ScoreSaber.Models;
using ScoreSaber.Services;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ScoreSaber.Networking;

internal class ScoreSaberAuthorizationManager : IAsyncInitializable
{
    private readonly Http _http;
    private readonly SiraLog _siraLog;
    private readonly IPlatformLoginService _platformLoginService;

    public ScoreSaberAuthorizationManager(Http http, SiraLog siraLog, IPlatformLoginService platformLoginService)
    {
        _http = http;
        _siraLog = siraLog;
        _platformLoginService = platformLoginService;
    }

    public async Task InitializeAsync(CancellationToken token)
    {
        _siraLog.Info("Logging into ScoreSaber...");
        var userInfo = await _platformLoginService.LoginAsync();
        
        if (userInfo is null)
        {
            _siraLog.Error("Could not acquire local platform user info");
            return;
        }

        if (_http.PersistentRequestHeaders.ContainsKey("Cookies"))
            _http.PersistentRequestHeaders.Remove("Cookies");

        WWWForm form = new();
        form.AddField("at", userInfo.AuthType);
        form.AddField("playerId", userInfo.Id);
        form.AddField("nonce", userInfo.Nonce);
        form.AddField("friends", userInfo.Friends);
        form.AddField("name", userInfo.Name);

        var response = await _http.PostIntoJsonAsync<AuthResponse>("/game/auth", form);
        _http.PersistentRequestHeaders.Add("Cookies", $"connect.sid={response.ServerKey}");
    }
}
