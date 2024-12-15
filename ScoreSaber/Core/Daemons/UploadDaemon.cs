#if RELEASE
using Newtonsoft.Json;
using ScoreSaber.Core.Data;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Services;
using ScoreSaber.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ScoreSaber.Core.Utils;
using static ScoreSaber.UI.Leaderboard.ScoreSaberLeaderboardViewController;
using System.Threading;
using ScoreSaber.Core.Http;
using ScoreSaber.Core.Http.Configuration;
using ScoreSaber.Core.Http.Endpoints.API;

namespace ScoreSaber.Core.Daemons {

    // TODO: Actually make pretty now that we're open source
    internal class UploadDaemon : IDisposable, IUploadDaemon {

        public event Action<UploadStatus, string> UploadStatusChanged;

        public bool uploading { get; set; }

        private readonly PlayerService _playerService = null;
        private readonly ReplayService _replayService = null;
        private readonly LeaderboardService _leaderboardService = null;
        private readonly ScoreSaberHttpClient _scoreSaberHttpClient = null;

        private readonly PlayerDataModel _playerDataModel = null;
        private readonly MaxScoreCache _maxScoreCache = null;

        private const string UPLOAD_SECRET = "f0b4a81c9bd3ded1081b365f7628781f";

        public UploadDaemon(PlayerService playerService, LeaderboardService leaderboardService, ReplayService replayService, PlayerDataModel playerDataModel, MaxScoreCache maxScoreCache, ScoreSaberHttpClient scoreSaberHttpClient) {
            _playerService = playerService;
            _replayService = replayService;
            _leaderboardService = leaderboardService;
            _playerDataModel = playerDataModel;
            _maxScoreCache = maxScoreCache;
            _scoreSaberHttpClient = scoreSaberHttpClient;

            SetupUploader();
            Plugin.Log.Debug("Upload service setup!");
        }

        private void SetupUploader() {

            var transitionSetup = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            var multiTransitionSetup = Resources.FindObjectsOfTypeAll<MultiplayerLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            if (Plugin.ScoreSubmission) {
                transitionSetup.didFinishEvent -= UploadDaemonHelper.ThreeInstance;
                transitionSetup.didFinishEvent -= Three;
                UploadDaemonHelper.ThreeInstance = Three;
                transitionSetup.didFinishEvent += Three;

                multiTransitionSetup.didFinishEvent -= UploadDaemonHelper.FourInstance;
                multiTransitionSetup.didFinishEvent -= Four;
                UploadDaemonHelper.FourInstance = Four;
                multiTransitionSetup.didFinishEvent += Four;
            }
        }

        // Standard uploader
        public void Three(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSO, LevelCompletionResults levelCompletionResults) {
            Five(standardLevelScenesTransitionSetupDataSO.gameMode, standardLevelScenesTransitionSetupDataSO.beatmapLevel, standardLevelScenesTransitionSetupDataSO.beatmapKey, levelCompletionResults, standardLevelScenesTransitionSetupDataSO.practiceSettings != null);
        }

        // Multiplayer uploader
        public void Four(MultiplayerLevelScenesTransitionSetupDataSO multiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData multiplayerResultsData) {

            if (multiplayerLevelScenesTransitionSetupDataSO.beatmapLevel == null) {
                return;
            }
            if (multiplayerResultsData.localPlayerResultData.multiplayerLevelCompletionResults.levelCompletionResults == null) {
                return;
            }
            if (multiplayerResultsData.localPlayerResultData.multiplayerLevelCompletionResults.playerLevelEndReason == MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndReason.HostEndedLevel) {
                return;
            }
            if (multiplayerResultsData.localPlayerResultData.multiplayerLevelCompletionResults.levelCompletionResults.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared) {
                return;
            }

            Five(multiplayerLevelScenesTransitionSetupDataSO.gameMode, multiplayerLevelScenesTransitionSetupDataSO.beatmapLevel, multiplayerLevelScenesTransitionSetupDataSO.beatmapKey, multiplayerResultsData.localPlayerResultData.multiplayerLevelCompletionResults.levelCompletionResults, false);
        }

