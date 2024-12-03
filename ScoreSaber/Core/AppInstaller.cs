using ScoreSaber.Core.Daemons;
using ScoreSaber.Core.Services;
using System.Reflection;
using Zenject;

namespace ScoreSaber.Core {
    internal class AppInstaller : Installer {

        public override void InstallBindings() {
            Plugin.Container = Container;
            Container.Bind<PlayerService>().AsSingle();

            Container.Bind<ReplayService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ScoreSaberRichPresenceService>().AsSingle();
        }
    }
}
