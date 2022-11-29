using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using ScoreSaber.UI.Menu.Hosts;
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
}