        public void Five(string gameMode, BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, LevelCompletionResults levelCompletionResults, bool practicing) {
            try {

                if (Plugin.ReplayState.IsPlaybackEnabled) { return; }
                var practiceViewController = Resources.FindObjectsOfTypeAll<PracticeViewController>().FirstOrDefault();

                if (practiceViewController.isInViewControllerHierarchy) {
                    _replayService.WriteSerializedReplay().RunTask();
                    return;
                }

                if (gameMode == "Solo" || gameMode == "Multiplayer") {
                    Plugin.Log.Debug($"Starting upload process for {beatmapKey.levelId}:{beatmapLevel.songName}");
                    if (practicing) {
                        // If practice write replay at this point
                        _replayService.WriteSerializedReplay().RunTask();
                        return;
                    }
                    if (levelCompletionResults.levelEndAction != LevelCompletionResults.LevelEndAction.None) {
                        if (levelCompletionResults.levelEndAction == LevelCompletionResults.LevelEndAction.Restart) {
                            Plugin.Log.Debug("Level was restarted before it was finished, don't write replay");
                        } else {
                        _replayService.WriteSerializedReplay().RunTask();
                        }
                        return;
                    }
                    if (levelCompletionResults.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared) {
                        _replayService.WriteSerializedReplay().RunTask();
                        return;
                    }
                    Six(beatmapLevel, beatmapKey, levelCompletionResults);
                }
            } catch (Exception ex) {
                UploadStatusChanged?.Invoke(UploadStatus.Error, "Failed to upload score, error written to log.");
                Plugin.Log.Error($"Failed to upload score: {ex}");
            }
        }

        //This starts the upload processs
        async void Six(BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, LevelCompletionResults levelCompletionResults) {

            int maxScore = await _maxScoreCache.GetMaxScore(beatmapLevel, beatmapKey);

            if (levelCompletionResults.multipliedScore > maxScore) {
                UploadStatusChanged?.Invoke(UploadStatus.Error, "Failed to upload (score was impossible)");
                Plugin.Log.Debug($"Score was better than possible, not uploading!");
                return;
            }

            try {
                UploadStatusChanged?.Invoke(UploadStatus.Packaging, "Packaging score...");
                ScoreSaberUploadData data = ScoreSaberUploadData.Create(beatmapLevel, beatmapKey, levelCompletionResults, _playerService.localPlayerInfo, GetVersionHash());
                string scoreData = JsonConvert.SerializeObject(data);

                // TODO: Simplify now that we're open source
                byte[] encodedPassword = new UTF8Encoding().GetBytes($"{UPLOAD_SECRET}-{_playerService.localPlayerInfo.playerKey}-{_playerService.localPlayerInfo.playerId}-{UPLOAD_SECRET}");
                byte[] keyHash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
                string key = BitConverter.ToString(keyHash)
                        .Replace("-", string.Empty)
                        .ToLower();

                string scoreDataHex = BitConverter.ToString(Swap(Encoding.UTF8.GetBytes(scoreData), Encoding.UTF8.GetBytes(key))).Replace("-", "");
                Seven(data, scoreDataHex, beatmapLevel, beatmapKey, levelCompletionResults).RunTask();
            } catch (Exception ex) {
                UploadStatusChanged?.Invoke(UploadStatus.Error, "Failed to upload score, error written to log.");
                Plugin.Log.Error($"Failed to upload score: {ex}");
            }
        }

