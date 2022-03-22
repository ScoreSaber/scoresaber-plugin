#if DEV
using ScoreSaber.Core.Data;
using ScoreSaber.Core.Services;
using ScoreSaber.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static ScoreSaber.UI.ViewControllers.ScoreSaberLeaderboardViewController;

namespace ScoreSaber.Core.Daemons {
    internal class MockUploadDaemon : IUploadDaemon {

        public bool uploading { get; set; }
        public event Action<UploadStatus, string> UploadStatusChanged;

        private readonly PlayerService _playerService = null;
        private readonly LeaderboardService _leaderboardService = null;

        public MockUploadDaemon(LeaderboardService leaderboardService, PlayerService playerService) {

            _playerService = playerService;
            _leaderboardService = leaderboardService;

            var transitionSetup = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            if (Plugin.ScoreSubmission) {
                UploadDaemonHelper.FiveInstance = FakeUpload;
                transitionSetup.didFinishEvent -= UploadDaemonHelper.FiveInstance;
                transitionSetup.didFinishEvent += UploadDaemonHelper.FiveInstance;
            }

            Plugin.Log.Debug("MockUpload service setup!");
        }

        public void FakeUpload(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSO, LevelCompletionResults levelCompletionResults) {
            uploading = true;
            if (!Plugin.ReplayState.isPlaybackEnabled) {
                Plugin.ReplayRecorder?.Write();
                var practiceViewController = Resources.FindObjectsOfTypeAll<PracticeViewController>().FirstOrDefault();
                if (!practiceViewController.isInViewControllerHierarchy) {

                    if (standardLevelScenesTransitionSetupDataSO.gameMode == "Solo") {

                        if (standardLevelScenesTransitionSetupDataSO.practiceSettings != null) { return; }
                        if (levelCompletionResults.levelEndAction != LevelCompletionResults.LevelEndAction.None) { return; }
                        if (levelCompletionResults.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared) { return; }

                        
                        if (standardLevelScenesTransitionSetupDataSO.difficultyBeatmap.level is CustomBeatmapLevel) {
                            if (_leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo.playerScore != null) {
                                if (levelCompletionResults.modifiedScore < _leaderboardService.currentLoadedLeaderboard.leaderboardInfoMap.leaderboardInfo.playerScore.modifiedScore) {
                                    UploadStatusChanged?.Invoke(UploadStatus.Error, "Didn't beat score, not uploading.");
                                    return;
                                }
                            }
                        }

                        // We good, "upload" the score
                        WriteReplay(standardLevelScenesTransitionSetupDataSO.difficultyBeatmap).RunTask();
                    }
                }
            }
        }

        public async Task WriteReplay(IDifficultyBeatmap beatmap) {

            UploadStatusChanged?.Invoke(UploadStatus.Uploading, "Packaging replay...");
            await TaskEx.WaitUntil(() => Plugin.ReplayState.serializedReplay != null);

            if (Plugin.Settings.saveLocalReplays) {
                string replayPath = $@"{Settings.replayPath}\{_playerService.localPlayerInfo.playerId}-{beatmap.level.songName.ReplaceInvalidChars().Truncate(155)}-{beatmap.difficulty.SerializedName()}-{beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}-{beatmap.level.levelID}.dat";
                File.WriteAllBytes(replayPath, Plugin.ReplayState.serializedReplay);
            }
            uploading = false;

            UploadStatusChanged?.Invoke(UploadStatus.Success, $"Mock score replay saved!");
        }
    }
}
#endif