namespace ScoreSaber.Core.Utils {
    public static class SuccessText {
        public static string Get(string playerId) {
            switch (playerId) {
                case PlayerIDs.Denyah:
                    return "Wagwan piffting wots ur bbm pin?";
                case PlayerIDs.Checksum:
                    return "u r cute";
                case PlayerIDs.Loloppe:
                    return "I love you";
                case PlayerIDs.Cytex727:
                    return "meow";
                case PlayerIDs.Dying:
                    return "helpimdying x checksum";
                case PlayerIDs.Spear:
                    return "\"Who’s joe?\" a distant voice asks.";
                case PlayerIDs.MakoCho:
                    return "SillyChamp";
                default:
                    return "Successfully signed into ScoreSaber!";
            }
        }
    }
}