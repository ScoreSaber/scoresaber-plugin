using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using System;
using System.Linq;
using Zenject;
using HeightEvent = ScoreSaber.Core.ReplaySystem.Data.HeightEvent;

namespace ScoreSaber.Core.ReplaySystem.Playback
{
    internal class HeightPlayer : TimeSynchronizer, IInitializable, ITickable, IScroller
    {
        private int _nextIndex = 0;
        private readonly HeightEvent[] _sortedHeightEvents;
        private readonly PlayerHeightDetector _playerHeightDetector;

        protected HeightPlayer(ReplayFile file, PlayerHeightDetector playerHeightDetector) {

            _playerHeightDetector = playerHeightDetector;
            _sortedHeightEvents = file.heightKeyframes.ToArray();
        }

        public void Initialize() {

            _playerHeightDetector.OnDestroy();
        }

        public void Tick() {

            float? newHeight = null;
            while (_nextIndex < _sortedHeightEvents.Length && audioTimeSyncController.songEndTime >= _sortedHeightEvents[_nextIndex].Time) {
                newHeight = _sortedHeightEvents[_nextIndex].Height;
                _nextIndex++;
            }
            if (newHeight.HasValue) {
                FieldAccessor<PlayerHeightDetector, Action<float>>.Get(_playerHeightDetector, "playerHeightDidChangeEvent").Invoke(newHeight.Value);
            }
        }

        public void TimeUpdate(float newTime) {

            for (int c = 0; c < _sortedHeightEvents.Length; c++) {
                if (_sortedHeightEvents[c].Time > newTime) {
                    _nextIndex = c;
                    return;
                }
            }
            _nextIndex = _sortedHeightEvents.Length;
            FieldAccessor<PlayerHeightDetector, Action<float>>.Get(_playerHeightDetector, "playerHeightDidChangeEvent").Invoke(_sortedHeightEvents.LastOrDefault().Height);
        }
    }
}