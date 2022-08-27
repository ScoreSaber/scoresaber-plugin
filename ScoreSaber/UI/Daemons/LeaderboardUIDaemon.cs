using System;
using System.Diagnostics;
using ScoreSaber.UI.ViewControllers;
using SiraUtil.Logging;
using Zenject;
using Object = UnityEngine.Object;

namespace ScoreSaber.UI.Daemons;

internal class LeaderboardUIDaemon : IInitializable, IDisposable {
    private readonly SiraLog _siraLog;
    private readonly PlatformLeaderboardViewController _platformLeaderboardViewController;

    public LeaderboardUIDaemon(SiraLog siraLog, PlatformLeaderboardViewController platformLeaderboardViewController) {
        _siraLog = siraLog;
        _platformLeaderboardViewController = platformLeaderboardViewController;
    }

    public void Initialize() {
        _siraLog.Info("Iniialized");
        _platformLeaderboardViewController.didActivateEvent += PlatformLeaderboardViewController_didActivateEvent;
    }

    private void PlatformLeaderboardViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
        _siraLog.Info("activating");
        _siraLog.Notice(firstActivation);
        
        if (!firstActivation)
            return;

        _siraLog.Debug($"{nameof(PlatformLeaderboardViewController)} activated for the first time. Setting up our {nameof(PlatformPanelViewController)}.");
        try {
            Stopwatch stopwatch = Stopwatch.StartNew();

            var leaderboardTransform = _platformLeaderboardViewController.transform;

            // Get the header panel under PlatformLeaderboardViewController. It should be the first child.
            var headerPanelTransform = leaderboardTransform.Find("HeaderPanel");

            // Clone the panel. This will be the base of our view controller.
            var platformPanelGO = Object.Instantiate(headerPanelTransform.gameObject, leaderboardTransform);
            platformPanelGO.transform.localPosition += new UnityEngine.Vector3(0, 15f, 0f);

            stopwatch.Stop();
            _siraLog.Debug($"Sucessfully setup the {nameof(PlatformLeaderboardViewController)} in {stopwatch.Elapsed.TotalMilliseconds}ms");
        }
        catch (Exception e) {
            _siraLog.Error($"Unable to setup the {nameof(PlatformPanelViewController)}!");
            _siraLog.Error(e);
        }
    }

    public void Dispose() {
        _platformLeaderboardViewController.didActivateEvent -= PlatformLeaderboardViewController_didActivateEvent;

    }
}
