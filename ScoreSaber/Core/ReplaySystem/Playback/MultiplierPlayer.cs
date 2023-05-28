﻿using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using System;
using System.Linq;

namespace ScoreSaber.Core.ReplaySystem.Playback
{
    internal class MultiplierPlayer : TimeSynchronizer, IScroller
    {
        private ScoreController _scoreController;
        private readonly MultiplierEvent[] _sortedMultiplierEvents;

        public MultiplierPlayer(ReplayFile file, ScoreController scoreController) {

            _scoreController = scoreController;
            _sortedMultiplierEvents = file.multiplierKeyframes.ToArray();
        }

        public void TimeUpdate(float newTime) {

            for (int c = 0; c < _sortedMultiplierEvents.Length; c++) {
                // TODO: this has potential to have problems if _sortedMultiplierEvents[c].Time is within an epsilon of newTime, potentially applying combo changes twice or not at all
                if (_sortedMultiplierEvents[c].Time > newTime) {
                    int multiplier = c != 0 ? _sortedMultiplierEvents[c - 1].Multiplier : 1;
                    float progress = c != 0 ? _sortedMultiplierEvents[c - 1].NextMultiplierProgress : 0;
                    UpdateMultiplier(multiplier, progress);
                    return;
                }
            }
            // TODO: should break if _sortedMultiplerEvents are empty
            var lastEvent = _sortedMultiplierEvents.LastOrDefault();
            UpdateMultiplier(lastEvent.Multiplier, lastEvent.NextMultiplierProgress);
        }

        private void UpdateMultiplier(int multiplier, float progress) {

            var counter = Accessors.MultiplierCounter(ref _scoreController);
            Accessors.Multiplier(ref counter) = multiplier;
            Accessors.MaxProgress(ref counter) = multiplier * 2;
            Accessors.Progress(ref counter) = (int)(progress * (multiplier * 2));
            FieldAccessor<ScoreController, Action<int, float>>.Get(_scoreController, "multiplierDidChangeEvent").Invoke(multiplier, progress);
        }
    }
}