#region

using Newtonsoft.Json;
using ScoreSaber.Core.Data.Internal;
using ScoreSaber.Core.Utils;
using System;
using System.Collections.Generic;

#endregion

namespace ScoreSaber.Core.Data.Models {
    internal class ScoreSaberUploadData {
        [JsonProperty("badCutsCount")] internal int badCutsCount;

        [JsonProperty("bpm")] internal int bpm;

        [JsonProperty("difficulty")] internal int difficulty;

        [JsonProperty("fullCombo")] internal bool fullCombo;

        [JsonProperty("gameMode")] internal string gameMode;

        [JsonProperty("hmd")] internal int hmd;

        [JsonProperty("infoHash")] internal string infoHash;

        [JsonProperty("leaderboardId")] internal string leaderboardId;

        [JsonProperty("levelAuthorName")] internal string levelAuthorName;

        [JsonProperty("maxCombo")] internal int maxCombo;

        [JsonProperty("missedCount")] internal int missedCount;

        [JsonProperty("modifiers")] internal List<string> modifiers;

        [JsonProperty("playerId")] internal string playerId;

        [JsonProperty("playerName")] internal string playerName;

        [JsonProperty("score")] internal int score;

        [JsonProperty("songAuthorName")] internal string songAuthorName;

        [JsonProperty("songName")] internal string songName;

        [JsonProperty("songSubName")] internal string songSubName;

        internal static ScoreSaberUploadData Create(object type, object rType, object lType, object iH) {
            ScoreSaberUploadData data = new ScoreSaberUploadData();

            IDifficultyBeatmap difficultyBeatmap = (IDifficultyBeatmap)type;
            LevelCompletionResults results = (LevelCompletionResults)rType;
            LocalPlayerInfo playerInfo = (LocalPlayerInfo)lType;

            string[] levelInfo = difficultyBeatmap.level.levelID.Split('_');
            data.gameMode = $"Solo{difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}";
            data.difficulty = difficultyBeatmap.difficulty.DefaultRating();
            data.infoHash = iH.ToString();
            data.leaderboardId = levelInfo[2];
            data.songName = difficultyBeatmap.level.songName;
            data.songSubName = difficultyBeatmap.level.songSubName;
            data.songAuthorName = difficultyBeatmap.level.songAuthorName;
            data.levelAuthorName = difficultyBeatmap.level.levelAuthorName;
            data.bpm = Convert.ToInt32(difficultyBeatmap.level.beatsPerMinute);

            data.playerName = playerInfo.playerName;
            data.playerId = playerInfo.playerId;

            data.badCutsCount = results.badCutsCount;
            data.missedCount = results.missedCount;
            data.maxCombo = results.maxCombo;
            data.fullCombo = results.fullCombo;

            data.score = results.multipliedScore;
            data.modifiers = LeaderboardUtils.GetModifierList(rType);
            data.hmd = HMD.Get();
            return data;
        }
    }
}