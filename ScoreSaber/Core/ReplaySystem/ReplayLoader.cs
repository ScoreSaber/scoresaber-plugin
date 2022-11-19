#region

using IPA.Utilities.Async;
using ScoreSaber.Core.Daemons;
using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Core.Utils;
using ScoreSaber.Libraries.SevenZip.Compress.LzmaAlone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

#endregion

namespace ScoreSaber.Core.ReplaySystem {
    internal class ReplayLoader {
        private readonly MenuTransitionsHelper _menuTransitionsHelper;

        private readonly PlayerDataModel _playerDataModel;
        private readonly ReplayFileReader _replayFileReader;
        private readonly StandardLevelScenesTransitionSetupDataSO _standardLevelScenesTransitionSetupDataSO;

        public ReplayLoader(PlayerDataModel playerDataModel, MenuTransitionsHelper menuTransitionsHelper) {
            _playerDataModel = playerDataModel;
            _menuTransitionsHelper = menuTransitionsHelper;
            _standardLevelScenesTransitionSetupDataSO =
                Accessors.StandardLevelScenesTransitionSetupData(ref _menuTransitionsHelper);
            _replayFileReader = new ReplayFileReader();
        }

        public async Task Load(byte[] replay, IDifficultyBeatmap difficultyBeatmap, GameplayModifiers modifiers,
            string playerName) {
            Plugin.ReplayState.CurrentLevel = difficultyBeatmap;
            Plugin.ReplayState.CurrentModifiers = modifiers;
            Plugin.ReplayState.CurrentPlayerName = playerName;
            switch (replay[0]) {
                case 93 when replay[1] == 0 && replay[2] == 0 && replay[3] == 128:
                    await LoadLegacyReplay(replay, difficultyBeatmap, modifiers);
                    break;
                default: {
                    ReplayFile replayFile = await LoadReplay(replay);
                    await StartReplay(replayFile, difficultyBeatmap);
                    break;
                }
            }
        }

        private async Task LoadLegacyReplay(byte[] replay, IDifficultyBeatmap difficultyBeatmap,
            GameplayModifiers gameplayModifiers) {
            await Task.Run(async () => {
                byte[] decompressed = SevenZipHelper.Decompress(replay);
                BinaryFormatter formatter = new BinaryFormatter();
                LegacyReplayFile.SavedData replayData = null;
                try {
                    using (MemoryStream dataStream = new MemoryStream(decompressed)) {
                        replayData = (LegacyReplayFile.SavedData)formatter.Deserialize(dataStream);
                    }
                } catch (Exception) {
                    throw new Exception("Failed to deserialize replay!");
                }

                Plugin.ReplayState.LoadedLegacyKeyframes = await AddFrames(replayData);
                Plugin.ReplayState.IsPlaybackEnabled = true;
                Plugin.ReplayState.IsLegacyReplay = true;
                PlayerData playerData = _playerDataModel.playerData;
                PlayerSpecificSettings playerSettings = playerData.playerSpecificSettings;
                _standardLevelScenesTransitionSetupDataSO.didFinishEvent -= UploadDaemonHelper.ThreeInstance;
                switch (gameplayModifiers) {
                    case null:
                        gameplayModifiers = new GameplayModifiers();
                        break;
                }

                _menuTransitionsHelper.StartStandardLevel("Replay", difficultyBeatmap, difficultyBeatmap.level,
                    playerData.overrideEnvironmentSettings,
                    playerData.colorSchemesSettings.GetSelectedColorScheme(), gameplayModifiers, playerSettings, null,
                    "Exit Replay", false, false, null, ReplayEnd, null);
            });
        }

        private async Task<List<LegacyReplayFile.Keyframe>> AddFrames(LegacyReplayFile.SavedData replayData) {
            List<LegacyReplayFile.Keyframe> _keyframes =
                new List<LegacyReplayFile.Keyframe>(replayData._keyframes.Length);
            await Task.Run(() => {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (replayData == null) {
                    return;
                }

                _keyframes.AddRange(replayData._keyframes.Select(ks => new LegacyReplayFile.Keyframe {
                    _pos1 = new Vector3(ks._xPos1, ks._yPos1, ks._zPos1),
                    _pos2 = new Vector3(ks._xPos2, ks._yPos2, ks._zPos2),
                    _pos3 = new Vector3(ks._xPos3, ks._yPos3, ks._zPos3),
                    _rot1 = new Quaternion(ks._xRot1, ks._yRot1, ks._zRot1, ks._wRot1),
                    _rot2 = new Quaternion(ks._xRot2, ks._yRot2, ks._zRot2, ks._wRot2),
                    _rot3 = new Quaternion(ks._xRot3, ks._yRot3, ks._zRot3, ks._wRot3),
                    _time = ks._time,
                    score = ks.score,
                    combo = ks.combo
                }));
            });
            return _keyframes;
        }

        private async Task<ReplayFile> LoadReplay(byte[] replay) {
            ReplayFile replayFile = await Task.Run(() => _replayFileReader.Read(replay));
            return replayFile;
        }


        private async Task StartReplay(ReplayFile replay, IDifficultyBeatmap difficultyBeatmap) {
            await Task.Run(() => {
                Plugin.ReplayState.IsLegacyReplay = false;
                Plugin.ReplayState.IsPlaybackEnabled = true;
                Plugin.ReplayState.LoadedReplayFile = replay;
                PlayerData playerData = _playerDataModel.playerData;
                PlayerSpecificSettings localPlayerSettings = playerData.playerSpecificSettings;
                PlayerSpecificSettings playerSettings = new PlayerSpecificSettings(replay.metadata.LeftHanded,
                    localPlayerSettings.playerHeight, localPlayerSettings.automaticPlayerHeight,
                    localPlayerSettings.sfxVolume, localPlayerSettings.reduceDebris, localPlayerSettings.noTextsAndHuds,
                    localPlayerSettings.noFailEffects, localPlayerSettings.advancedHud, localPlayerSettings.autoRestart,
                    localPlayerSettings.saberTrailIntensity, localPlayerSettings.noteJumpDurationTypeSettings,
                    localPlayerSettings.noteJumpFixedDuration, localPlayerSettings.noteJumpStartBeatOffset,
                    localPlayerSettings.hideNoteSpawnEffect, localPlayerSettings.adaptiveSfx,
                    localPlayerSettings.environmentEffectsFilterDefaultPreset,
                    localPlayerSettings.environmentEffectsFilterExpertPlusPreset);

                _standardLevelScenesTransitionSetupDataSO.didFinishEvent -= UploadDaemonHelper.ThreeInstance;
                UnityMainThreadTaskScheduler.Factory.StartNew(() => _menuTransitionsHelper.StartStandardLevel("Replay",
                    difficultyBeatmap, difficultyBeatmap.level,
                    playerData.overrideEnvironmentSettings, playerData.colorSchemesSettings.GetSelectedColorScheme(),
                    LeaderboardUtils.GetModifierFromStrings(replay.metadata.Modifiers.ToArray(), false)
                        .gameplayModifiers,
                    playerSettings, null, "Exit Replay", false, false, null, ReplayEnd, null));
            });
        }

        private void ReplayEnd(StandardLevelScenesTransitionSetupDataSO standardLevelSceneSetupData,
            LevelCompletionResults levelCompletionResults) {
            Plugin.ReplayState.IsPlaybackEnabled = false;
            if (Plugin.ScoreSubmission) {
                _standardLevelScenesTransitionSetupDataSO.didFinishEvent += UploadDaemonHelper.ThreeInstance;
            }
        }
    }
}