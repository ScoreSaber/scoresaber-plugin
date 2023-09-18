using ScoreSaber.Core.Daemons;
using System.Reflection;
using Zenject;

namespace ScoreSaber.Core {
    internal class AppInstaller : Installer {

        public override void InstallBindings() {
            Plugin.Container = Container;

            Container.Bind<ReplayService>().AsSingle().NonLazy();
        }
    }
}
