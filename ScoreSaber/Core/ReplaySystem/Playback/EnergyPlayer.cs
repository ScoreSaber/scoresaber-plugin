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

            if(_gameEnergyUIPanel || !_gameEnergyUIPanel.gameObject.activeSelf) {
                return;
            }

            bool isFailingEnergy = energy <= Mathf.Epsilon;

            bool noFail = _gameEnergyCounter.noFail;
            _gameEnergyCounter.noFail = false;
            _gameEnergyCounter._didReach0Energy = isFailingEnergy;
            _gameEnergyCounter.ProcessEnergyChange(energy);
            _gameEnergyCounter._nextFrameEnergyChange = 0;
            _gameEnergyCounter.energy = energy;
            _gameEnergyCounter.noFail = noFail;

            if (_gameEnergyUIPanel != null) {
                _gameEnergyUIPanel.Init();
                var director = _gameEnergyUIPanel._playableDirector;
                director.Stop();
                director.RebindPlayableGraphOutputs();
                director.Evaluate();
                if(!isFailingEnergy) {
                    _gameEnergyUIPanel.transform.Find("Laser").gameObject.SetActive(false);
                }
                var fullIcon = _gameEnergyUIPanel.transform.Find("EnergyIconFull");
                var emptyIcon = _gameEnergyUIPanel.transform.Find("EnergyIconEmpty");
                fullIcon.transform.localPosition = new Vector3(59, 0);
                emptyIcon.transform.localPosition = new Vector3(-59, 0);
                _gameEnergyCounter.gameEnergyDidChangeEvent += _gameEnergyUIPanel.HandleGameEnergyDidChange;
                _gameEnergyUIPanel._energyBar.enabled = !isFailingEnergy;
            }
            _gameEnergyUIPanel.RefreshEnergyUI(energy);
        }
    }
}