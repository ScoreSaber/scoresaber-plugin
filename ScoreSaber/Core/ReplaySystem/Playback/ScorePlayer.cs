using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Core.Utils;
using System;
using System.Linq;
using Zenject;

namespace ScoreSaber.Core.ReplaySystem.Playback {
    internal class ScorePlayer : TimeSynchronizer, ITickable, IScroller
    {
        private int _nextIndex;
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

            int? recentMultipliedScore = null;
            int? recentImmediateMaxPossibleScore = null;
            while (_nextIndex < _sortedScoreEvents.Length && audioTimeSyncController.songTime >= _sortedScoreEvents[_nextIndex].Time) {
                ScoreEvent activeEvent = _sortedScoreEvents[_nextIndex++];
                recentMultipliedScore = activeEvent.Score;
                recentImmediateMaxPossibleScore = activeEvent.ImmediateMaxPossibleScore;
            }

            if (recentMultipliedScore is int score) {
                UpdateScore(score, recentImmediateMaxPossibleScore, audioTimeSyncController.songTime);
            }
        }

        public void TimeUpdate(float newTime) {

            UpdateMultiplier();

            _nextIndex = _sortedScoreEvents.Length;
            for (int c = 0; c < _sortedScoreEvents.Length; c++) {
                if (_sortedScoreEvents[c].Time > newTime) {
                    _nextIndex = c;
                    break;
                }
            }

            if (_nextIndex > 0) {
                var scoreEvent = _sortedScoreEvents[_nextIndex - 1];
                UpdateScore(scoreEvent.Score, scoreEvent.ImmediateMaxPossibleScore, newTime);
            } else {
                UpdateScore(0, 0, newTime);
            }
        }

        private void UpdateMultiplier() {

            var totalMultiplier = _scoreController._gameplayModifiersModel.GetTotalMultiplier(_scoreController._gameplayModifierParams, _gameEnergyCounter.energy);
            _scoreController._prevMultiplierFromModifiers = totalMultiplier;
        }

        private void UpdateScore(int newScore, int? immediateMaxPossibleScore, float time) {

            var immediate = immediateMaxPossibleScore ?? LeaderboardUtils.OldMaxRawScoreForNumberOfNotes(CalculatePostNoteCountForTime(time));
            var multiplier = _scoreController._prevMultiplierFromModifiers;

            var newModifiedScore = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(newScore, multiplier);

            _scoreController._multipliedScore = newScore;
            _scoreController._immediateMaxPossibleMultipliedScore = immediate;
            _scoreController._modifiedScore = newModifiedScore;
            _scoreController._immediateMaxPossibleModifiedScore = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(immediate, multiplier);

            FieldAccessor<ScoreController, Action<int, int>>.Get(_scoreController, "scoreDidChangeEvent").Invoke(newScore, newModifiedScore);
        }

        private int CalculatePostNoteCountForTime(float time) {

            int count = 0;
            foreach (var noteEvent in _sortedNoteEvents) {

                if (noteEvent.Time > time)
                    break;

                var eventType = noteEvent.EventType;
                if (eventType == NoteEventType.GoodCut || eventType == NoteEventType.BadCut || eventType == NoteEventType.Miss)
                    count++;

            }
            return count;
        }
    }
}