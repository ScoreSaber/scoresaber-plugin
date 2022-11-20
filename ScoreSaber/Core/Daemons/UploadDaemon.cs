#if RELEASE
using Newtonsoft.Json;
using ScoreSaber.Core.AC;
using ScoreSaber.Core.Data.Internal;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Services;
using ScoreSaber.Core.Utils;
using ScoreSaber.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ScoreSaber.UI.Leaderboard.ScoreSaberLeaderboardViewController;

namespace ScoreSaber.Core.Daemons {

    // TODO: Actually make pretty now that we're open source
    internal class UploadDaemon : IDisposable, IUploadDaemon {

        public event Action<UploadStatus, string> UploadStatusChanged;

        public bool Uploading { get; set; }

        private readonly PlayerService _playerService = null;
        private readonly ReplayService _replayService = null;
        private readonly LeaderboardService _leaderboardService = null;

        private readonly PlayerDataModel _playerDataModel = null;
        private readonly CustomLevelLoader _customLevelLoader = null;

        public UploadDaemon(PlayerService playerService, LeaderboardService leaderboardService, ReplayService replayService, PlayerDataModel playerDataModel, CustomLevelLoader customLevelLoader) {

            _playerService = playerService;
            _replayService = replayService;
            _leaderboardService = leaderboardService;
            _playerDataModel = playerDataModel;
            _customLevelLoader = customLevelLoader;

            SetupUploader();
            Plugin.Log.Debug("Upload service setup!");
        }

        /// <summary>
        /// This method prepare both the standard and multiplayer upload process on Transition Event
        /// by linking them to <see cref="StandardSceneTransition"/> and <see cref="MultiplayerSceneTransition"/>
        /// </summary>
        private void SetupUploader() {

            var transitionSetup = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            var multiTransitionSetup = Resources.FindObjectsOfTypeAll<MultiplayerLevelScenesTransitionSetupDataSO>().FirstOrDefault();

            if (!Plugin.ScoreSubmission) {
                return;
            }

            transitionSetup.didFinishEvent -= UploadDaemonHelper.StandardSceneTransitionInstance;
            transitionSetup.didFinishEvent -= StandardSceneTransition;
            UploadDaemonHelper.StandardSceneTransitionInstance = StandardSceneTransition;
            transitionSetup.didFinishEvent += StandardSceneTransition;

            multiTransitionSetup.didFinishEvent -= UploadDaemonHelper.MultiplayerSceneTransitionInstance;
            multiTransitionSetup.didFinishEvent -= MultiplayerSceneTransition;
            UploadDaemonHelper.MultiplayerSceneTransitionInstance = MultiplayerSceneTransition;
            multiTransitionSetup.didFinishEvent += MultiplayerSceneTransition;
        }

        /// <summary>
        /// This method prepare standard <c>data</c> and <c>results</c> to be sent to <see cref="ReplayHandler"/>
        /// </summary>
        public void StandardSceneTransition(StandardLevelScenesTransitionSetupDataSO data, LevelCompletionResults results) {

            ReplayHandler(data.gameMode, data.difficultyBeatmap, results, data.practiceSettings != null);
        }

        /// <summary>
        /// This method prepare multiplayer <c>data</c> and <c>results</c> to be sent to <see cref="ReplayHandler"/>
        /// </summary>
        public void MultiplayerSceneTransition(MultiplayerLevelScenesTransitionSetupDataSO data, MultiplayerResultsData results) {

            if (data.difficultyBeatmap == null) {
                return;
            }
            if (results.localPlayerResultData.multiplayerLevelCompletionResults.levelCompletionResults == null) {
                return;
            }
            if (results.localPlayerResultData.multiplayerLevelCompletionResults.playerLevelEndReason == MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndReason.HostEndedLevel) {
                return;
            }
            if (results.localPlayerResultData.multiplayerLevelCompletionResults.levelCompletionResults.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared) {
                return;
            }

            ReplayHandler(data.gameMode, data.difficultyBeatmap, results.localPlayerResultData.multiplayerLevelCompletionResults.levelCompletionResults, false);
        }

