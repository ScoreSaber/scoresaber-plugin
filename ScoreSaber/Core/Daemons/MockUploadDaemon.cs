#if DEV

#region

using ScoreSaber.Core.Data.Internal;
using ScoreSaber.Core.Services;
using ScoreSaber.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static ScoreSaber.UI.Leaderboard.ScoreSaberLeaderboardViewController;

#endregion

namespace ScoreSaber.Core.Daemons {
    internal class MockUploadDaemon : IUploadDaemon {
        private readonly LeaderboardService _leaderboardService;

        private readonly PlayerService _playerService;
        private readonly ReplayService _replayService;

        public MockUploadDaemon(LeaderboardService leaderboardService, PlayerService playerService,
            ReplayService replayService) {
            _playerService = playerService;
            _leaderboardService = leaderboardService;
            _replayService = replayService;

            StandardLevelScenesTransitionSetupDataSO standardtransitionSetup =
                Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            MultiplayerLevelScenesTransitionSetupDataSO multiTransitionSetup =
                Resources.FindObjectsOfTypeAll<MultiplayerLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            switch (Plugin.ScoreSubmission) {
                case true:
                    standardtransitionSetup.didFinishEvent -= UploadDaemonHelper.ThreeInstance;
                    standardtransitionSetup.didFinishEvent -= FakeUpload; // Just to be sure
                    UploadDaemonHelper.ThreeInstance = FakeUpload;
                    standardtransitionSetup.didFinishEvent += FakeUpload;

                    multiTransitionSetup.didFinishEvent -= UploadDaemonHelper.FourInstance;
                    multiTransitionSetup.didFinishEvent -= FakeMultiUpload; // Just to be sure
                    UploadDaemonHelper.FourInstance = FakeMultiUpload;
                    multiTransitionSetup.didFinishEvent += FakeMultiUpload;
                    break;
            }

            Plugin.Log.Debug("MockUpload service setup!");
        }

        public bool uploading { get; set; }
        public event Action<UploadStatus, string> UploadStatusChanged;

        public void FakeUpload(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSO,
            LevelCompletionResults levelCompletionResults) {
            ProcessUpload(standardLevelScenesTransitionSetupDataSO.gameMode,
                standardLevelScenesTransitionSetupDataSO.difficultyBeatmap, levelCompletionResults,
                standardLevelScenesTransitionSetupDataSO.practiceSettings != null);
        }


        private void FakeMultiUpload(
            MultiplayerLevelScenesTransitionSetupDataSO multiplayerLevelScenesTransitionSetupDataSO,
            MultiplayerResultsData multiplayerResultsData) {
            ProcessUpload(multiplayerLevelScenesTransitionSetupDataSO.gameMode,
                multiplayerLevelScenesTransitionSetupDataSO.difficultyBeatmap,
                multiplayerResultsData.localPlayerResultData.multiplayerLevelCompletionResults.levelCompletionResults,
                false);
        }

        private void ProcessUpload(string gameMode, IDifficultyBeatmap difficultyBeatmap,
            LevelCompletionResults levelCompletionResults, bool practicing) {
            PracticeViewController practiceViewController =
                Resources.FindObjectsOfTypeAll<PracticeViewController>().FirstOrDefault();
            switch (practiceViewController.isInViewControllerHierarchy) {
                case false: {
                    switch (gameMode) {
                        case "Solo":
                        case "Multiplayer": {
                            switch (practicing) {
                                case true:
                                    // We still want to write a replay to memory if in practice mode
                                    _replayService.WriteSerializedReplay().RunTask();
                                    return;
                            }

                            if (levelCompletionResults.levelEndAction != LevelCompletionResults.LevelEndAction.None) {
                                _replayService.WriteSerializedReplay().RunTask();
                                return;
                            }

                            if (levelCompletionResults.levelEndStateType !=
                                LevelCompletionResults.LevelEndStateType.Cleared) {
                                _replayService.WriteSerializedReplay().RunTask();
                                return;
                            }


                            switch (difficultyBeatmap.level) {
                                case CustomBeatmapLevel _: {
                                    if (_leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo
                                            .playerScore != null) {
                                        switch (levelCompletionResults.modifiedScore < _leaderboardService
                                                    .currentLoadedLeaderboard
                                                    .leaderboardInfoMap.leaderboardInfo.playerScore.modifiedScore) {
                                            case true:
                                                UploadStatusChanged?.Invoke(UploadStatus.Error,
                                                    "Didn't beat score, not uploading.");
                                                return;
                                        }
                                    }

                                    break;
                                }
                            }

                            // We good, "upload" the score
                            WriteReplay(difficultyBeatmap).RunTask();
                            break;
                        }
                    }

                    break;
                }
                default:
                    // We still want to write a replay to memory if in practice mode
                    _replayService.WriteSerializedReplay().RunTask();
                    break;
            }
        }

        public async Task WriteReplay(IDifficultyBeatmap beatmap) {
            uploading = true;
            UploadStatusChanged?.Invoke(UploadStatus.Uploading, "Packaging replay...");

            byte[] serializedReplay = await _replayService.WriteSerializedReplay();

            switch (Plugin.Settings.saveLocalReplays) {
                case true: {
                    string replayPath =
                        $@"{Settings.replayPath}\{_playerService.localPlayerInfo.playerId}-{beatmap.level.songName.ReplaceInvalidChars().Truncate(155)}-{beatmap.difficulty.SerializedName()}-{beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}-{beatmap.level.levelID}.dat";
                    File.WriteAllBytes(replayPath, serializedReplay);
                    break;
                }
            }

            uploading = false;

            UploadStatusChanged?.Invoke(UploadStatus.Success, "Mock score replay saved!");
        }
    }
}
#endif