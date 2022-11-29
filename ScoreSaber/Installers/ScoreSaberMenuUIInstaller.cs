using ScoreSaber.Leaderboard;
using ScoreSaber.UI.Menu;
using Zenject;

namespace ScoreSaber.Installers;

internal sealed class ScoreSaberMenuUIInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Bind<ScoreSaberLeaderboard>().AsSingle();
        Container.BindInterfacesTo<ScoreSaberLeaderboardManager>().AsSingle();

        Container.Bind<PanelViewController>().FromNewComponentAsViewController().AsSingle();
        Container.Bind<ScoreViewController>().FromNewComponentAsViewController().AsSingle();
    }
}