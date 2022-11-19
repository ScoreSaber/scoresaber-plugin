#region

using Newtonsoft.Json;
using System;

#endregion

namespace ScoreSaber.Core.Data.Models {
    internal class Leaderboard {
        [JsonProperty("leaderboardInfo")] internal LeaderboardInfo leaderboardInfo { get; set; }

        [JsonProperty("scores")] internal Score[] scores { get; set; }
    }

    internal class LeaderboardInfo {
        [JsonProperty("id")] internal int id { get; set; }

        [JsonProperty("songHash")] internal string songHash { get; set; }

        [JsonProperty("songName")] internal string songName { get; set; }

        [JsonProperty("songSubName")] internal string songSubName { get; set; }

        [JsonProperty("songAuthorName")] internal string songAuthorName { get; set; }

        [JsonProperty("levelAuthorName")] internal string levelAuthorName { get; set; }

        [JsonProperty("difficulty")] internal Difficulty difficulty { get; set; }

        [JsonProperty("maxScore")] internal int maxScore { get; set; }

        [JsonProperty("createdDate")] internal DateTimeOffset createdDate { get; set; }

        [JsonProperty("rankedDate")] internal DateTimeOffset? rankedDate { get; set; }

        [JsonProperty("qualifiedDate")] internal DateTimeOffset? qualifiedDate { get; set; }

        [JsonProperty("lovedDate")] internal DateTimeOffset? lovedDate { get; set; }

        [JsonProperty("ranked")] internal bool ranked { get; set; }

        [JsonProperty("qualified")] internal bool qualified { get; set; }

        [JsonProperty("loved")] internal bool loved { get; set; }

        [JsonProperty("maxPP")] internal double maxPP { get; set; }

        [JsonProperty("stars")] internal double stars { get; set; }

        [JsonProperty("plays")] internal int plays { get; set; }

        [JsonProperty("dailyPlays")] internal int dailyPlays { get; set; }

        [JsonProperty("coverImage")] internal string coverImage { get; set; }

        [JsonProperty("positiveModifiers")] internal bool positiveModifiers { get; set; }

        [JsonProperty("playerScore")] internal Score playerScore { get; set; }
    }

    internal class Difficulty {
        [JsonProperty("leaderboardId")] internal int leaderboardId { get; set; }

        [JsonProperty("difficulty")] internal int difficulty { get; set; }

        [JsonProperty("gameMode")] internal string gameMode { get; set; }

        [JsonProperty("difficultyRaw")] internal string difficultyRaw { get; set; }
    }

    internal class Score {
        [JsonProperty("id")] internal int id { get; set; }

        [JsonProperty("leaderboardPlayerInfo")]
        internal LeaderboardPlayer leaderboardPlayerInfo { get; set; }

        [JsonProperty("rank")] internal int rank { get; set; }

        [JsonProperty("baseScore")] internal int baseScore { get; set; }

        [JsonProperty("modifiedScore")] internal int modifiedScore { get; set; }

        [JsonProperty("pp")] internal double pp { get; set; }

        [JsonProperty("weight")] internal double weight { get; set; }

        [JsonProperty("modifiers")] internal string modifiers { get; set; }

        [JsonProperty("multiplier")] internal double multiplier { get; set; }

        [JsonProperty("badCuts")] internal int badCuts { get; set; }

        [JsonProperty("missedNotes")] internal int missedNotes { get; set; }

        [JsonProperty("maxCombo")] internal int maxCombo { get; set; }

        [JsonProperty("fullCombo")] internal bool fullCombo { get; set; }

        [JsonProperty("hmd")] internal int hmd { get; set; }

        [JsonProperty("timeSet")] internal DateTime timeSet { get; set; }

        [JsonProperty("hasReplay")] internal bool hasReplay { get; set; }
    }

    internal class LeaderboardPlayer {
        [JsonProperty("id")] internal string id { get; set; }

        [JsonProperty("name")] internal string name { get; set; }

        [JsonProperty("profilePicture")] internal string profilePicture { get; set; }

        [JsonProperty("country")] internal string country { get; set; }

        [JsonProperty("permissions")] internal int permissions { get; set; }

        [JsonProperty("role")] internal string role { get; set; }
    }
}