using LeaderboardCore.Managers;
using SiraUtil.Logging;
using System;
using Zenject;

namespace ScoreSaber.Leaderboard;

internal sealed class ScoreSaberLeaderboardManager : IInitializable, IDisposable
{
    private readonly SiraLog _siraLog;
    private readonly ScoreSaberLeaderboard _scoreSaberLeaderboard;
    private readonly CustomLeaderboardManager _customLeaderboardManager;

    public ScoreSaberLeaderboardManager(SiraLog siraLog, ScoreSaberLeaderboard scoreSaberLeaderboard, CustomLeaderboardManager customLeaderboardManager)
    {
        _siraLog = siraLog;
        _scoreSaberLeaderboard = scoreSaberLeaderboard;
        _customLeaderboardManager = customLeaderboardManager;
    }

    public void Initialize()
    {
        _siraLog.Debug("Registering ScoreSaber leaderboard into LeaderboardCore");
        _customLeaderboardManager.Register(_scoreSaberLeaderboard);
    }

    public void Dispose()
    {
        _siraLog.Debug("Unregistering ScoreSaber leaderboard from LeaderboardCore");
        _customLeaderboardManager.Unregister(_scoreSaberLeaderboard);
    }
}