using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using ScoreSaber.Core.Data;
using ScoreSaber.Core.Services;
using ScoreSaber.UI.ViewControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace ScoreSaber.UI.Main.Settings.ViewControllers {
    [HotReload(RelativePathToLayout = @"./MainSettingsViewController.bsml")]
    internal class MainSettingsViewController : BSMLAutomaticViewController 
    {
        [Inject] private readonly RichPresenceService _richPresenceService = null;
        [Inject] private readonly MenuButtonView _menuButtonView = null;

        // NORMAL SETTINGS
        [UIValue("showScorePP")]
        public bool ShowScorePP {
            get => Plugin.Settings.showScorePP;
            set => Plugin.Settings.showScorePP = value;
        }

        [UIValue("showLocalPlayerRank")]
        public bool ShowLocalPlayerRank {
            get => Plugin.Settings.showLocalPlayerRank;
            set => Plugin.Settings.showLocalPlayerRank = value;
        }

        [UIValue("hideNAScores")]
        public bool HideNAScores {
            get => Plugin.Settings.hideNAScoresFromLeaderboard;
            set => Plugin.Settings.hideNAScoresFromLeaderboard = value;
        }

        [UIValue("locationFilterMode")]
        public string LocationFilterMode {
            get => Plugin.Settings.locationFilterMode;
            set => Plugin.Settings.locationFilterMode = value;
        }

        [UIValue("enableCountryLeaderboards")]
        public bool EnableCountryLeaderboards {
            get => Plugin.Settings.enableCountryLeaderboards;
            set => Plugin.Settings.enableCountryLeaderboards = value;
        }

        [UIValue("locationFilerOptions")]
        public List<object> LocationFilterOptions = new object[] {
            "Country",
            "Region",
        }.ToList();

        // REPLAY SETTINGS

        [UIValue("saveLocalReplays")]
        public bool SaveLocalReplays {
            get => Plugin.Settings.saveLocalReplays;
            set => Plugin.Settings.saveLocalReplays = value;
        }

        [UIValue("replayCameraSmoothing")]
        public bool ReplayCameraSmoothing {
            get => Plugin.Settings.replayCameraSmoothing;
            set => Plugin.Settings.replayCameraSmoothing = value;
        }

        [UIValue("replayCameraFOV")]
        public float ReplayCameraFOV {
            get => Plugin.Settings.replayCameraFOV;
            set => Plugin.Settings.replayCameraFOV = value;
        }

        [UIValue("currentXValueRotation")]
        public float currentXValueRotation {
            get => Plugin.Settings.replayCameraXRotation;
            set => Plugin.Settings.replayCameraXRotation = value;
        }


        [UIValue("currentYValueRotation")]
        public float currentYValueRotation {
            get => Plugin.Settings.replayCameraYRotation;
            set => Plugin.Settings.replayCameraYRotation = value;
        }

        [UIValue("currentZValueRotation")]
        public float currentZValueRotation {
           get => Plugin.Settings.replayCameraZRotation;
           set => Plugin.Settings.replayCameraZRotation = value;
        }

        [UIValue("currentXValueOffset")]
        public float currentXValueOffset {
            get => Plugin.Settings.replayCameraXOffset;
            set => Plugin.Settings.replayCameraXOffset = value;
        }


        [UIValue("currentYValueOffset")]
        public float currentYValueOffset {
            get => Plugin.Settings.replayCameraYOffset;
            set => Plugin.Settings.replayCameraYOffset = value;
        }

        [UIValue("currentZValueOffset")]
        public float currentZValueOffset {
            get => Plugin.Settings.replayCameraZOffset;
            set => Plugin.Settings.replayCameraZOffset = value;
        }

        [UIValue("startReplayUIHidden")]
        public bool StartReplayUIHidden {
            get => Plugin.Settings.startReplayUIHidden;
            set => Plugin.Settings.startReplayUIHidden = value;
        }

        [UIValue("desktopImberSize")]
        public float DesktopImberSize {
            get => Plugin.Settings.replayUISize;
            set => Plugin.Settings.replayUISize = value;
        }


        [UIValue("currentDesktopReplayUIPositionX")]
        public float currentDesktopReplayUIPositionX {
            get => Plugin.Settings.replayUIPosition.x;
            set {
                var replayUIPosition = Plugin.Settings.replayUIPosition;
                replayUIPosition.x = value;
                Plugin.Settings.replayUIPosition = replayUIPosition;
            }
        }

        [UIValue("currentDesktopReplayUIPositionY")]
        public float currentDesktopReplayUIPositionY {
            get => Plugin.Settings.replayUIPosition.y;
            set {
                var replayUIPosition = Plugin.Settings.replayUIPosition;
                replayUIPosition.y = value;
                Plugin.Settings.replayUIPosition = replayUIPosition;
            }
        }

        [UIValue("hideWatermarkIfUserReplay")]
        public bool HideWatermarkIfUserReplay {
            get => Plugin.Settings.hideWatermarkIfUsersReplay;
            set => Plugin.Settings.hideWatermarkIfUsersReplay = value;
        }

        // MAIN SETTINGS

        [UIValue("enableRichPresence")]
        public bool EnableRichPresence {
            get => Plugin.Settings.enableRichPresence;
            set {
                Plugin.Settings.enableRichPresence = value;
                _richPresenceService.ToggleRichPresence(value);
            }
        }

        [UIValue("showMainMenuButton")]
        public bool ShowMainMenuButton {
            get => Plugin.Settings.showMainMenuButton;
            set {
                Plugin.Settings.showMainMenuButton = value;
                _menuButtonView.MenuButtonVisibilityChanged?.Invoke(value);
            }
        }
    }
}
