#region

using ScoreSaber.Core.ReplaySystem.Data;
using System;
using System.Collections.Generic;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Recorders {
    internal class EnergyEventRecorder : TimeSynchronizer, IInitializable, IDisposable {
        private readonly List<EnergyEvent> _energyKeyframes;
        private readonly IGameEnergyCounter _gameEnergyCounter;

        public EnergyEventRecorder(IGameEnergyCounter gameEnergyCounter) {
            _gameEnergyCounter = gameEnergyCounter;
            _energyKeyframes = new List<EnergyEvent>();
        }

        public void Dispose() {
            if (_gameEnergyCounter != null) {
                _gameEnergyCounter.gameEnergyDidChangeEvent -= GameEnergyCounter_gameEnergyDidChangeEvent;
            }
        }

        public void Initialize() {
            if (_gameEnergyCounter != null) {
                _gameEnergyCounter.gameEnergyDidChangeEvent += GameEnergyCounter_gameEnergyDidChangeEvent;
            }
        }

        private void GameEnergyCounter_gameEnergyDidChangeEvent(float energy) {
            _energyKeyframes.Add(new EnergyEvent { Energy = energy, Time = audioTimeSyncController.songTime });
        }

        public List<EnergyEvent> Export() {
            return _energyKeyframes;
        }
    }
}