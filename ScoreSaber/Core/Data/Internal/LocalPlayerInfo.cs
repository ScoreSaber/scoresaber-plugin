namespace ScoreSaber.Core.Data.Internal {
    internal class LocalPlayerInfo {
        internal LocalPlayerInfo(string _playerId, string _playerName, string _playerFriends, string _authType,
            string _playerNonce) {
            playerId = _playerId;
            playerName = _playerName;
            playerFriends = _playerFriends;
            authType = _authType;
            playerNonce = _playerNonce;
        }

        internal string serverKey { get; set; }
        internal string playerId { get; set; }
        internal string playerName { get; set; }
        internal string playerKey { get; set; }
        internal string playerFriends { get; set; }
        internal string playerNonce { get; set; }
        internal string authType { get; set; }
        internal bool succeeded { get; set; }
    }
}