        public async Task Seven(ScoreSaberUploadData scoreSaberUploadData, string uploadData, BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, LevelCompletionResults results) {
            try {
                UploadStatusChanged?.Invoke(UploadStatus.Packaging, "Packaging replay...");
                byte[] serializedReplay = await _replayService.WriteSerializedReplay();

                UploadStatusChanged?.Invoke(UploadStatus.Packaging, "Checking leaderboard ranked status...");

                bool ranked = true;
                Leaderboard currentLeaderboard = await _leaderboardService.GetCurrentLeaderboard(beatmapKey, beatmapLevel);

                if (currentLeaderboard != null) {
                    ranked = currentLeaderboard.leaderboardInfo.ranked;
                    if (currentLeaderboard.leaderboardInfo.playerScore != null) {
                        if (results.modifiedScore < currentLeaderboard.leaderboardInfo.playerScore.modifiedScore) {
                            UploadStatusChanged?.Invoke(UploadStatus.Error, "Didn't beat score, not uploading.");
                            UploadStatusChanged?.Invoke(UploadStatus.Done, "");
                            uploading = false;
                            return;
                        }
                    }
                } else {
                    Plugin.Log.Debug("Failed to get leaderboards ranked status");
                }

                bool done = false;
                bool failed = false;
                int attempts = 1;

                // Create http packet
                WWWForm form = new WWWForm();
                form.AddField("data", uploadData);
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
                    uploading = true;
                    string response = null;
                    Plugin.Log.Info("Attempting score upload...");
                    UploadStatusChanged?.Invoke(UploadStatus.Uploading, "Uploading score...");
                    try {
                        response = await _scoreSaberHttpClient.PostRawAsync(new UploadRequest().BuildUrl(), form); // this endpoint doesnt give back json, just a raw string, so we have to use PostRawAsync
                    } catch (HttpRequestException httpException) {
                        if (httpException.IsScoreSaberError) {
                            Plugin.Log.Error($"Failed to upload score: {httpException.ScoreSaberError.errorMessage}:{httpException}");
                        } else {
                            Plugin.Log.Error($"Failed to upload score: {httpException.IsNetworkError}:{httpException.IsHttpError}:{httpException}");
                        }
                    } catch (Exception ex) {
                        Plugin.Log.Error($"Failed to upload score: {ex.ToString()}");
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

                    if (!done) {
                        if (attempts < 4) {
                            UploadStatusChanged?.Invoke(UploadStatus.Retrying, $"Failed, attempting again ({attempts} of 3 tries...)");
                            attempts++;
                            await Task.Delay(1000);
                        } else {
                            done = true;
                            failed = true;
                        }
                    }
                }

                if (done && !failed) {
                    SaveLocalReplay(scoreSaberUploadData, beatmapKey, serializedReplay);
                    Plugin.Log.Info("Score uploaded!");
                    UploadStatusChanged?.Invoke(UploadStatus.Success, $"Score uploaded!");
                }

                if (failed) {
                    UploadStatusChanged?.Invoke(UploadStatus.Error, $"Failed to upload score.");
                }

                uploading = false;
                UploadStatusChanged?.Invoke(UploadStatus.Done, "");
            } catch (Exception) {
                uploading = false;
                UploadStatusChanged?.Invoke(UploadStatus.Done, "");
            }
        }

        private void SaveLocalReplay(ScoreSaberUploadData scoreSaberUploadData, BeatmapKey beatmapKey, byte[] replay) {

            if (replay == null) {
                Plugin.Log.Error("Failed to write local replay; replay is null");
                return;
            }

            try {
                if (Plugin.Settings.saveLocalReplays) {
                    string replayPath = $@"{Settings.replayPath}\{scoreSaberUploadData.playerId}-{scoreSaberUploadData.songName.ReplaceInvalidChars().Truncate(155)}-{beatmapKey.difficulty.SerializedName()}-{beatmapKey.beatmapCharacteristic.serializedName}-{scoreSaberUploadData.leaderboardId}.dat";
                    File.WriteAllBytes(replayPath, replay);
                }
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to write local replay; {ex}");
            }
        }

        public void Dispose() {
            Plugin.Log.Info("Upload service succesfully deconstructed");
            var transitionSetup = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            if (transitionSetup != null)
                transitionSetup.didFinishEvent -= Three;
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

        internal static string GetVersionHash() {
            using (var md5 = MD5.Create()) {
                string versionString = string.Format("{0}{1}", Plugin.Instance.LibVersion, IPA.Utilities.UnityGame.GameVersion);
                string hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(versionString))).Replace("-", "").ToLowerInvariant();
                return hash;
            }
        }
    }
}
#endif