        /// <summary>
        /// This method handle replay in situation where the score can't be uploaded,
        /// otherwise <c>beatmap</c> and <c>results</c> get sent to <see cref="PreUpload"/>
        /// </summary>
        public void ReplayHandler(string gameMode, IDifficultyBeatmap levelCasted, LevelCompletionResults results, bool practiceMode) {

            try {
                if (Plugin.ReplayState.IsPlaybackEnabled) {
                    return;
                }

                var practiceViewController =
                    Resources.FindObjectsOfTypeAll<PracticeViewController>().FirstOrDefault();
                if (!practiceViewController.isInViewControllerHierarchy) {
                    if (gameMode != "Solo" && gameMode != "Multiplayer") {
                        return;
                    }

                    Plugin.Log.Debug($"Starting upload process for {levelCasted.level.levelID}:{levelCasted.level.songName}");

                    if (practiceMode) {
                        // If practice write replay at this point
                        _replayService.WriteSerializedReplay().RunTask();
                        return;
                    }

                    if (results.levelEndAction != LevelCompletionResults.LevelEndAction.None) {
                        _replayService.WriteSerializedReplay().RunTask();
                        return;
                    }

                    if (results.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared) {
                        _replayService.WriteSerializedReplay().RunTask();
                        return;
                    }

                    PreUpload(levelCasted, results);
                } else {
                    // If practice write replay at this point
                    _replayService.WriteSerializedReplay().RunTask();
                }
            } catch (Exception ex) {
                UploadStatusChanged?.Invoke(UploadStatus.Error, "Failed to upload score, error written to log.");
                Plugin.Log.Error($"Failed to upload score: {ex}");
            }
        }

        /// <summary>
        /// This method prepare and verify the data to be uploaded. It also encode the score data with MD5 in UTF8.
        /// The data get sent to <see cref="UploadScore"/>
        /// </summary>
        async void PreUpload(IDifficultyBeatmap levelCasted, LevelCompletionResults results) {

            if (!(levelCasted.level is CustomBeatmapLevel)) {
                return;
            }

            var defaultEnvironment = _customLevelLoader.LoadEnvironmentInfo(null, false);

            var beatmapData =
                await levelCasted.GetBeatmapDataAsync(defaultEnvironment, _playerDataModel.playerData.playerSpecificSettings);

            if (LeaderboardUtils.ContainsV3Stuff(beatmapData)) {
                UploadStatusChanged?.Invoke(UploadStatus.Error, "New note type not supported, not uploading");
                return;
            }

            double maxScore = LeaderboardUtils.MaxRawScoreForNumberOfNotes(beatmapData.cuttableNotesCount);
            // Modifiers?
            maxScore *= 1.12;

            if (results.modifiedScore > maxScore) {
                return; // Score is above maximum possible, not uploading.
            }

            try {
                UploadStatusChanged?.Invoke(UploadStatus.Packaging, "Packaging score...");
                var data = ScoreSaberUploadData.Create(levelCasted, results, _playerService.LocalPlayerInfo, new AntiCheat().AC());
                string scoreData = JsonConvert.SerializeObject(data);

                // TODO: Simplify now that we're open source

                byte[] encodedPassword = new UTF8Encoding().GetBytes($"f0b4a81c9bd3ded1081b365f7628781f-{_playerService.LocalPlayerInfo.PlayerKey}-{_playerService.LocalPlayerInfo.PlayerId}-f0b4a81c9bd3ded1081b365f7628781f");

                byte[] keyHash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);

                string key = BitConverter.ToString(keyHash)
                    .Replace("-", string.Empty)
                    .ToLower();

                string scoreDataHex = BitConverter.ToString(Swap(Encoding.UTF8.GetBytes(scoreData), Encoding.UTF8.GetBytes(key))).Replace("-", "");

                UploadScore(data, scoreDataHex, levelCasted, results).RunTask();
            } catch (Exception ex) {
                UploadStatusChanged?.Invoke(UploadStatus.Error, "Failed to upload score, error written to log.");
                Plugin.Log.Error($"Failed to upload score: {ex}");
            }
        }

