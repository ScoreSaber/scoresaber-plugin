using Newtonsoft.Json;
using ScoreSaber.Core.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

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

        internal static ScoreSaberUploadData Create(object type, object rType, object lType, object iH) {

            ScoreSaberUploadData data = new ScoreSaberUploadData();

            IDifficultyBeatmap difficultyBeatmap = (IDifficultyBeatmap)type;
            LevelCompletionResults results = (LevelCompletionResults)rType;
            LocalPlayerInfo playerInfo = (LocalPlayerInfo)lType;

            string[] levelInfo = difficultyBeatmap.level.levelID.Split('_');
            data.gameMode = $"Solo{difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}";
            data.difficulty = BeatmapDifficultyMethods.DefaultRating(difficultyBeatmap.difficulty);
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