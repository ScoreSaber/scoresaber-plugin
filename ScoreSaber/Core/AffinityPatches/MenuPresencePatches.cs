using HarmonyLib;
using ScoreSaber.Core.Services;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace ScoreSaber.Core.AffinityPatches {
    public class MenuPresencePatches : IAffinity {

        [Inject] private readonly ScoreSaberRichPresenceService _richPresenceService = null;
        [Inject] private readonly SiraLog _log = null;

        [AffinityPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator), "HandleStandardLevelDidFinish")]
        [AffinityPostfix]
        public void HandleStopStandardLevelPostfix() {
            var jsonObject = new SceneChangeEvent() {
                Timestamp = _richPresenceService.TimeRightNow,
                Scene = Scene.menu
            };

            _richPresenceService.SendUserProfileChannel("scene_change", jsonObject);
            _log.Notice("Sent scene change event to rich presence service");
        }

        [AffinityPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator), nameof(SinglePlayerLevelSelectionFlowCoordinator.StartLevel))]
        [AffinityPostfix]
        public void HandleStartLevelPostfix(SinglePlayerLevelSelectionFlowCoordinator __instance, Action beforeSceneSwitchCallback, bool practice) {

            string hash = string.Empty;

            if (!__instance.selectedBeatmapLevel.hasPrecalculatedData) {
                hash = "#" + __instance.selectedBeatmapLevel.levelID;
            } else {
                hash = __instance.selectedBeatmapLevel.levelID.Split('_')[2];
            }

            bool isPractice = practice;
            Services.GameMode gameMode = Services.GameMode.solo;

            if (isPractice) {
                gameMode = Services.GameMode.practice;

                // this is to privatise the practice mode song, as it would be exposed in the rich presence, still not shown in UI though.
                var songeventSilly = new SongStartEvent(_richPresenceService.TimeRightNow, gameMode,
                                    "HYPER SONIC MEGA DEATH **** CORE",
                                    string.Empty,
                                    "oce v...",
                                    "echelon#6295",
                                    "Standard",
                                    "A297DECDFB0B3FE6F14F0BEF788AEBAC978E825E",
                                    (int)2000000000,
                                    ((1 + 1 - 1) * 0) + 1, // those who know 💀💀💀 🥭🥭🥭
                                    0,
                                    1);

                _richPresenceService.SendUserProfileChannel("start_song", songeventSilly);
                return;
            }

            GameplayModifiers gameplayModifiers = __instance.gameplayModifiers;
            int startTime = 0;
            if (__instance.isInPracticeView) {
                startTime = (int)__instance._practiceViewController._practiceSettings.startSongTime;
            }

            var songevent = new SongStartEvent(_richPresenceService.TimeRightNow, gameMode,
                                                __instance.selectedBeatmapLevel.songName,
                                                __instance.selectedBeatmapLevel.songSubName,
                                                Data.Models.ScoreSaberUploadData.FriendlyLevelAuthorName(__instance.selectedBeatmapLevel.allMappers, __instance.selectedBeatmapLevel.allLighters),
                                                __instance.selectedBeatmapLevel.songAuthorName,
                                                __instance.selectedBeatmapKey.beatmapCharacteristic.SerializedName(),
                                                hash,
                                                (int)__instance.selectedBeatmapLevel.songDuration,
                                                ((int)__instance.selectedBeatmapKey.difficulty * 2) + 1,
                                                startTime,
                                                gameplayModifiers.songSpeedMul);

            _richPresenceService.SendUserProfileChannel("start_song", songevent);
        }

        [AffinityPatch(typeof(MultiplayerLevelSelectionFlowCoordinator), nameof(MultiplayerLevelSelectionFlowCoordinator.HandleLobbyGameStateControllerGameStarted))]
        [AffinityPostfix]
        public void HandleMultiplayerGameStartPostfix(MultiplayerLevelSelectionFlowCoordinator __instance, ILevelGameplaySetupData levelGameplaySetupData) {

            string hash = string.Empty;

            if (!levelGameplaySetupData.beatmapKey.levelId.StartsWith("custom_level_")) {
                hash = "#" + __instance.selectedBeatmapLevel.levelID;
            } else {
                hash = __instance.selectedBeatmapLevel.levelID.Split('_')[2];
            }

            // i dont like this, but i have to do it
            BeatmapLevel beatmapLevel = SongCore.Loader.GetLevelByHash(levelGameplaySetupData.beatmapKey.levelId);

            GameplayModifiers gameplayModifiers = levelGameplaySetupData.gameplayModifiers;
            int startTime = 0;

            var songevent = new SongStartEvent(_richPresenceService.TimeRightNow, GameMode.multiplayer,
                                                beatmapLevel.songName,
                                                beatmapLevel.songSubName,
                                                beatmapLevel.allMappers.Join().ToString(),
                                                beatmapLevel.songAuthorName,
                                                levelGameplaySetupData.beatmapKey.beatmapCharacteristic.SerializedName(),
                                                hash,
                                                (int)beatmapLevel.songDuration,
                                                ((int)levelGameplaySetupData.beatmapKey.difficulty * 2) + 1,
                                                startTime,
                                                gameplayModifiers.songSpeedMul);

            _richPresenceService.SendUserProfileChannel("start_song", songevent);
        }
    }
}
