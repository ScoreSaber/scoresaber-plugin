#region

using ScoreSaber.Core.Daemons;
using ScoreSaber.Core.ReplaySystem;
using ScoreSaber.Core.ReplaySystem.UI;
using ScoreSaber.Core.Services;
using ScoreSaber.Patches;
using ScoreSaber.UI.Leaderboard;
using ScoreSaber.UI.Main;
using ScoreSaber.UI.Main.ViewControllers;
using ScoreSaber.UI.Multiplayer;
using Zenject;

#endregion

namespace ScoreSaber.Core {
    internal class MainInstaller : Installer {
        public override void InstallBindings() {
            Container.BindInstance(new object()).WithId("ScoreSaberUIBindings").AsCached();
            Container.Bind<ReplayLoader>().AsSingle().NonLazy();
            Container.BindInterfacesTo<ResultsViewReplayButtonController>().AsSingle();

            Container.Bind<GlobalLeaderboardService>().AsSingle();
            Container.Bind<LeaderboardService>().AsSingle();
            Container.Bind<PlayerService>().AsSingle();


            Container.Bind<PanelView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<FAQViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<TeamViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<GlobalViewController>().FromNewComponentAsViewController().AsSingle();

            Container.BindInterfacesTo<ScoreSaberMultiplayerInitializer>().AsSingle();
            //Container.BindInterfacesTo<ScoreSaberMultiplayerLobbyLeaderboardFlowManager>().AsSingle();
            Container.BindInterfacesTo<ScoreSaberMultiplayerResultsLeaderboardFlowManager>().AsSingle();
            Container.BindInterfacesTo<ScoreSaberMultiplayerLevelSelectionLeaderboardFlowManager>().AsSingle();

            Container.BindInterfacesTo<ScoreSaberFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();

            Container.BindInterfacesAndSelfTo<ScoreSaberLeaderboardViewController>().AsSingle().NonLazy();
            Container.BindInterfacesTo<LeaderboardPatches>().AsSingle();

#if RELEASE
            Container.BindInterfacesAndSelfTo<UploadDaemon>().AsSingle().NonLazy();
#else
            Container.BindInterfacesAndSelfTo<MockUploadDaemon>().AsSingle().NonLazy();
#endif
        }
    }
}