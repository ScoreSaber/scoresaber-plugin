using ScoreSaber.Leaderboard;
using ScoreSaber.UI.Menu;
using ScoreSaber.UI.Menu.Hosts;
using Zenject;

namespace ScoreSaber.Installers;

internal sealed class ScoreSaberMenuUIInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Bind<ScoreSaberLeaderboard>().AsSingle();
        Container.BindInterfacesTo<VisualLeaderboardManager>().AsSingle();
        Container.BindInterfacesTo<ScoreSaberLeaderboardManager>().AsSingle();

        Container.Bind<PanelViewController>().FromNewComponentAsViewController().AsSingle();
        Container.Bind<ScoreViewController>().FromNewComponentAsViewController().AsSingle();

        // Install the dependent hosts for the view controllers
        Container.Bind<ScoreTableHost>().AsSingle();
        Container.Bind<ScoreScopeHost>().AsSingle();
    }
}