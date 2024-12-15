using ScoreSaber.Core.Daemons;
using ScoreSaber.Core.Http;
using ScoreSaber.Core.Services;
using System.Reflection;
using Zenject;

namespace ScoreSaber.Core {
    internal class AppInstaller : Installer {

        public override void InstallBindings() {
            Plugin.Container = Container;
            Container.Bind<PlayerService>().AsSingle();

            Container.Bind<ReplayService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<RichPresenceService>().AsSingle();
            Container.Bind<ScoreSaberHttpClient>().FromInstance(new ScoreSaberHttpClient(new("ScoreSaber-PC", Plugin.Instance.LibVersion, 5, 120))).AsSingle();
        }
    }
}
