using ScoreSaber.UI.Daemons;
using Zenject;

namespace ScoreSaber.Installers;

internal class MenuInstaller : Installer {

    public override void InstallBindings() {
        // UI Setup
        Container.BindInterfacesTo<LeaderboardUIDaemon>().AsSingle();
    }
}
