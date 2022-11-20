#region

using ScoreSaber.Core.ReplaySystem.HarmonyPatches;
using ScoreSaber.Core.ReplaySystem.Legacy;
using ScoreSaber.Core.ReplaySystem.Playback;
using ScoreSaber.Core.ReplaySystem.UI;
using ScoreSaber.Core.ReplaySystem.UI.Legacy;
using SiraUtil.Affinity;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Installers {

    internal class PlaybackInstaller : Installer {
        private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;

        public PlaybackInstaller(GameplayCoreSceneSetupData gameplayCoreSceneSetupData) {

            _gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
        }

        public override void InstallBindings() {

            if (!Plugin.ReplayState.IsPlaybackEnabled) {
                return;
            }

            Container.BindInstance(new object()).WithId("ScoreSaberReplay").AsCached();
            if (!Plugin.ReplayState.IsLegacyReplay) {
                Container.BindInstance(Plugin.ReplayState.LoadedReplayFile).AsSingle();
                Container.BindInterfacesAndSelfTo<PosePlayer>().AsSingle();
                Container.BindInterfacesTo<NotePlayer>().AsSingle();
                Container.BindInterfacesTo<ScorePlayer>().AsSingle();
                Container.BindInterfacesTo<ComboPlayer>().AsSingle();
                Container.BindInterfacesTo<EnergyPlayer>().AsSingle();
                Container.BindInterfacesTo<MultiplierPlayer>().AsSingle();
                if (_gameplayCoreSceneSetupData.playerSpecificSettings.automaticPlayerHeight)
                    Container.BindInterfacesTo<HeightPlayer>().AsSingle();
                Container.BindInterfacesAndSelfTo<ReplayTimeSyncController>().AsSingle();
                Container.Bind<NonVRReplayUI>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                Container.Bind<IAffinity>().To<CancelScoreControllerBufferFinisher>().AsSingle();
                Container.Bind<IAffinity>().To<CancelSaberCuttingPatch>().AsSingle();
            } else {
                Container.Bind<IAffinity>().To<CancelScoreControllerBufferFinisher>().AsSingle();
                Container.BindInstance(Plugin.ReplayState.LoadedLegacyKeyframes).AsSingle();
                Container.BindInterfacesAndSelfTo<LegacyReplayPlayer>().AsSingle();
                Container.BindInterfacesTo<LegacyReplayPatches>().AsSingle();
            }
            Container.Bind<GameReplayUI>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }
    }
}