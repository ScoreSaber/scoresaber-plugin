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

        public void TimeUpdate(float newTime) {
            if (_sortedEnergyEvents.Length == 0) {
                UpdateEnergy(1.0f);
                return;
            }

            for (int c = 0; c < _sortedEnergyEvents.Length; c++) {
                if (_sortedEnergyEvents[c].Time > newTime) {
                    float energy = c != 0 ? _sortedEnergyEvents[c - 1].Energy : _sortedEnergyEvents[0].Energy;
                    UpdateEnergy(energy);
                    return;
                }
            }

            UpdateEnergy(_sortedEnergyEvents.Last().Energy);
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
                director.RebindPlayableGraphOutputs();
                director.Evaluate();
                Accessors.EnergyBar(ref _gameEnergyUIPanel).enabled = !isFailingEnergy;
            }
            _gameEnergyUIPanel.RefreshEnergyUI(energy);
        }
    }
}