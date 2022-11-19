#region

using ScoreSaber.Core.ReplaySystem.Recorders;
using ScoreSaber.Core.Utils;
using SiraUtil.Logging;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Installers {
    internal class RecordInstaller : Installer {
        private readonly SiraLog _siraLog;
        private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;

        public RecordInstaller(SiraLog siraLog, GameplayCoreSceneSetupData gameplayCoreSceneSetupData) {

            _siraLog = siraLog;
            _gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
        }

        public override void InstallBindings() {

            bool hasV3Stuff = LeaderboardUtils.ContainsV3Stuff(_gameplayCoreSceneSetupData.transformedBeatmapData);

            if (hasV3Stuff) {
                _siraLog.Warn("This map contains Beatmap V3 sliders! Not recording...");
                return;
            }

            if (Plugin.ReplayState.IsPlaybackEnabled) {
                return;
            }

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