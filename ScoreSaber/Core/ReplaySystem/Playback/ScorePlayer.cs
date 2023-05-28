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
            for (int c = 0; c < _sortedScoreEvents.Length; c++) {
                if (_sortedScoreEvents[c].Time > newTime) {
                    _nextIndex = c;
                    if (c != 0) {
                        UpdateScore(_sortedScoreEvents[c - 1].Score, _sortedScoreEvents[c - 1].ImmediateMaxPossibleScore, newTime);
                    } else {
                        UpdateScore(0, 0, newTime);
                    }
                    return;
                }
            }
            _nextIndex = _sortedScoreEvents.Length;
            var scoreEvent = _sortedScoreEvents.LastOrDefault();
            UpdateScore(scoreEvent.Score, scoreEvent.ImmediateMaxPossibleScore, newTime);
        }

        private void UpdateMultiplier() {

            var totalMultiplier = Accessors.ModifiersModelSO(ref _scoreController).GetTotalMultiplier(Accessors.ModifierPanelsSO(ref _scoreController), _gameEnergyCounter.energy);
            Accessors.GameplayMultiplier(ref _scoreController) = totalMultiplier;
        }

        private void UpdateScore(int newScore, int? immediateMaxPossibleScore, float time) {

            var immediate = immediateMaxPossibleScore ?? LeaderboardUtils.OldMaxRawScoreForNumberOfNotes(CalculatePostNoteCountForTime(time));
            var multiplier = Accessors.GameplayMultiplier(ref _scoreController);

            var newModifiedScore = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(newScore, multiplier);

            Accessors.MultipliedScore(ref _scoreController) = newScore;
            Accessors.ImmediateMultipliedPossible(ref _scoreController) = immediate;
            Accessors.ModifiedScore(ref _scoreController) = newModifiedScore;
            Accessors.ImmediateModifiedPossible(ref _scoreController) = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(immediate, multiplier);

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