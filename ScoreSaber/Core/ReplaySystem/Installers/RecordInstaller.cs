using Zenject;
using ScoreSaber.Core.ReplaySystem.Recorders;

namespace ScoreSaber.Core.ReplaySystem.Installers
{
    internal class RecordInstaller : Installer
    {
        public override void InstallBindings() {

            if (!Plugin.ReplayState.isPlaybackEnabled) {

                Container.BindInterfacesAndSelfTo<Recorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<MetadataRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<HeightEventRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<NoteEventRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<PoseRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<ScoreEventRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<EnergyEventRecorder>().AsSingle();
                Container.BindMemoryPool<SwingFinisher, MemoryPool<SwingFinisher>>().WithInitialSize(64);
            }
        }
    }
}
