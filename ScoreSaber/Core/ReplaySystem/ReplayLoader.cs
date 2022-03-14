using IPA.Utilities.Async;
using ScoreSaber.Core.Daemons;
using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Core.ReplaySystem.Legacy;
using ScoreSaber.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

namespace ScoreSaber.Core.ReplaySystem
{
    internal class ReplayLoader
    {
        private readonly PlayerDataModel _playerDataModel;
        private readonly MenuTransitionsHelper _menuTransitionsHelper;
        private readonly StandardLevelScenesTransitionSetupDataSO _standardLevelScenesTransitionSetupDataSO;
        private readonly ReplayFileReader _replayFileReader;
        public ReplayLoader(PlayerDataModel playerDataModel, MenuTransitionsHelper menuTransitionsHelper) {

            _playerDataModel = playerDataModel;
            _menuTransitionsHelper = menuTransitionsHelper;
            _standardLevelScenesTransitionSetupDataSO = Accessors.StandardLevelScenesTransitionSetupData(ref _menuTransitionsHelper);
            _replayFileReader = new ReplayFileReader();
        }

        public async Task Load(byte[] replay, IDifficultyBeatmap difficultyBeatmap, GameplayModifiers modifiers, string playerName) {

            Plugin.ReplayState.currentLevel = difficultyBeatmap;
            Plugin.ReplayState.currentModifiers = modifiers;
            Plugin.ReplayState.currentPlayerName = playerName;
            if (replay[0] == 93 && replay[1] == 0 && replay[2] == 0 && replay[3] == 128) {
                await LoadLegacyReplay(replay, difficultyBeatmap, modifiers);
            } else {
                ReplayFile replayFile = await LoadReplay(replay);
                await StartReplay(replayFile, difficultyBeatmap);
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private async Task LoadLegacyReplay(byte[] replay, IDifficultyBeatmap difficultyBeatmap, GameplayModifiers gameplayModifiers) {
            await Task.Run(async () => {
                byte[] decompressed = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(replay);
                BinaryFormatter formatter = new BinaryFormatter();
                Z.SavedData replayData = null;
                try {
                    using (var dataStream = new MemoryStream(decompressed)) {
                        replayData = (Z.SavedData)formatter.Deserialize(dataStream);
                    }
                } catch (Exception) {
                    throw new Exception("Failed to deserialize replay!");
                }

                Plugin.ReplayState.legacyKeyframes = await AddFrames(replayData);
                Plugin.ReplayState.isPlaybackEnabled = true;
                Plugin.ReplayState.isLegacyReplay = true;
                PlayerData playerData = _playerDataModel.playerData;
                PlayerSpecificSettings playerSettings = playerData.playerSpecificSettings;
                _standardLevelScenesTransitionSetupDataSO.didFinishEvent -= UploadDaemonHelper.FiveInstance;
                if (gameplayModifiers == null) {
                    gameplayModifiers = new GameplayModifiers();
                }
                _menuTransitionsHelper.StartStandardLevel("Replay", difficultyBeatmap, difficultyBeatmap.level, playerData.overrideEnvironmentSettings, playerData.colorSchemesSettings.GetSelectedColorScheme(), gameplayModifiers, playerSettings, null, "Exit Replay", false, null, null, ReplayEnd);
            });
        }

        private async Task<List<Z.Keyframe>> AddFrames(Z.SavedData replayData) {
            List<Z.Keyframe> _keyframes = new List<Z.Keyframe>(replayData._keyframes.Length);
            await Task.Run(() => {
                if (replayData != null) {
                    for (int i = 0; i < replayData._keyframes.Length; i++) {
                        Z.SavedData.KeyframeSerializable ks = replayData._keyframes[i];
                        Z.Keyframe k = new Z.Keyframe {
                            _pos1 = new Vector3(ks._xPos1, ks._yPos1, ks._zPos1),
                            _pos2 = new Vector3(ks._xPos2, ks._yPos2, ks._zPos2),
                            _pos3 = new Vector3(ks._xPos3, ks._yPos3, ks._zPos3),
                            _rot1 = new Quaternion(ks._xRot1, ks._yRot1, ks._zRot1, ks._wRot1),
                            _rot2 = new Quaternion(ks._xRot2, ks._yRot2, ks._zRot2, ks._wRot2),
                            _rot3 = new Quaternion(ks._xRot3, ks._yRot3, ks._zRot3, ks._wRot3),
                            _time = ks._time,
                            score = ks.score,
                            combo = ks.combo
                        };
                        _keyframes.Add(k);
                    }
                }
            });
            return _keyframes;
        }

        private async Task<ReplayFile> LoadReplay(byte[] replay) {

            var replayFile =  await Task.Run(() => {
                return _replayFileReader.Read(replay);
            });
            return replayFile;
        }


        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private async Task StartReplay(ReplayFile replay, IDifficultyBeatmap difficultyBeatmap) {

            await Task.Run(() => {
                Plugin.ReplayState.isLegacyReplay = false;
                Plugin.ReplayState.isPlaybackEnabled = true;
                Plugin.ReplayState.file = replay;
                PlayerData playerData = _playerDataModel.playerData;
                PlayerSpecificSettings localPlayerSettings = playerData.playerSpecificSettings;
                PlayerSpecificSettings playerSettings = new PlayerSpecificSettings(replay.metadata.LeftHanded, localPlayerSettings.playerHeight, localPlayerSettings.automaticPlayerHeight, localPlayerSettings.sfxVolume, localPlayerSettings.reduceDebris, localPlayerSettings.noTextsAndHuds, localPlayerSettings.noFailEffects, localPlayerSettings.advancedHud, localPlayerSettings.autoRestart, localPlayerSettings.saberTrailIntensity, localPlayerSettings.noteJumpDurationTypeSettings, localPlayerSettings.noteJumpFixedDuration, localPlayerSettings.noteJumpStartBeatOffset, localPlayerSettings.hideNoteSpawnEffect, localPlayerSettings.adaptiveSfx, localPlayerSettings.environmentEffectsFilterDefaultPreset, localPlayerSettings.environmentEffectsFilterExpertPlusPreset);

                _standardLevelScenesTransitionSetupDataSO.didFinishEvent -= UploadDaemonHelper.FiveInstance;
                UnityMainThreadTaskScheduler.Factory.StartNew(() =>  _menuTransitionsHelper.StartStandardLevel("Replay", difficultyBeatmap, difficultyBeatmap.level, playerData.overrideEnvironmentSettings, playerData.colorSchemesSettings.GetSelectedColorScheme(), LeaderboardUtils.GetModifierFromStrings(replay.metadata.Modifiers.ToArray(), false).gameplayModifiers, playerSettings, null, "Exit Replay", false, null, null, ReplayEnd));
            });
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void ReplayEnd(StandardLevelScenesTransitionSetupDataSO standardLevelSceneSetupData, LevelCompletionResults levelCompletionResults) {

            Plugin.ReplayState.isPlaybackEnabled = false;
            if (Plugin.ScoreSubmission) {
               _standardLevelScenesTransitionSetupDataSO.didFinishEvent += UploadDaemonHelper.FiveInstance;
            }
        }
    }
}