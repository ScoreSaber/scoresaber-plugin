using Newtonsoft.Json;
using ScoreSaber.Core.Data;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Data.Wrappers;
using ScoreSaber.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Steamworks;
using System.Reflection;
using UnityEngine;

namespace ScoreSaber.Core.Services {
    internal class PlayerService {

        public LocalPlayerInfo localPlayerInfo { get; set; }
        public LoginStatus loginStatus { get; set; }
        public event Action<LoginStatus, string> LoginStatusChanged;
        public enum LoginStatus {
            None = 0,
            InProgress = 1,
            Error = 2,
            Success = 3
        }

        public PlayerService() {
            Plugin.Log.Debug("PlayerService Setup!");
        }

        public void ChangeLoginStatus(LoginStatus _loginStatus, string status) {

            loginStatus = _loginStatus;
            LoginStatusChanged?.Invoke(loginStatus, status);
        }

        public void GetLocalPlayerInfo() {

            if (localPlayerInfo == null) {
                SignIn().RunTask();
            }
        }

        private async Task SignIn() {

            if (loginStatus == LoginStatus.InProgress)
                return;

            ChangeLoginStatus(LoginStatus.InProgress, "Signing into ScoreSaber...");

            var userInfo = await BS_Utils.Gameplay.GetUserInfo.GetUserAsync();
            var platform = BS_Utils.Gameplay.GetUserInfo.GetPlatformUserModel();

            var nonce = string.Empty;
            var platformString = string.Empty;

            switch (userInfo.platform) {
                case UserInfo.Platform.Steam:
                    nonce = await platform.user.GetAccessTokenAsync();
                    platformString = "0";
                    break;
                case UserInfo.Platform.Oculus:
                    var accessToken = await platform.user.GetAccessTokenAsync();
                    var xplatformToken = await platform.user.GetXPlatformAccessTokenAsync();
                    nonce = accessToken + "," + xplatformToken;
                    platformString = "1";
                    break;
            }

            var playerId = userInfo.platformUserId;
            var playerName = userInfo.userName;
            


            // Get friends list
            var friends = string.Empty;
            if (userInfo.platform == UserInfo.Platform.Steam) {
                var friendDetailList = new List<string>();
                int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagAll);
                for (int i = 0; i < friendCount; i++) {
                    CSteamID friendSteamId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagAll);
                    friendDetailList.Add(friendSteamId.ToString());
                }
                friends = string.Join(",", friendDetailList);
            }
            else {
                // Fallback for non-Steam platforms (e.g., Oculus)
                 var platformNetworkPlayerModel = Resources.FindObjectsOfTypeAll<PlatformNetworkPlayerModel>().FirstOrDefault();
                if (platformNetworkPlayerModel != null) {
                    var friendIds = new List<string>();
                    foreach (var friend in platformNetworkPlayerModel.friends) {
                        var idProperty = friend.GetType().GetProperty("id");
                        if (idProperty != null) {
                            var idValue = idProperty.GetValue(friend);
                            if (idValue != null) {
                                friendIds.Add(idValue.ToString());
                                continue;
                            }
                        }
                        friendIds.Add(friend.userId);
                    }
                    friends = string.Join(",", friendIds);
                }
            }

            var playerInfo = new LocalPlayerInfo(playerId, playerName, friends, platformString, nonce);

            int attempts = 1;

            while (attempts < 4) {

                var authenticated = await AuthenticateWithScoreSaber(playerInfo);

                if (authenticated) {
                    localPlayerInfo = playerInfo;
                    string successText = "Successfully signed into ScoreSaber!";
                    if (localPlayerInfo.playerId == PlayerIDs.Denyah)
                        successText = "Wagwan piffting wots ur bbm pin?";

                    ChangeLoginStatus(LoginStatus.Success, successText);
                    break;
                } else {
                    ChangeLoginStatus(LoginStatus.Error, $"Failed, attempting again ({attempts} of 3 tries...)");
                    attempts++;
                    await Task.Delay(4000);
                }

            }

            if (loginStatus != LoginStatus.Success) {
                ChangeLoginStatus(LoginStatus.Error, "Failed to authenticate with ScoreSaber! Please restart your game");
            }
        }

        private async Task<bool> AuthenticateWithScoreSaber(LocalPlayerInfo playerInfo) {


            if (Plugin.HttpInstance.PersistentRequestHeaders.ContainsKey("Cookies")) {
                Plugin.HttpInstance.PersistentRequestHeaders.Remove("Cookies");
            }

            WWWForm form = new WWWForm();
            form.AddField("at", playerInfo.authType);
            form.AddField("playerId", playerInfo.playerId);
            form.AddField("nonce", playerInfo.playerNonce);
            form.AddField("friends", playerInfo.playerFriends);
            form.AddField("name", playerInfo.playerName);

            try {
                string response = await Plugin.HttpInstance.PostAsync("/game/auth", form);
                var authResponse = JsonConvert.DeserializeObject<AuthResponse>(response);
                playerInfo.playerKey = authResponse.a;
                playerInfo.serverKey = authResponse.e;

                Plugin.HttpInstance.PersistentRequestHeaders.Add("Cookies", $"connect.sid={playerInfo.serverKey}");
                return true;
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed user authentication: {ex.Message}");
                return false;
            }
        }

        public async Task<PlayerInfo> GetPlayerInfo(string playerId, bool full) {

            string url = $"/player/{playerId}";

            if (full) {
                url += "/full";
            } else {
                url += "/basic";
            }

            var response = await Plugin.HttpInstance.GetAsync(url);
            var playerStats = JsonConvert.DeserializeObject<PlayerInfo>(response);
            return playerStats;
        }

        public async Task<byte[]> GetReplayData(BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, int leaderboardId, ScoreMap scoreMap) {

            if (scoreMap.hasLocalReplay) {
                string replayPath = GetReplayPath(scoreMap.parent.songHash, beatmapKey.difficulty.SerializedName(), beatmapKey.beatmapCharacteristic.serializedName, scoreMap.score.leaderboardPlayerInfo.id, beatmapLevel.songName);
                if (replayPath != null) {
                    return File.ReadAllBytes(replayPath);
                }
            }

            byte[] response = await Plugin.HttpInstance.DownloadAsync($"/game/telemetry/downloadReplay?playerId={scoreMap.score.leaderboardPlayerInfo.id}&leaderboardId={leaderboardId}");

            if (response != null) {
                return response;
            } else {
                throw new Exception("Failed to download replay");
            }
        }

        private string GetReplayPath(string levelId, string difficultyName, string characteristic, string playerId, string songName) {

            songName = songName.ReplaceInvalidChars().Truncate(155);

            string path = $@"{Settings.replayPath}\{playerId}-{songName}-{difficultyName}-{characteristic}-{levelId}.dat";
            if (File.Exists(path)) {
                return path;
            }

            string legacyPath = $@"{Settings.replayPath}\{playerId}-{songName}-{levelId}.dat";
            if (File.Exists(legacyPath)) {
                return legacyPath;
            }

            return null;
        }

    }
}