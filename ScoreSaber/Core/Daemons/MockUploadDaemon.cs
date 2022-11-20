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
        public bool Uploading { get; set; }
        public event Action<UploadStatus, string> UploadStatusChanged;

        private readonly PlayerService _playerService = null;
        private readonly LeaderboardService _leaderboardService = null;
        private readonly ReplayService _replayService = null;

        public MockUploadDaemon(LeaderboardService leaderboardService, PlayerService playerService, ReplayService replayService) {

            _playerService = playerService;
            _leaderboardService = leaderboardService;
            _replayService = replayService;

            var standardtransitionSetup = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            var multiTransitionSetup = Resources.FindObjectsOfTypeAll<MultiplayerLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            if (Plugin.ScoreSubmission) {

                standardtransitionSetup.didFinishEvent -= UploadDaemonHelper.StandardSceneTransitionInstance;
                standardtransitionSetup.didFinishEvent -= FakeUpload;
                UploadDaemonHelper.StandardSceneTransitionInstance = FakeUpload;
                standardtransitionSetup.didFinishEvent += FakeUpload;

                multiTransitionSetup.didFinishEvent -= UploadDaemonHelper.MultiplayerSceneTransitionInstance;
                multiTransitionSetup.didFinishEvent -= FakeMultiUpload;
                UploadDaemonHelper.MultiplayerSceneTransitionInstance = FakeMultiUpload;
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

            var practiceViewController = Resources.FindObjectsOfTypeAll<PracticeViewController>().FirstOrDefault();
            if (!practiceViewController.isInViewControllerHierarchy) {
                if (gameMode != "Solo" && gameMode != "Multiplayer") {
                    return;
                }

                if (practicing) {
                    // We still want to write a replay to memory if in practice mode
                    _replayService.WriteSerializedReplay().RunTask();
                    return;
                }

                if (levelCompletionResults.levelEndAction != LevelCompletionResults.LevelEndAction.None) {
                    _replayService.WriteSerializedReplay().RunTask();
                    return;
                }

                if (levelCompletionResults.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared) {
                    _replayService.WriteSerializedReplay().RunTask();
                    return;
                }


                if (difficultyBeatmap.level is CustomBeatmapLevel) {
                    if (_leaderboardService.currentLoadedLeaderboard.LeaderboardInfoMap.leaderboardInfo.PlayerScore != null) {
                        if (levelCompletionResults.modifiedScore < _leaderboardService.currentLoadedLeaderboard.LeaderboardInfoMap.leaderboardInfo.PlayerScore.ModifiedScore) {
                            UploadStatusChanged?.Invoke(UploadStatus.Error, "Didn't beat score, not uploading.");
                            return;
                        }
                    }
                }

                // We good, "upload" the score
                WriteReplay(difficultyBeatmap).RunTask();
            } else {
                // We still want to write a replay to memory if in practice mode
                _replayService.WriteSerializedReplay().RunTask();
            }
        }

        public async Task WriteReplay(IDifficultyBeatmap beatmap) {

            Uploading = true;
            UploadStatusChanged?.Invoke(UploadStatus.Uploading, "Packaging replay...");

            byte[] serializedReplay = await _replayService.WriteSerializedReplay();

            if (Plugin.Settings.SaveLocalReplays) {
                string replayPath = $@"{Settings.ReplayPath}\{_playerService.LocalPlayerInfo.PlayerId}-{beatmap.level.songName.ReplaceInvalidChars().Truncate(155)}-{beatmap.difficulty.SerializedName()}-{beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}-{beatmap.level.levelID}.dat";
                File.WriteAllBytes(replayPath, serializedReplay);
            }
            Uploading = false;

            UploadStatusChanged?.Invoke(UploadStatus.Success, $"Mock score replay saved!");
        }
    }
}
#endif