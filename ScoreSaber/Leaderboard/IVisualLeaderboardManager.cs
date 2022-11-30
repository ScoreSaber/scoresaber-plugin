using ScoreSaber.Models;
using System;

namespace ScoreSaber.Leaderboard;

internal interface IVisualLeaderboardManager
{
    event Action<Models.Leaderboard?>? OnLeaderboardChanged;

    void ChangeScope(LeaderboardScope scope);

    void ChangePage(int page);
}