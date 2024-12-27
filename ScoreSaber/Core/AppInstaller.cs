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
            Container.Bind<ScoreSaberHttpClient>().FromInstance(new ScoreSaberHttpClient(new HttpClientOptions(applicationName: "ScoreSaber-PC", version: Plugin.Instance.LibVersion, defaultTimeout: 5, uploadTimeout: 120))).AsSingle();
        }
    }
}
