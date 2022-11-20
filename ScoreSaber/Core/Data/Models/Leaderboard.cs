#region

using Newtonsoft.Json;
using System;

#endregion

namespace ScoreSaber.Core.Data.Models {

    internal class Leaderboard {
        [JsonProperty("leaderboardInfo")]
        internal LeaderboardInfo LeaderboardInfo { get; set; }
        [JsonProperty("scores")]
        internal Score[] Scores { get; set; }
    }

    internal class LeaderboardInfo {
        [JsonProperty("id")]
        internal int Id { get; set; }
        [JsonProperty("songHash")]
        internal string SongHash { get; set; }
        [JsonProperty("songName")]
        internal string SongName { get; set; }
        [JsonProperty("songSubName")]
        internal string SongSubName { get; set; }
        [JsonProperty("songAuthorName")]
        internal string SongAuthorName { get; set; }
        [JsonProperty("levelAuthorName")]
        internal string LevelAuthorName { get; set; }
        [JsonProperty("difficulty")]
        internal Difficulty Difficulty { get; set; }
        [JsonProperty("maxScore")]
        internal int MaxScore { get; set; }
        [JsonProperty("createdDate")]
        internal DateTimeOffset CreatedDate { get; set; }
        [JsonProperty("rankedDate")]
        internal DateTimeOffset? RankedDate { get; set; }
        [JsonProperty("qualifiedDate")]
        internal DateTimeOffset? QualifiedDate { get; set; }
        [JsonProperty("lovedDate")]
        internal DateTimeOffset? LovedDate { get; set; }
        [JsonProperty("ranked")]
        internal bool Ranked { get; set; }
        [JsonProperty("qualified")]
        internal bool Qualified { get; set; }
        [JsonProperty("loved")]
        internal bool Loved { get; set; }
        [JsonProperty("maxPP")]
        internal double MaxPP { get; set; }
        [JsonProperty("stars")]
        internal double Stars { get; set; }
        [JsonProperty("plays")]
        internal int Plays { get; set; }
        [JsonProperty("dailyPlays")]
        internal int DailyPlays { get; set; }
        [JsonProperty("coverImage")]
        internal string CoverImage { get; set; }
        [JsonProperty("positiveModifiers")]
        internal bool PositiveModifiers { get; set; }
        [JsonProperty("playerScore")]
        internal Score PlayerScore { get; set; }
    }

    internal class Difficulty {
        [JsonProperty("leaderboardId")]
        internal int LeaderboardId { get; set; }
        [JsonProperty("difficulty")]
        internal int Diff { get; set; }
        [JsonProperty("gameMode")]
        internal string GameMode { get; set; }
        [JsonProperty("difficultyRaw")]
        internal string DifficultyRaw { get; set; }
    }

    internal class Score {
        [JsonProperty("id")]
        internal int Id { get; set; }
        [JsonProperty("leaderboardPlayerInfo")]
        internal LeaderboardPlayer LeaderboardPlayerInfo { get; set; }
        [JsonProperty("rank")]
        internal int Rank { get; set; }
        [JsonProperty("baseScore")]
        internal int BaseScore { get; set; }
        [JsonProperty("modifiedScore")]
        internal int ModifiedScore { get; set; }
        [JsonProperty("pp")]
        internal double PP { get; set; }
        [JsonProperty("weight")]
        internal double Weight { get; set; }
        [JsonProperty("modifiers")]
        internal string Modifiers { get; set; }
        [JsonProperty("multiplier")]
        internal double Multiplier { get; set; }
        [JsonProperty("badCuts")]
        internal int BadCuts { get; set; }
        [JsonProperty("missedNotes")]
        internal int MissedNotes { get; set; }
        [JsonProperty("maxCombo")]
        internal int MaxCombo { get; set; }
        [JsonProperty("fullCombo")]
        internal bool FullCombo { get; set; }
        [JsonProperty("hmd")]
        internal int Hmd { get; set; }
        [JsonProperty("timeSet")]
        internal DateTime TimeSet { get; set; }
        [JsonProperty("hasReplay")]
        internal bool HasReplay { get; set; }
    }

    internal class LeaderboardPlayer {
        [JsonProperty("id")]
        internal string Id { get; set; }
        [JsonProperty("name")]
        internal string Name { get; set; }
        [JsonProperty("profilePicture")]
        internal string ProfilePicture { get; set; }
        [JsonProperty("country")]
        internal string Country { get; set; }
        [JsonProperty("permissions")]
        internal int Permissions { get; set; }
        [JsonProperty("role")]
        internal string Role { get; set; }
    }
}