using ScoreSaber.Core.AffinityPatches;
using ScoreSaber.Core.Daemons;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.ReplaySystem;
using ScoreSaber.Core.ReplaySystem.UI;
using ScoreSaber.Core.Services;
using ScoreSaber.Core.Utils;
using ScoreSaber.Patches;
using ScoreSaber.UI.Elements.Leaderboard;
using ScoreSaber.UI.Elements.Profile;
using ScoreSaber.UI.Leaderboard;
using ScoreSaber.UI.Main;
using ScoreSaber.UI.Main.Settings.ViewControllers;
using ScoreSaber.UI.Main.ViewControllers;
using ScoreSaber.UI.Multiplayer;
using ScoreSaber.UI.PromoBanner;
using ScoreSaber.UI.ViewControllers;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace ScoreSaber.Core {
    internal class MainInstaller : Installer {

        public override void InstallBindings() {
            Container.BindInstance(new object()).WithId("ScoreSaberUIBindings").AsCached();

            Container.BindInterfacesAndSelfTo<MenuPresencePatches>().AsSingle();

            Container.BindInterfacesAndSelfTo<MenuButtonView>().AsSingle();

            Container.BindInterfacesAndSelfTo<PanelView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<ScoreSaberLeaderboardViewController>().FromNewComponentAsViewController().AsSingle();

            Container.BindInterfacesTo<ScoreSaberLeaderboard>().AsSingle();

            Container.Bind<TweeningUtils>().AsSingle();

            Container.Bind<ReplayLoader>().AsSingle().NonLazy();
            Container.BindInterfacesTo<ResultsViewReplayButtonController>().AsSingle();

            Container.Bind<GlobalLeaderboardService>().AsSingle();
            Container.Bind<LeaderboardService>().AsSingle();

            Container.Bind<MaxScoreCache>().AsSingle();
          


            Container.Bind<FAQViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<TeamViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<GlobalViewController>().FromNewComponentAsViewController().AsSingle();

            Container.Bind<MainSettingsViewController>().FromNewComponentAsViewController().AsSingle();

            Container.BindInterfacesTo<ScoreSaberMultiplayerInitializer>().AsSingle();
            //Container.BindInterfacesTo<ScoreSaberMultiplayerLobbyLeaderboardFlowManager>().AsSingle();
            Container.BindInterfacesTo<ScoreSaberMultiplayerResultsLeaderboardFlowManager>().AsSingle();
            Container.BindInterfacesTo<ScoreSaberMultiplayerLevelSelectionLeaderboardFlowManager>().AsSingle();

            Container.BindInterfacesAndSelfTo<ScoreSaberFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<ScoreSaberSettingsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();

            List<ProfilePictureView> Imageholder = Enumerable.Range(0, 10).Select(x => new ProfilePictureView(x)).ToList();
            Container.Bind<ProfilePictureView>().FromMethodMultiple(context => Imageholder).AsSingle().WhenInjectedInto<ScoreSaberLeaderboardViewController>();
            Imageholder.ForEach(x => Container.QueueForInject(x));

            List<CellClickingView> clickingViews = Enumerable.Range(0, 10).Select(x => new CellClickingView(x)).ToList();
            Container.Bind<CellClickingView>().FromMethodMultiple(context => clickingViews).AsSingle().WhenInjectedInto<ScoreSaberLeaderboardViewController>();
            clickingViews.ForEach(y => Container.QueueForInject(y));

            Container.Bind<PromoBannerView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<PromoBanner>().AsSingle().NonLazy();

#if RELEASE
            Container.BindInterfacesAndSelfTo<UploadDaemon>().AsSingle().NonLazy();
#else
            Container.BindInterfacesAndSelfTo<MockUploadDaemon>().AsSingle().NonLazy();
#endif
        }
    }
}
