#region

using ScoreSaber.Core.ReplaySystem.UI;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Installers {
    internal class ImberInstaller : Installer {
        public override void InstallBindings() {

            if (!Plugin.ReplayState.IsPlaybackEnabled || Plugin.ReplayState.IsLegacyReplay) {
                return;
            }

            Container.Bind<VRControllerAccessor>().AsSingle();
            Container.BindInterfacesTo<ImberManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<ImberScrubber>().AsSingle();
            Container.BindInterfacesAndSelfTo<ImberSpecsReporter>().AsSingle();
            Container.BindInterfacesAndSelfTo<ImberUIPositionController>().AsSingle();
            Container.Bind<MainImberPanelView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind(typeof(ITickable), typeof(SpectateAreaController)).To<SpectateAreaController>().AsSingle();
        }
    }
}