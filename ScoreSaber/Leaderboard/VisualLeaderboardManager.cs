using System;
using LeaderboardCore.Interfaces;
using ScoreSaber.Models;
using System.Threading;
using System.Threading.Tasks;
using SiraUtil.Web;
using SiraUtil.Logging;
using Newtonsoft.Json;
using ScoreSaber.Networking;
using IPA.Utilities.Async;

namespace ScoreSaber.Leaderboard;

internal sealed class VisualLeaderboardManager : IVisualLeaderboardManager, INotifyLeaderboardSet, IDisposable
{
    private int _page = 0;
    private LeaderboardScope _scope;
    private CancellationTokenSource? _cts;

    private readonly Http _http;
    private readonly SiraLog _siraLog;

    public event Action<Models.Leaderboard?>? OnLeaderboardChanged;

    public VisualLeaderboardManager(Http http, SiraLog siraLog)
    {
        _http = http;
        _siraLog = siraLog;
    }

    public void ChangePage(int page)
    {
        // TODO: Convert to property?
        _page = page;
    }

    public void ChangeScope(LeaderboardScope scope)
    {
        _scope = scope;
    }

    public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
    {
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        UnityMainThreadTaskScheduler.Factory.StartNew(async () =>
        {
            try
            {
                var url = GetLeaderboardUrl(difficultyBeatmap, _scope, _page);

                var leaderboard = await _http.GetAsJsonAsync<Models.Leaderboard>(url);

                // do the leaderboard fetch
                OnLeaderboardChanged?.Invoke(leaderboard);

                _siraLog.Notice($"Loaded Leaderboard {leaderboard.Info.SongName} ({leaderboard.Info.Id})");
            }
            catch (Exception ex)
            {
                _siraLog.Error(ex);
                _siraLog.Error("Request failed");
                // do the exception handling stuff
            }
        }, _cts.Token);
    }

    public void Dispose()
    {
        _cts?.Dispose();
    }

    private static string GetLeaderboardUrl(IDifficultyBeatmap difficultyBeatmap, LeaderboardScope scope, int page)
    {
        string url = "/game/leaderboard";
        string leaderboardId = difficultyBeatmap.level.levelID.Split('_')[2];
        string gameMode = $"Solo{difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}";
        string difficulty = BeatmapDifficultyMethods.DefaultRating(difficultyBeatmap.difficulty).ToString();

        var specificEndpoint = scope switch
        {
            LeaderboardScope.Global => string.Empty,
            LeaderboardScope.Player => "/around-player",
            LeaderboardScope.Friends => "/around-friends",
            LeaderboardScope.Country => "/around-country",
            _ => throw new NotImplementedException(),
        };

        url += $"{specificEndpoint}/{leaderboardId}/mode/{gameMode}/difficulty/{difficulty}?page={page}";

        // TODO: Reimplement
        /*if (Plugin.Settings.hideNAScoresFromLeaderboard)
        {
            url = $"{url}&hideNA=1";
        }*/

        return url;
    }
}
