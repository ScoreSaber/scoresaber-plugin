#region

using ScoreSaber.Core.ReplaySystem.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Recorders {
    internal class MetadataRecorder : TimeSynchronizer, IInitializable, IDisposable {
        private readonly BeatmapObjectSpawnController.InitData _beatmapObjectSpawnControllerInitData;
        private readonly IGameEnergyCounter _gameEnergyCounter;
        private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        private readonly MainSettingsModelSO _mainSettingsModelSO;
        private float _failTime;

        public MetadataRecorder(GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
            BeatmapObjectSpawnController.InitData beatmapObjectSpawnControllerInitData,
            IGameEnergyCounter gameEnergyCounter) {
            _beatmapObjectSpawnControllerInitData = beatmapObjectSpawnControllerInitData;
            _mainSettingsModelSO = Resources.FindObjectsOfTypeAll<MainSettingsModelSO>()[0];
            _gameEnergyCounter = gameEnergyCounter;
            _gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
        }

        public void Dispose() {
            _gameEnergyCounter.gameEnergyDidReach0Event -= GameEnergyCounter_gameEnergyDidReach0Event;
        }


        public void Initialize() {
            _gameEnergyCounter.gameEnergyDidReach0Event += GameEnergyCounter_gameEnergyDidReach0Event;
        }


        private void GameEnergyCounter_gameEnergyDidReach0Event() {
            _failTime = audioTimeSyncController.songTime;
        }

        public Metadata Export() {
            VRPosition roomCenter = new VRPosition {
                X = _mainSettingsModelSO.roomCenter.value.x,
                Y = _mainSettingsModelSO.roomCenter.value.y,
                Z = _mainSettingsModelSO.roomCenter.value.z
            };

            return new Metadata {
                Version = "2.0.0",
                LevelID = _gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID,
                Difficulty = _gameplayCoreSceneSetupData.difficultyBeatmap.difficulty.DefaultRating(),
                Characteristic = _gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet
                    .beatmapCharacteristic.serializedName,
                Environment = _gameplayCoreSceneSetupData.environmentInfo.serializedName,
                Modifiers = GetModifierList(_gameplayCoreSceneSetupData.gameplayModifiers),
                NoteSpawnOffset = _beatmapObjectSpawnControllerInitData.noteJumpValue,
                LeftHanded = _gameplayCoreSceneSetupData.playerSpecificSettings.leftHanded,
                InitialHeight = _gameplayCoreSceneSetupData.playerSpecificSettings.playerHeight,
                RoomRotation = _mainSettingsModelSO.roomRotation,
                RoomCenter = roomCenter,
                FailTime = _failTime
            };
        }

        public string[] GetModifierList(GameplayModifiers modifiers) {
            List<string> result = new List<string>();
            switch (modifiers.energyType) {
                case GameplayModifiers.EnergyType.Battery:
                    result.Add("BE");
                    break;
            }

            switch (modifiers.noFailOn0Energy) {
                case true:
                    result.Add("NF");
                    break;
            }

            switch (modifiers.instaFail) {
                case true:
                    result.Add("IF");
                    break;
            }

            switch (modifiers.failOnSaberClash) {
                case true:
                    result.Add("SC");
                    break;
            }

            switch (modifiers.enabledObstacleType) {
                case GameplayModifiers.EnabledObstacleType.NoObstacles:
                    result.Add("NO");
                    break;
            }

            switch (modifiers.noBombs) {
                case true:
                    result.Add("NB");
                    break;
            }

            switch (modifiers.strictAngles) {
                case true:
                    result.Add("SA");
                    break;
            }

            switch (modifiers.disappearingArrows) {
                case true:
                    result.Add("DA");
                    break;
            }

            switch (modifiers.ghostNotes) {
                case true:
                    result.Add("GN");
                    break;
            }

            switch (modifiers.songSpeed) {
                case GameplayModifiers.SongSpeed.Slower:
                    result.Add("SS");
                    break;
                case GameplayModifiers.SongSpeed.Faster:
                    result.Add("FS");
                    break;
                case GameplayModifiers.SongSpeed.SuperFast:
                    result.Add("SF");
                    break;
            }

            switch (modifiers.smallCubes) {
                case true:
                    result.Add("SC");
                    break;
            }

            switch (modifiers.strictAngles) {
                case true:
                    result.Add("SA");
                    break;
            }

            switch (modifiers.proMode) {
                case true:
                    result.Add("PM");
                    break;
            }

            switch (modifiers.noArrows) {
                case true:
                    result.Add("NA");
                    break;
            }

            return result.ToArray();
        }
    }
}