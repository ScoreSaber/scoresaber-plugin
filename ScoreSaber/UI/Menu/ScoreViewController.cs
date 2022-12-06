using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using ScoreSaber.Leaderboard;
using ScoreSaber.UI.Menu.Hosts;
using SiraUtil.Logging;
using Zenject;

namespace ScoreSaber.UI.Menu;

[HotReload(RelativePathToLayout = @"Views\ScoreView.bsml")]
[ViewDefinition("ScoreSaber.UI.Menu.Views.ScoreView.bsml")]
internal class ScoreViewController : BSMLAutomaticViewController
{
    [Inject]
    [UIValue("ScoreScopeHost")]
    protected readonly ScoreScopeHost _scoreScopeHost = null!;

    [Inject]
    [UIValue("ScoreTableHost")]
    protected readonly ScoreTableHost _scoreTableHost = null!;

    [Inject]
    private readonly IVisualLeaderboardManager _visualLeaderboardManager = null!;

    [Inject]
    private readonly SiraLog _siraLog = null!;

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        _visualLeaderboardManager.OnLeaderboardChanged += VisualLeaderboardManager_OnLeaderboardChanged;
        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
    }

    private void VisualLeaderboardManager_OnLeaderboardChanged(ScoreSaber.Models.Leaderboard? leaderboard)
    {
        if (leaderboard is null)
        {
            _scoreTableHost.SetLoadingIndicator(true);
            return;
        }

        foreach (var score in leaderboard.Scores)
            _siraLog.Info(score.PlayerInfo.Name);

        _scoreTableHost.SetLoadingIndicator(false);
        for (int i = 0; i < _scoreTableHost.Cells.Count; i++)
        {
            var cell = _scoreTableHost.Cells[i];
            if (leaderboard.Scores.Length > i)
            {
                var score = leaderboard.Scores[i];
                cell.Visible = true;
                cell.Rank = score.Rank;
                cell.Score = score.ModifiedScore;
                cell.Name = score.PlayerInfo.Name; //.Replace("|", "-");
            }
            else
            {
                cell.Visible = false;
            }
        }
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        _visualLeaderboardManager.OnLeaderboardChanged -= VisualLeaderboardManager_OnLeaderboardChanged;
    }
}