        /// <summary>
        /// This method verify the leaderboard and rank status of the level. Proceed to package the replay and upload it.
        /// Finally, attempt to upload the score. On success, <see cref="SaveLocalReplay"/> is called.
        /// </summary>
        public async Task UploadScore(ScoreSaberUploadData rawDataCasted, string data, IDifficultyBeatmap levelCasted, LevelCompletionResults results) {

            try {
                UploadStatusChanged?.Invoke(UploadStatus.Packaging, "Checking leaderboard ranked status...");

                var currentLeaderboard = await _leaderboardService.GetCurrentLeaderboard(levelCasted);

                if (currentLeaderboard != null) {
                    if (currentLeaderboard.LeaderboardInfo.PlayerScore != null) {
                        if (results.modifiedScore < currentLeaderboard.LeaderboardInfo.PlayerScore.ModifiedScore) {
                            UploadStatusChanged?.Invoke(UploadStatus.Error, "Didn't beat score, not uploading.");
                            UploadStatusChanged?.Invoke(UploadStatus.Done, "");
                            Uploading = false;
                            return;
                        }
                    }
                } else {
                    Plugin.Log.Debug("Failed to get leaderboards ranked status");
                }

                bool done = false;
                bool failed = false;
                int attempts = 1;
                UploadStatusChanged?.Invoke(UploadStatus.Packaging, "Packaging replay...");
                byte[] serializedReplay = await _replayService.WriteSerializedReplay();

                // Create http packet
                var form = new WWWForm();
                form.AddField("data", data);
                if (serializedReplay != null) {
                    Plugin.Log.Debug($"Replay size: {serializedReplay.Length}");
                    form.AddBinaryData("zr", serializedReplay);
                } else {
                    UploadStatusChanged?.Invoke(UploadStatus.Error, "Failed to upload (failed to serialize replay)");
                    done = true;
                    failed = true;
                }

                // Start upload process
                while (!done) {
                    Uploading = true;
                    string response = null;

                    Plugin.Log.Info("Attempting score upload...");
                    UploadStatusChanged?.Invoke(UploadStatus.Uploading, "Uploading score...");

                    try {
                        response = await Plugin.HttpInstance.PostAsync("/game/upload", form);
                    } catch (HttpErrorException httpException) {
                        Plugin.Log.Error(httpException.IsScoreSaberError
                            ? $"Failed to upload score: {httpException.ScoreSaberError.ErrorMessage}:{httpException}"
                            : $"Failed to upload score: {httpException.IsNetworkError}:{httpException.IsHttpError}:{httpException}");
                    } catch (Exception ex) {
                        Plugin.Log.Error($"Failed to upload score: {ex}");
                    }

                    if (!string.IsNullOrEmpty(response)) {
                        if (response.Contains("uploaded")) {
                            done = true;
                        } else {
                            if (response == "banned") {
                                UploadStatusChanged?.Invoke(UploadStatus.Error, "Failed to upload (banned)");
                                done = true;
                                failed = true;
                            }
                            Plugin.Log.Error($"Raw failed response: ${response}");
                        }
                    }

                    if (done) {
                        continue;
                    }

                    if (attempts < 4) {
                        UploadStatusChanged?.Invoke(UploadStatus.Retrying, $"Failed, attempting again ({attempts} of 3 tries...)");
                        attempts++;
                        await Task.Delay(1000);
                    } else {
                        done = true;
                        failed = true;
                    }
                }

                if (!failed) {
                    SaveLocalReplay(rawDataCasted, levelCasted, serializedReplay);
                    Plugin.Log.Info("Score uploaded!");
                    UploadStatusChanged?.Invoke(UploadStatus.Success, $"Score uploaded!");
                } else {
                    UploadStatusChanged?.Invoke(UploadStatus.Error, $"Failed to upload score.");
                }

                Uploading = false;
                UploadStatusChanged?.Invoke(UploadStatus.Done, "");
            } catch (Exception) {
                Uploading = false;
                UploadStatusChanged?.Invoke(UploadStatus.Done, "");
            }
        }

        /// <summary>
        /// This method write the local replay in the UserData/ScoreSaber/Replays folder following the specific format:
        /// Player ID - Song Name - Difficulty - BeatmapCharacteristic - LeaderboardID
        /// </summary>
        private void SaveLocalReplay(ScoreSaberUploadData rawDataCasted, IDifficultyBeatmap levelCasted, byte[] replay) {

            try {
                if (replay != null) {
                    if (!Plugin.Settings.SaveLocalReplays) {
                        return;
                    }

                    string replayPath =
                        $@"{Settings.ReplayPath}\{rawDataCasted.playerId}-{rawDataCasted.songName.ReplaceInvalidChars().Truncate(155)}-{levelCasted.difficulty.SerializedName()}-{levelCasted.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}-{rawDataCasted.leaderboardId}.dat";
                    File.WriteAllBytes(replayPath, replay);
                } else {
                    Plugin.Log.Error("Failed to write local replay; replay is null");
                }
            } catch (Exception) {
                // ignored
            }
        }

        public void Dispose() {

            Plugin.Log.Info("Upload service successfully deconstructed");
            var transitionSetup = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            if (transitionSetup != null) {
                transitionSetup.didFinishEvent -= StandardSceneTransition;
            }
        }

        private byte[] Swap(byte[] panda1, byte[] panda2) {

            int N1 = 11;
            int N2 = 13;
            int NS = 257;

            for (int I = 0; I <= panda2.Length - 1; I++) {
                NS += NS % (panda2[I] + 1);
            }

            byte[] T = new byte[panda1.Length];
            for (int I = 0; I <= panda1.Length - 1; I++) {
                NS = panda2[I % panda2.Length] + NS;
                N1 = (NS + 5) * (N1 & 255) + (N1 >> 8);
                N2 = (NS + 7) * (N2 & 255) + (N2 >> 8);
                NS = ((N1 << 8) + N2) & 255;

                T[I] = (byte)(panda1[I] ^ (byte)(NS));
            }

            return T;
        }
    }
}
#endif