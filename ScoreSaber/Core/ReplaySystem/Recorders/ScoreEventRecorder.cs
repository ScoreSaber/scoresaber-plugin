using ScoreSaber.Core.ReplaySystem.Data;
using System;
using System.Collections.Generic;
using Zenject;

namespace ScoreSaber.Core.ReplaySystem.Recorders
{
    internal class ScoreEventRecorder : TimeSynchronizer, IInitializable, IDisposable
    {
        private readonly ScoreController _scoreController;
        private readonly List<ScoreEvent> _scoreKeyframes;
        private readonly List<ComboEvent> _comboKeyframes;
        private readonly List<MultiplierEvent> _multiplierKeyframes;

        public ScoreEventRecorder(ScoreController scoreController) {

            _scoreController = scoreController;
            _scoreKeyframes = new List<ScoreEvent>();
            _comboKeyframes = new List<ComboEvent>();
            _multiplierKeyframes = new List<MultiplierEvent>();
        }

        public void Initialize() {

            _scoreController.comboDidChangeEvent += ScoreController_comboDidChangeEvent;
            _scoreController.scoreDidChangeEvent += ScoreController_scoreDidChangeEvent;
            _scoreController.multiplierDidChangeEvent += ScoreController_multiplierDidChangeEvent;
        }

        public void Dispose() {

            _scoreController.comboDidChangeEvent -= ScoreController_comboDidChangeEvent;
            _scoreController.scoreDidChangeEvent -= ScoreController_scoreDidChangeEvent;
            _scoreController.multiplierDidChangeEvent -= ScoreController_multiplierDidChangeEvent;
        }

        private void ScoreController_scoreDidChangeEvent(int rawScore, int score) {

            _scoreKeyframes.Add(new ScoreEvent() { Score = rawScore, Time = audioTimeSyncController.songTime });
        }

        private void ScoreController_comboDidChangeEvent(int combo) {

            _comboKeyframes.Add(new ComboEvent() { Combo = combo, Time = audioTimeSyncController.songTime });
        }

        private void ScoreController_multiplierDidChangeEvent(int multiplier, float nextMultiplierProgress) {

            _multiplierKeyframes.Add(new MultiplierEvent() {
                Multiplier = multiplier,
                NextMultiplierProgress = nextMultiplierProgress,
                Time = audioTimeSyncController.songTime
            });
        }

        public List<ScoreEvent> ExportScoreKeyframes() {

            return _scoreKeyframes;
        }

        public List<ComboEvent> ExportComboKeyframes() {

            return _comboKeyframes;
        }

        public List<MultiplierEvent> ExportMultiplierKeyframes() {

            return _multiplierKeyframes;
        }

    }
}
