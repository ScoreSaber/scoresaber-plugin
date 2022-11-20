#region

using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Core.Utils;
using System;
using System.Linq;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Playback {
    internal class ScorePlayer : TimeSynchronizer, ITickable, IScroller {
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

            int? recentMultipliedScore = null;
            while (audioTimeSyncController.songTime >= _sortedScoreEvents[_lastIndex].Time) {

                var activeEvent = _sortedScoreEvents[_lastIndex++];
                recentMultipliedScore = Accessors.MultipliedScore(ref _scoreController) = activeEvent.Score;

                if (_lastIndex >= _sortedScoreEvents.Length)
                    break;
            }

            if (recentMultipliedScore.HasValue) {

                var postNoteCount = CalculatePostNoteCountForTime(audioTimeSyncController.songTime);
                Accessors.ImmediatePossible(ref _scoreController) = LeaderboardUtils.MaxRawScoreForNumberOfNotes(postNoteCount);
                FieldAccessor<ScoreController, Action<int, int>>.Get(_scoreController, "scoreDidChangeEvent").Invoke(recentMultipliedScore.Value,
                    ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(recentMultipliedScore.Value, Accessors.GameplayMultiplier(ref _scoreController)));
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

            // TODO: Deal with ScoreModel.MaxRawScoreForNumberOfNotes. Doesn't exist now and the max multiplied score is computed on the fly. We'll need to reimplement the old method for replays that use beatmap v2.

            int postNoteCount = CalculatePostNoteCountForTime(time);
            var totalMultiplier = Accessors.ModifiersModelSO(ref _scoreController).GetTotalMultiplier(Accessors.ModifierPanelsSO(ref _scoreController), _gameEnergyCounter.energy);

            Accessors.GameplayMultiplier(ref _scoreController) = totalMultiplier;

            var immediate = Accessors.ImmediatePossible(ref _scoreController) = LeaderboardUtils.MaxRawScoreForNumberOfNotes(postNoteCount);
            var earlyScore = newScore;
            Accessors.MultipliedScore(ref _scoreController) = earlyScore;

            FieldAccessor<ScoreController, Action<int, int>>.Get(_scoreController, "scoreDidChangeEvent").Invoke(earlyScore,
                ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(earlyScore, totalMultiplier));
        }

        private int CalculatePostNoteCountForTime(float time) {

            return _sortedNoteEvents.TakeWhile(noteEvent => !(noteEvent.Time > time))
                .Select(noteEvent => noteEvent.EventType).Count(eventType =>
                    eventType == NoteEventType.GoodCut || eventType == NoteEventType.BadCut ||
                    eventType == NoteEventType.Miss);
        }
    }
}