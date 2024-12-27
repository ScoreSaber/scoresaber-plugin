#nullable enable
using Newtonsoft.Json;
using ScoreSaber.Core.Data;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Data.Wrappers;
using ScoreSaber.Core.Http;
using ScoreSaber.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ScoreSaber.Core.Services {
    internal class PlayerService {

        private readonly ScoreSaberHttpClient client;
        public LocalPlayerInfo? localPlayerInfo { get; set; }
        public LoginStatus loginStatus { get; set; }
        public event Action<LoginStatus, string>? LoginStatusChanged;
        public enum LoginStatus {
            None = 0,
            InProgress = 1,
            Error = 2,
            Success = 3
        }

        public PlayerService(ScoreSaberHttpClient scoreSaberHttpClient) {
            this.client = scoreSaberHttpClient;
            Plugin.Log.Debug("PlayerService Setup!");
        }

        public void ChangeLoginStatus(LoginStatus _loginStatus, string status) {

            loginStatus = _loginStatus;
            LoginStatusChanged?.Invoke(loginStatus, status);
        }

        public void SignIn() {
            if (localPlayerInfo != null) return;
            SignInTask().RunTask();
        }

        private async Task SignInTask() {
            if (loginStatus == LoginStatus.InProgress) return;
            ChangeLoginStatus(LoginStatus.InProgress, "Signing into ScoreSaber...");
            var platformUserModel = Plugin.Container.TryResolve<IPlatformUserModel>();
            var userInfo = await platformUserModel.GetUserInfo(CancellationToken.None);
            var authToken = await platformUserModel.GetUserAuthToken();
            var (nonce, platform) = userInfo.platform switch {
                UserInfo.Platform.Steam => (authToken.token, "0"),
                UserInfo.Platform.Oculus => ($"{authToken.token},{(await platformUserModel.RequestXPlatformAccessToken(CancellationToken.None)).token}", "1"),
                _ => (string.Empty, string.Empty)
            };
            if (string.IsNullOrEmpty(nonce) || string.IsNullOrEmpty(platform)) {
                ChangeLoginStatus(LoginStatus.Error, "Failed to authenticate with ScoreSaber: Invalid platform");
                return;
            }
            var friendIds = await platformUserModel.GetUserFriendsUserIds(false);
            var playerInfo = new LocalPlayerInfo(
                userInfo.platformUserId,
                userInfo.userName,
                string.Join(",", friendIds.Where(x => x != "0")),
                platform,
                nonce
            );
            for (int attempt = 1; attempt <= 3; attempt++) {
                if (await AuthenticateWithScoreSaber(playerInfo)) {
                    localPlayerInfo = playerInfo;
                    var successText = string.Empty;
                    if (localPlayerInfo.playerId != PlayerIDs.Denyah) {
                        successText = "Successfully signed into ScoreSaber!";
                    } else {
                        successText = "Wagwan piffting wots ur bbm pin?";
                    }
                    ChangeLoginStatus(LoginStatus.Success, successText);
                    return;
                }
                ChangeLoginStatus(LoginStatus.Error, $"Failed, attempting again ({attempt} of 3 tries...)");
                if (attempt < 3)
                    await Task.Delay(4000);
            }
            ChangeLoginStatus(LoginStatus.Error, "Failed to authenticate with ScoreSaber! Please restart your game");
        }

        private async Task<bool> AuthenticateWithScoreSaber(LocalPlayerInfo playerInfo) {
            try {
                var request = new Http.Endpoints.API.Player.AuthenticateRequest(
                    playerId: playerInfo.playerId,
                    authType: playerInfo.authType,
                    nonce: playerInfo.playerNonce,
                    friends: playerInfo.playerFriends,
                    name: playerInfo.playerName
                );
                var response = await client.PostAsync<AuthResponse>(request, request.Form);
                if (response != null) {
                    playerInfo.playerKey = response.a;
                    playerInfo.serverKey = response.e;
                    client.SetCookie($"connect.sid={playerInfo.serverKey}");
                    return true;
                }
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed user authentication: {ex.Message}");
            }
            return false;
        }

        public async Task<PlayerInfo> GetPlayerInfo(string playerId, bool full) {
            var request = new Http.Endpoints.API.Player.ProfileRequest(playerId, full);
            return await client.GetAsync<PlayerInfo>(request);
        }
        public async Task<RichPresenceResponse?> GetRichPresence(string playerId) {
            try {
                var request = new Http.Endpoints.Realtime.UserRequest(playerId);
                return await client.GetAsync<RichPresenceResponse>(request);
            } catch {
                return null;
            }
        }

        public async Task<byte[]> GetReplayData(BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, int leaderboardId, ScoreMap scoreMap) {
            if (scoreMap.hasLocalReplay) {
                var replayPath = GetReplayPath(scoreMap.parent.songHash, beatmapKey.difficulty.SerializedName(),
                    beatmapKey.beatmapCharacteristic.serializedName, scoreMap.score.leaderboardPlayerInfo.id, beatmapLevel.songName);
                if (replayPath != null) return File.ReadAllBytes(replayPath);
            }
            var response = await client.DownloadAsync(new Http.Endpoints.API.Player.ReplayRequest(scoreMap.score.leaderboardPlayerInfo.id, leaderboardId.ToString()));
            if (response != null) return response;
            throw new Exception("Failed to download replay");
        }

        private string? GetReplayPath(string levelId, string difficultyName, string characteristic, string playerId, string songName) {
            var sanitizedSongName = songName.ReplaceInvalidChars().Truncate(155);
            var fullPath = $@"{Settings.replayPath}\{playerId}-{sanitizedSongName}-{difficultyName}-{characteristic}-{levelId}.dat";
            if (File.Exists(fullPath)) return fullPath;
            var legacyPath = $@"{Settings.replayPath}\{playerId}-{sanitizedSongName}-{levelId}.dat";
            return File.Exists(legacyPath) ? legacyPath : null;
        }
    }
}