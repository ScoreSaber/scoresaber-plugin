using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using System;
using System.Linq;
using Zenject;

namespace ScoreSaber.Core.ReplaySystem.Playback
{
    internal class ScorePlayer : TimeSynchronizer, ITickable, IScroller
    {
        private int _lastIndex;
        private ScoreController _scoreController;
        private readonly NoteEvent[] _sortedNoteEvents;
        private readonly ScoreEvent[] _sortedScoreEvents;
        private readonly IGameEnergyCounter _gameEnergyCounter;

        public ScorePlayer(ReplayFile file, ScoreController scoreController, IGameEnergyCounter gameEnergyCounter) {

            _scoreController = scoreController;
            _gameEnergyCounter = gameEnergyCounter;
            _sortedScoreEvents = file.scoreKeyframes.ToArray();
            _sortedNoteEvents = file.noteKeyframes.OrderBy(nk => nk.NoteID.Time).ToArray();
        }

        public void Tick() {

            if (_lastIndex >= _sortedScoreEvents.Length)
                return;

            while (audioTimeSyncController.songTime >= _sortedScoreEvents[_lastIndex].Time) {

                ScoreEvent activeEvent = _sortedScoreEvents[_lastIndex++];
                Accessors.RawScore(ref _scoreController) = activeEvent.Score;

                if (_lastIndex >= _sortedScoreEvents.Length)
                    break;
            }
        }

        public void TimeUpdate(float newTime) {

            for (int c = 0; c < _sortedScoreEvents.Length; c++) {
                if (_sortedScoreEvents[c].Time >= newTime) {
                    _lastIndex = c;
                    Tick();
                    UpdateScore(c != 0 ? _sortedScoreEvents[c - 1].Score : 0, newTime);
                    return;
                }
            }
            UpdateScore(_sortedScoreEvents.LastOrDefault().Score, newTime);
        }

        private void UpdateScore(int newScore, float time) {

            int cutOrMissRecorded = _sortedNoteEvents.Count(ne => (ne.EventType == NoteEventType.GoodCut || ne.EventType == NoteEventType.BadCut || ne.EventType == NoteEventType.Miss) && time > ne.Time);
            Accessors.CutOrMissedNotes(ref _scoreController) = cutOrMissRecorded;
            Accessors.PrevModifierScore(ref _scoreController) = Accessors.ModifiersModelSO(ref _scoreController).GetTotalMultiplier(Accessors.ModifierPanelsSO(ref _scoreController), _gameEnergyCounter.energy);

            var immediate = Accessors.ImmediatePossible(ref _scoreController) = ScoreModel.MaxRawScoreForNumberOfNotes(cutOrMissRecorded);
            var earlyScore = newScore;
            Accessors.PrevRawScore(ref _scoreController) = earlyScore;
            Accessors.RawScore(ref _scoreController) = earlyScore;

            FieldAccessor<ScoreController, Action<int, int>>.Get(_scoreController, "immediateMaxPossibleScoreDidChangeEvent").Invoke(immediate,
                ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(immediate, _scoreController.gameplayModifiersScoreMultiplier));

            FieldAccessor<ScoreController, Action<int, int>>.Get(_scoreController, "scoreDidChangeEvent").Invoke(earlyScore,
                ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(earlyScore, _scoreController.gameplayModifiersScoreMultiplier));
        }
    }
}