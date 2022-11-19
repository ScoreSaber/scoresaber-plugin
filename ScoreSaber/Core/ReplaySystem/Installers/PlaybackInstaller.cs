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
            switch (Plugin.ReplayState.IsPlaybackEnabled) {
                case true: {
                    Container.BindInstance(new object()).WithId("ScoreSaberReplay").AsCached();
                    switch (Plugin.ReplayState.IsLegacyReplay) {
                        case false: {
                            Container.BindInstance(Plugin.ReplayState.LoadedReplayFile).AsSingle();
                            Container.BindInterfacesAndSelfTo<PosePlayer>().AsSingle();
                            Container.BindInterfacesTo<NotePlayer>().AsSingle();
                            Container.BindInterfacesTo<ScorePlayer>().AsSingle();
                            Container.BindInterfacesTo<ComboPlayer>().AsSingle();
                            Container.BindInterfacesTo<EnergyPlayer>().AsSingle();
                            Container.BindInterfacesTo<MultiplierPlayer>().AsSingle();
                            switch (_gameplayCoreSceneSetupData.playerSpecificSettings.automaticPlayerHeight) {
                                case true:
                                    Container.BindInterfacesTo<HeightPlayer>().AsSingle();
                                    break;
                            }

                            Container.BindInterfacesAndSelfTo<ReplayTimeSyncController>().AsSingle();
                            Container.Bind<NonVRReplayUI>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                            Container.Bind<IAffinity>().To<CancelScoreControllerBufferFinisher>().AsSingle();
                            Container.Bind<IAffinity>().To<CancelSaberCuttingPatch>().AsSingle();
                            break;
                        }
                        default:
                            Container.Bind<IAffinity>().To<CancelScoreControllerBufferFinisher>().AsSingle();
                            Container.BindInstance(Plugin.ReplayState.LoadedLegacyKeyframes).AsSingle();
                            Container.BindInterfacesAndSelfTo<LegacyReplayPlayer>().AsSingle();
                            Container.BindInterfacesTo<LegacyReplayPatches>().AsSingle();
                            break;
                    }

                    Container.Bind<GameReplayUI>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                    break;
                }
            }
        }
    }
}