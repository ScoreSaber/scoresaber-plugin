using ScoreSaber.Models;
using ScoreSaber.Utilities;
using SiraUtil.Logging;
using Steamworks;
using System.Threading.Tasks;

namespace ScoreSaber.Services.Login;

internal class SteamLoginService : IPlatformLoginService
{
    private readonly SiraLog _siraLog;

    public SteamLoginService(SiraLog siraLog)
    {
        _siraLog = siraLog;
    }

    public async Task<LocalPlatformUserInfo?> LoginAsync()
    {
        int attempts = 1;

        _siraLog.Debug("Starting Steam login");
        LocalPlatformUserInfo? userInfo = null;

        while (4 > attempts)
        {
            _siraLog.Debug($"Attempt number {attempts}");
            await TaskUtilities.WaitUntil(() => SteamManager.Initialized);

            _siraLog.Debug("Fetching auth token...");
            string authToken = (await new SteamPlatformUserModel().GetUserAuthToken()).token;

            _siraLog.Debug("Collecting user info (name, friends, etc)");
            userInfo = await Task.Run(() =>
            {
                CSteamID steamID = SteamUser.GetSteamID();
                string playerId = steamID.m_SteamID.ToString();
                string playerName = SteamFriends.GetPersonaName();
                string friends = playerId + ",";
                for (int i = 0; i < SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagAll); i++)
                {
                    CSteamID friendSteamId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);

                    if (friendSteamId.m_SteamID.ToString() != "0")
                        friends = friends + friendSteamId.m_SteamID.ToString() + ",";
                }
                friends = friends.Remove(friends.Length - 1);
                return new LocalPlatformUserInfo(playerId, playerName, authToken, friends, "0");
            });

            if (userInfo is not null)
                break;
        }

        if (attempts >= 4)
        {
            _siraLog.Error("Tried too many times!");
        }

        return userInfo;
    }
}