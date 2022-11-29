using HMUI;
using LeaderboardCore.Models;
using ScoreSaber.UI.Menu;

namespace ScoreSaber.Leaderboard;

internal sealed class ScoreSaberLeaderboard : CustomLeaderboard
{
    private readonly PanelViewController _panelViewController;
    private readonly ScoreViewController _scoreViewController;

    public ScoreSaberLeaderboard(PanelViewController panelViewController, ScoreViewController scoreViewController)
    {
        _panelViewController = panelViewController;
        _scoreViewController = scoreViewController;
    }

    protected override ViewController panelViewController => _panelViewController;

    protected override ViewController leaderboardViewController => _scoreViewController;
}