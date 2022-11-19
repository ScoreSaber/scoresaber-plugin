#region

using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Playback {
    internal class EnergyPlayer : TimeSynchronizer, IScroller {
        private readonly EnergyEvent[] _sortedEnergyEvents;
        private GameEnergyCounter _gameEnergyCounter;
        private GameEnergyUIPanel _gameEnergyUIPanel;

        public EnergyPlayer(ReplayFile file, GameEnergyCounter gameEnergyCounter, GameEnergyUIPanel gameEnergyUIPanel) {
            _gameEnergyCounter = gameEnergyCounter;
            _gameEnergyUIPanel = gameEnergyUIPanel;
            _sortedEnergyEvents = file.energyKeyframes.ToArray();
        }

        public void TimeUpdate(float newTime) {
            if (_gameEnergyUIPanel == null) { return; }

            for (int c = 0; c < _sortedEnergyEvents.Length; c++) {
                switch (_sortedEnergyEvents[c].Time >= newTime) {
                    case true: {
                        float energy = c != 0 ? _sortedEnergyEvents[c - 1].Energy : 0.5f;
                        UpdateEnergy(energy);
                        return;
                    }
                }
            }

            UpdateEnergy(0.5f);
            EnergyEvent lastEvent = _sortedEnergyEvents.LastOrDefault();
            switch (newTime >= lastEvent.Time) {
                case true when lastEvent.Energy <= Mathf.Epsilon:
                    UpdateEnergy(0f);
                    break;
            }
        }

        private void UpdateEnergy(float energy) {
            if (_gameEnergyUIPanel == null) { return; }

            bool isFailingEnergy = energy <= Mathf.Epsilon;

            bool noFail = _gameEnergyCounter.noFail;
            Accessors.NoFailPropertyUpdater(ref _gameEnergyCounter, false);
            Accessors.DidReachZero(ref _gameEnergyCounter) = isFailingEnergy;
            _gameEnergyCounter.ProcessEnergyChange(energy);
            Accessors.NextEnergyChange(ref _gameEnergyCounter) = 0;
            Accessors.ActiveEnergy(ref _gameEnergyCounter, energy);
            Accessors.NoFailPropertyUpdater(ref _gameEnergyCounter, noFail);

            _gameEnergyUIPanel.Init();
            PlayableDirector director = Accessors.Director(ref _gameEnergyUIPanel);
            director.Stop();
            director.Evaluate();
            Accessors.EnergyBar(ref _gameEnergyUIPanel).enabled = !isFailingEnergy;

            FieldAccessor<GameEnergyCounter, Action<float>>.Get(_gameEnergyCounter, "gameEnergyDidChangeEvent")
                .Invoke(energy);
        }
    }
}