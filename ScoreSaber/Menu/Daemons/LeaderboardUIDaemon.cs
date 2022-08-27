using System;
using System.Diagnostics;
using HMUI;
using Polyglot;
using ScoreSaber.UI.ViewControllers;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace ScoreSaber.UI.Daemons;

internal class LeaderboardUIDaemon : IInitializable {

    private readonly SiraLog _siraLog;
    private readonly PlatformLeaderboardViewController _platformLeaderboardViewController;

    public LeaderboardUIDaemon(SiraLog siraLog, PlatformLeaderboardViewController platformLeaderboardViewController) {

        _siraLog = siraLog;
        _platformLeaderboardViewController = platformLeaderboardViewController;
    }

    public void Initialize() {

        CreateUI();
    }

    private void CreateUI() {

        _siraLog.Debug($"{nameof(PlatformLeaderboardViewController)} activated for the first time. Setting up our {nameof(PlatformPanelViewController)}.");
        try {

            Stopwatch stopwatch = Stopwatch.StartNew();
            var leaderboardTransform = _platformLeaderboardViewController.transform;

            // Get the header panel under PlatformLeaderboardViewController. It should be the first child.
            var headerPanelTransform = leaderboardTransform.Find("HeaderPanel");

            // Clone the panel. This will be the base of our view controller.
            var platformPanelGO = Object.Instantiate(headerPanelTransform.gameObject, leaderboardTransform);
            var platformPanelTransform = platformPanelGO.GetComponent<RectTransform>();

            // Rename it for cleaner debugging and for other modders.
            platformPanelGO.name = "ScoreSaberPanelWrapper";

            // Offset the panel position to look nicely above the "HIGHSCORES" banner. 
            platformPanelTransform.localPosition += new Vector3(2.5f, 14f);

            // Enlarge the panel as well so the image wraps around it nicely.
            platformPanelTransform.sizeDelta = new Vector2(0f, 12f);

            // We reuse the text in the clone for our notification system.
            var notificationText = platformPanelGO.GetComponentInChildren<CurvedTextMeshPro>();
            notificationText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            notificationText.fontSize = 5f;

            var notificationTextGO = notificationText.gameObject;
            notificationTextGO.name = "ScoreSaberNotificationText";

            // Destroy the localizer component on it.
            Object.Destroy(notificationText.GetComponent<LocalizedTextMeshProUGUI>());

            // Move the text in the right position.
            var notificationTextTransform = notificationText.GetComponent<RectTransform>();
            notificationTextTransform.localPosition = new Vector3(17f, 3f);

            notificationText.text = "hi";

            stopwatch.Stop();
            _siraLog.Debug($"Sucessfully setup the {nameof(PlatformLeaderboardViewController)} in {stopwatch.Elapsed.TotalMilliseconds}ms");
        }
        catch (Exception e) {

            _siraLog.Error($"Unable to setup the {nameof(PlatformPanelViewController)}!");
            _siraLog.Error(e);
        }
    }
}
