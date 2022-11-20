namespace ScoreSaber.Core.Data.Internal {
    internal class LocalPlayerInfo {

        internal string ServerKey { get; set; }
        internal string PlayerId { get; set; }
        internal string PlayerName { get; set; }
        internal string PlayerKey { get; set; }
        internal string PlayerFriends { get; set; }
        internal string PlayerNonce { get; set; }
        internal string AuthType { get; set; }
        internal bool Succeeded { get; set; }

        internal LocalPlayerInfo(string _playerId, string _playerName, string _playerFriends, string _authType, string _playerNonce) {

            PlayerId = _playerId;
            PlayerName = _playerName;
            PlayerFriends = _playerFriends;
            AuthType = _authType;
            PlayerNonce = _playerNonce;
        }

    }
}