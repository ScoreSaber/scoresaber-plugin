using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace ScoreSaber.Core.ReplaySystem.Playback
{
    internal class EnergyPlayer : TimeSynchronizer, IScroller
    {
        private GameEnergyCounter _gameEnergyCounter;
        private GameEnergyUIPanel _gameEnergyUIPanel;
        private readonly EnergyEvent[] _sortedEnergyEvents;

        public EnergyPlayer(ReplayFile file, GameEnergyCounter gameEnergyCounter, DiContainer container) {

            _gameEnergyCounter = gameEnergyCounter;
            _gameEnergyUIPanel = container.TryResolve<GameEnergyUIPanel>();
            _sortedEnergyEvents = file.energyKeyframes.ToArray();
        }

        private const float EPSILON = 0.001f;
        private const float DEBOUNCE_TIME = 0.05f;
        private float _lastUpdateTime = -1f;

        public void TimeUpdate(float newTime) {
            if (Time.time - _lastUpdateTime < DEBOUNCE_TIME) {
                return;
            } 
            _lastUpdateTime = Time.time;
            for (int c = 0; c < _sortedEnergyEvents.Length; c++) {
                if (_sortedEnergyEvents[c].Time - newTime > EPSILON) {
                    float energy = c != 0 ? _sortedEnergyEvents[c - 1].Energy : 0.5f;
                    UpdateEnergy(energy);
                    return;
                }
            }

            UpdateEnergy(0.5f);
            var lastEvent = _sortedEnergyEvents.LastOrDefault();
            if (newTime >= lastEvent.Time && lastEvent.Energy <= Mathf.Epsilon) {
                UpdateEnergy(0f);
            }
        }


        private void UpdateEnergy(float energy) {

            bool isFailingEnergy = energy <= Mathf.Epsilon;

            bool noFail = _gameEnergyCounter.noFail;
            Accessors.NoFailPropertyUpdater(ref _gameEnergyCounter, false);
            Accessors.DidReachZero(ref _gameEnergyCounter) = isFailingEnergy;
            _gameEnergyCounter.ProcessEnergyChange(energy);
            Accessors.NextEnergyChange(ref _gameEnergyCounter) = 0;
            Accessors.ActiveEnergy(ref _gameEnergyCounter, energy);
            Accessors.NoFailPropertyUpdater(ref _gameEnergyCounter, noFail);

            if (_gameEnergyUIPanel != null) {
                _gameEnergyUIPanel.Init();
                var director = Accessors.Director(ref _gameEnergyUIPanel);
                director.Stop();
                director.Evaluate();
                Accessors.EnergyBar(ref _gameEnergyUIPanel).enabled = !isFailingEnergy;
            }

            FieldAccessor<GameEnergyCounter, Action<float>>.Get(_gameEnergyCounter, "gameEnergyDidChangeEvent").Invoke(energy);
        }
    }
}