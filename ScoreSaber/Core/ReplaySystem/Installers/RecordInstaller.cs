using Zenject;
using ScoreSaber.Core.ReplaySystem.Recorders;
using SiraUtil.Logging;
using ScoreSaber.Core.Utils;
using System.Reflection;
using ScoreSaber.Core.Daemons;

namespace ScoreSaber.Core.ReplaySystem.Installers
{
    internal class RecordInstaller : Installer
    {
        public override void InstallBindings() {

            if (!Plugin.ReplayState.IsPlaybackEnabled) {
                Plugin.Log.Debug("Installing replay recorders");
                Container.BindInterfacesAndSelfTo<Recorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<MetadataRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<HeightEventRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<NoteEventRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<PoseRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<ScoreEventRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<EnergyEventRecorder>().AsSingle();
                Plugin.Log.Debug("Replay recorders installed");
            }
        }
    }
}
