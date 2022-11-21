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
        internal string playerName;
        [JsonProperty("playerId")]
        internal string playerId;
        [JsonProperty("score")]
        internal int score;
        [JsonProperty("leaderboardId")]
        internal string leaderboardId;
        [JsonProperty("songName")]
        internal string songName;
        [JsonProperty("songSubName")]
        internal string songSubName;
        [JsonProperty("levelAuthorName")]
        internal string levelAuthorName;
        [JsonProperty("songAuthorName")]
        internal string songAuthorName;
        [JsonProperty("bpm")]
        internal int bpm;
        [JsonProperty("difficulty")]
        internal int difficulty;
        [JsonProperty("infoHash")]
        internal string infoHash;
        [JsonProperty("modifiers")]
        internal List<string> modifiers;
        [JsonProperty("gameMode")]
        internal string gameMode;
        [JsonProperty("badCutsCount")]
        internal int badCutsCount;
        [JsonProperty("missedCount")]
        internal int missedCount;
        [JsonProperty("maxCombo")]
        internal int maxCombo;
        [JsonProperty("fullCombo")]
        internal bool fullCombo;
        [JsonProperty("hmd")]
        internal int hmd;

        internal static ScoreSaberUploadData Create(IDifficultyBeatmap difficultyBeatmap, LevelCompletionResults results, LocalPlayerInfo playerInfo, string infoHash) {

            string[] levelInfo = difficultyBeatmap.level.levelID.Split('_');

            var data = new ScoreSaberUploadData {
                gameMode = $"Solo{difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}",
                difficulty = difficultyBeatmap.difficulty.DefaultRating(),
                infoHash = infoHash.ToString(),
                leaderboardId = levelInfo[2],
                songName = difficultyBeatmap.level.songName,
                songSubName = difficultyBeatmap.level.songSubName,
                songAuthorName = difficultyBeatmap.level.songAuthorName,
                levelAuthorName = difficultyBeatmap.level.levelAuthorName,
                bpm = Convert.ToInt32(difficultyBeatmap.level.beatsPerMinute),
                playerName = playerInfo.PlayerName,
                playerId = playerInfo.PlayerId,
                badCutsCount = results.badCutsCount,
                missedCount = results.missedCount,
                maxCombo = results.maxCombo,
                fullCombo = results.fullCombo,
                score = results.multipliedScore,
                modifiers = LeaderboardUtils.GetModifierList(results),
                hmd = HMD.Get()
            };

            return data;
        }
    }
}