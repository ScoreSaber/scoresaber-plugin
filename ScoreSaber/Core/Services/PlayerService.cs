using HarmonyLib;
using Newtonsoft.Json;
using ScoreSaber.Core.Data;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Data.Wrappers;
using ScoreSaber.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ScoreSaber.Core.Services {
    internal class PlayerService {

        public LocalPlayerInfo localPlayerInfo { get; set; }
        public LoginStatus loginStatus { get; set; }
        public event Action<LoginStatus, string> LoginStatusChanged;
        public enum LoginStatus {
            Info = 0,
            Error = 1,
            Success = 2
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
                if (File.Exists(Path.Combine(IPA.Utilities.UnityGame.InstallPath, "Beat Saber_Data", "Plugins", "x86_64", "steam_api64.dll"))) {
                    GetLocalPlayerInfo1().RunTask();
                } else {
                    GetLocalPlayerInfo2();
                }
            }
        }

        private async Task GetLocalPlayerInfo1() {

            ChangeLoginStatus(LoginStatus.Info, "Signing into ScoreSaber...");

            int attempts = 1;

            while (attempts < 4) {
                LocalPlayerInfo steamInfo = await GetLocalSteamInfo();
                if (steamInfo != null) {
                    bool authenticated = await AuthenticateWithScoreSaber(steamInfo);
                    if (authenticated) {
                        localPlayerInfo = steamInfo;
                        string successText = "Successfully signed into ScoreSaber!";
                        if (localPlayerInfo.playerId == PlayerIDs.Denyah) {
                            successText = "Wagwan piffting wots ur bbm pin?";
                        }
                        ChangeLoginStatus(LoginStatus.Success, successText);
                        break;
                    } else {
                        ChangeLoginStatus(LoginStatus.Error, $"Failed, attempting again ({attempts} of 3 tries...)");
                        attempts++;
                        await Task.Delay(4000);
                    }
                } else {
                    Plugin.Log.Error("Steamworks is not initialized!");
                    ChangeLoginStatus(LoginStatus.Error, "Failed to authenticate! Error getting steam info");
                    break;
                }
            }

            if (loginStatus != LoginStatus.Success) {
                ChangeLoginStatus(LoginStatus.Error, "Failed to authenticate with ScoreSaber! Please restart your game");
            }
        }

        private void GetLocalPlayerInfo2() {

            ChangeLoginStatus(LoginStatus.Info, "Signing into ScoreSaber...");
#if !NO_OCULUS
            Oculus.Platform.Users.GetLoggedInUser().OnComplete(delegate (Oculus.Platform.Message<Oculus.Platform.Models.User> loggedInMessage) {
                if (!loggedInMessage.IsError) {
                    Oculus.Platform.Users.GetLoggedInUserFriends().OnComplete(delegate (Oculus.Platform.Message<Oculus.Platform.Models.UserList> friendsMessage) {
                        if (!friendsMessage.IsError) {
                            Oculus.Platform.Users.GetUserProof().OnComplete(delegate (Oculus.Platform.Message<Oculus.Platform.Models.UserProof> userProofMessage) {
                                if (!userProofMessage.IsError) {
                                    Oculus.Platform.Users.GetAccessToken().OnComplete(async delegate (Oculus.Platform.Message<string> authTokenMessage) {
                                        string playerId = loggedInMessage.Data.ID.ToString();
                                        string playerName = loggedInMessage.Data.OculusID;
                                        string friends = playerId + ",";
                                        string nonce = userProofMessage.Data.Value + "," + authTokenMessage.Data;
                                        LocalPlayerInfo oculusInfo = new LocalPlayerInfo(playerId, playerName, friends, "1", nonce);
                                        bool authenticated = await AuthenticateWithScoreSaber(oculusInfo);
                                        if (authenticated) {
                                            localPlayerInfo = oculusInfo;
                                            ChangeLoginStatus(LoginStatus.Success, "Successfully signed into ScoreSaber!");
                                        } else {
                                            ChangeLoginStatus(LoginStatus.Error, "Failed to authenticate with ScoreSaber! Please restart your game");
                                        }
                                    });
                                   
                                } else {
                                    ChangeLoginStatus(LoginStatus.Error, "Failed to authenticate! Error getting oculus info");
                                }
                            });
                        } else {
                            ChangeLoginStatus(LoginStatus.Error, "Failed to authenticate! Error getting oculus info");
                        }
                    });
                } else {
                    ChangeLoginStatus(LoginStatus.Error, "Failed to authenticate! Error getting oculus info");
                }
            });
#endif
        }

        private async Task<LocalPlayerInfo> GetLocalSteamInfo() {
            var platformUserModel = Plugin.Container.TryResolve<IPlatformUserModel>();
            var authToken = await platformUserModel.GetUserAuthToken();
            var userInfo = await platformUserModel.GetUserInfo(new CancellationToken());

            string playerId = userInfo.platformUserId;
            string playerName = userInfo.userName;
            var friendIds = await platformUserModel.GetUserFriendsUserIds(false);
            string friends = string.Join(",", friendIds.Where(x => x != "0"));
            return new LocalPlayerInfo(playerId, playerName, friends, "0", authToken.token);
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

        public async Task<byte[]> GetReplayData(IDifficultyBeatmap level, int leaderboardId, ScoreMap scoreMap) {

            if (scoreMap.hasLocalReplay) {
                string replayPath = GetReplayPath(scoreMap.parent.songHash, level.difficulty.SerializedName(), level.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName, scoreMap.score.leaderboardPlayerInfo.id, level.level.songName);
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