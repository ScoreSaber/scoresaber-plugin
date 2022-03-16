using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Core.Utils;
using SiraUtil.Logging;
using System;
using System.Linq;
using Zenject;

namespace ScoreSaber.Core.ReplaySystem.Playback
{
    internal class ScorePlayer : TimeSynchronizer, ITickable, IScroller
    {
        private int _lastIndex;
        private readonly SiraLog _siraLog;
        private ScoreController _scoreController;
        private readonly NoteEvent[] _sortedNoteEvents;
        private readonly ScoreEvent[] _sortedScoreEvents;
        private readonly IGameEnergyCounter _gameEnergyCounter;

        public ScorePlayer(SiraLog siraLog, ReplayFile file, ScoreController scoreController, IGameEnergyCounter gameEnergyCounter) {
            
            _siraLog = siraLog;
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

                ScoreEvent activeEvent = _sortedScoreEvents[_lastIndex++];
                recentMultipliedScore = Accessors.MultipliedScore(ref _scoreController) = activeEvent.Score;
                _siraLog.Info("New Score Event Received: " + activeEvent.Score);
                
                if (_lastIndex >= _sortedScoreEvents.Length)
                    break;
            }

            if (recentMultipliedScore.HasValue) {

                Accessors.ImmediatePossible(ref _scoreController) = LeaderboardUtils.OldMaxRawScoreForNumberOfNotes(_lastIndex);
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
            /*
            int cutOrMissRecorded = _sortedNoteEvents.Count(ne => (ne.EventType == NoteEventType.GoodCut || ne.EventType == NoteEventType.BadCut || ne.EventType == NoteEventType.Miss) && time > ne.Time);
            Accessors.CutOrMissedNotes(ref _scoreController) = cutOrMissRecorded;

            var totalMultiplier = Accessors.ModifiersModelSO(ref _scoreController).GetTotalMultiplier(Accessors.ModifierPanelsSO(ref _scoreController), _gameEnergyCounter.energy);

            Accessors.PrevModifierScore(ref _scoreController) = totalMultiplier;
            
            var immediate = Accessors.ImmediatePossible(ref _scoreController) = ScoreModel.MaxRawScoreForNumberOfNotes(cutOrMissRecorded);
            var earlyScore = newScore;
            Accessors.PrevRawScore(ref _scoreController) = earlyScore;
            Accessors.RawScore(ref _scoreController) = earlyScore;

            FieldAccessor<ScoreController, Action<int, int>>.Get(_scoreController, "immediateMaxPossibleScoreDidChangeEvent").Invoke(immediate,
                ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(immediate, totalMultiplier));

            FieldAccessor<ScoreController, Action<int, int>>.Get(_scoreController, "scoreDidChangeEvent").Invoke(earlyScore,
                ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(earlyScore, totalMultiplier));*/
        }
    }
}