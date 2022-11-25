#region

using Newtonsoft.Json;
using ScoreSaber.Core.Data.Internal;
using ScoreSaber.Core.Utils;
using System;
using System.Collections.Generic;

#endregion

namespace ScoreSaber.Core.Data.Models {

    internal class ScoreSaberUploadData {
        [JsonProperty("playerName")]
        internal string PlayerName;
        [JsonProperty("playerId")]
        internal string PlayerId;
        [JsonProperty("score")]
        internal int Score;
        [JsonProperty("leaderboardId")]
        internal string LeaderboardId;
        [JsonProperty("songName")]
        internal string SongName;
        [JsonProperty("songSubName")]
        internal string SongSubName;
        [JsonProperty("levelAuthorName")]
        internal string LevelAuthorName;
        [JsonProperty("songAuthorName")]
        internal string SongAuthorName;
        [JsonProperty("bpm")]
        internal int BPM;
        [JsonProperty("difficulty")]
        internal int Difficulty;
        [JsonProperty("infoHash")]
        internal string InfoHash;
        [JsonProperty("modifiers")]
        internal List<string> Modifiers;
        [JsonProperty("gameMode")]
        internal string GameMode;
        [JsonProperty("badCutsCount")]
        internal int BadCutsCount;
        [JsonProperty("missedCount")]
        internal int MissedCount;
        [JsonProperty("maxCombo")]
        internal int MaxCombo;
        [JsonProperty("fullCombo")]
        internal bool FullCombo;
        [JsonProperty("hmd")]
        internal int HMD;

        /// <summary>
        /// This method create an ScoreSaberUploadData object and fill it with the current session raw data.
        /// </summary>
        internal static ScoreSaberUploadData Create(IDifficultyBeatmap difficultyBeatmap, LevelCompletionResults results, LocalPlayerInfo playerInfo, string infoHash) {

            string[] levelInfo = difficultyBeatmap.level.levelID.Split('_');

            var data = new ScoreSaberUploadData {
                GameMode = $"Solo{difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}",
                Difficulty = difficultyBeatmap.difficulty.DefaultRating(),
                InfoHash = infoHash.ToString(),
                LeaderboardId = levelInfo[2],
                SongName = difficultyBeatmap.level.songName,
                SongSubName = difficultyBeatmap.level.songSubName,
                SongAuthorName = difficultyBeatmap.level.songAuthorName,
                LevelAuthorName = difficultyBeatmap.level.levelAuthorName,
                BPM = Convert.ToInt32(difficultyBeatmap.level.beatsPerMinute),
                PlayerName = playerInfo.PlayerName,
                PlayerId = playerInfo.PlayerId,
                BadCutsCount = results.badCutsCount,
                MissedCount = results.missedCount,
                MaxCombo = results.maxCombo,
                FullCombo = results.fullCombo,
                Score = results.multipliedScore,
                Modifiers = LeaderboardUtils.GetModifierList(results),
                HMD = Internal.HMD.Get()
            };

            return data;
        }
    }
}