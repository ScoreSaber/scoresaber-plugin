#if DEV
using ScoreSaber.Core.Data;
using ScoreSaber.Core.Services;
using ScoreSaber.Extensions;
using SiraUtil.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static ScoreSaber.UI.Leaderboard.ScoreSaberLeaderboardViewController;

namespace ScoreSaber.Core.Daemons {
    internal class MockUploadDaemon : IUploadDaemon {

        public bool uploading { get; set; }
        public event Action<UploadStatus, string> UploadStatusChanged;

        private readonly SiraLog _siraLog;
        private readonly PlayerService _playerService = null;
        private readonly LeaderboardService _leaderboardService = null;
        private readonly ReplayService _replayService = null;

        public MockUploadDaemon(SiraLog siraLog, LeaderboardService leaderboardService, PlayerService playerService, ReplayService replayService) {

            _siraLog = siraLog;
            _playerService = playerService;
            _leaderboardService = leaderboardService;
            _replayService = replayService;

            var standardtransitionSetup = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            var multiTransitionSetup = Resources.FindObjectsOfTypeAll<MultiplayerLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            if (Plugin.ScoreSubmission) {

                standardtransitionSetup.didFinishEvent -= UploadDaemonHelper.FiveInstance;
                standardtransitionSetup.didFinishEvent -= FakeUpload; // Just to be sure

                UploadDaemonHelper.FiveInstance = FakeUpload;
                standardtransitionSetup.didFinishEvent += FakeUpload;



                multiTransitionSetup.didFinishEvent -= FakeMultiUpload;
                multiTransitionSetup.didFinishEvent += FakeMultiUpload;
            }

            Plugin.Log.Debug("MockUpload service setup!");
        }

        public void FakeUpload(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSO, LevelCompletionResults levelCompletionResults) {

            ProcessUpload(standardLevelScenesTransitionSetupDataSO.gameMode, standardLevelScenesTransitionSetupDataSO.difficultyBeatmap, levelCompletionResults, standardLevelScenesTransitionSetupDataSO.practiceSettings != null);
        }


        private void FakeMultiUpload(MultiplayerLevelScenesTransitionSetupDataSO multiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData multiplayerResultsData) {

            ProcessUpload(multiplayerLevelScenesTransitionSetupDataSO.gameMode, multiplayerLevelScenesTransitionSetupDataSO.difficultyBeatmap, multiplayerResultsData.localPlayerResultData.multiplayerLevelCompletionResults.levelCompletionResults, false);
        }

        private void ProcessUpload(string gameMode, IDifficultyBeatmap difficultyBeatmap, LevelCompletionResults levelCompletionResults, bool practicing) {

            uploading = true;
            bool doingAsynchronous = false;
            void StopUploading() {
                if (!doingAsynchronous)
                    uploading = false;
            }


            _siraLog.Notice(gameMode);
            var practiceViewController = Resources.FindObjectsOfTypeAll<PracticeViewController>().FirstOrDefault();
            if (!practiceViewController.isInViewControllerHierarchy) {
                if (gameMode == "Solo" || gameMode == "Multiplayer") {

                    if (practicing) {
                        // We still want to write a replay to memory if in practice mode
                        _replayService.WriteSerializedReplay().RunTask();
                        StopUploading();
                        return;
                    }

                    if (levelCompletionResults.levelEndAction != LevelCompletionResults.LevelEndAction.None) {
                        _replayService.WriteSerializedReplay().RunTask();
                        StopUploading();
                        return;
                    }

                    if (levelCompletionResults.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared) {
                        _replayService.WriteSerializedReplay().RunTask();
                        StopUploading();
                        return;
                    }


                    if (difficultyBeatmap.level is CustomBeatmapLevel) {
                        if (_leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo.playerScore != null) {
                            if (levelCompletionResults.modifiedScore < _leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo.playerScore.modifiedScore) {
                                UploadStatusChanged?.Invoke(UploadStatus.Error, "Didn't beat score, not uploading.");
                                StopUploading();
                                return;
                            }
                        }
                    }

                    // We good, "upload" the score
                    doingAsynchronous = true;
                    WriteReplay(difficultyBeatmap).RunTask();
                }
            } else {
                // We still want to write a replay to memory if in practice mode
                _replayService.WriteSerializedReplay().RunTask();
            }
            StopUploading();
        }

        public async Task WriteReplay(IDifficultyBeatmap beatmap) {

            UploadStatusChanged?.Invoke(UploadStatus.Uploading, "Packaging replay...");

            byte[] serializedReplay = await _replayService.WriteSerializedReplay();

            if (Plugin.Settings.saveLocalReplays) {
                string replayPath = $@"{Settings.replayPath}\{_playerService.localPlayerInfo.playerId}-{beatmap.level.songName.ReplaceInvalidChars().Truncate(155)}-{beatmap.difficulty.SerializedName()}-{beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}-{beatmap.level.levelID}.dat";
                File.WriteAllBytes(replayPath, serializedReplay);
            }
            uploading = false;

            UploadStatusChanged?.Invoke(UploadStatus.Success, $"Mock score replay saved!");
        }
    }
}
#endif