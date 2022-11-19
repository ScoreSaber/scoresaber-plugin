#region

using ScoreSaber.Core.Daemons;
using Zenject;

#endregion

namespace ScoreSaber.Core {
    internal class AppInstaller : Installer {
        public override void InstallBindings() {
            Container.Bind<ReplayService>().AsSingle().NonLazy();
        }
    }
}