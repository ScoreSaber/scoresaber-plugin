#region

using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using System;
using System.Linq;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Playback {
    internal class HeightPlayer : TimeSynchronizer, IInitializable, ITickable, IScroller {
        private int _lastIndex;
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

            if (_lastIndex >= _sortedHeightEvents.Length - 1)
                return;

            var activeEvent = _sortedHeightEvents[_lastIndex];
            if (!(audioTimeSyncController.songEndTime >= activeEvent.Time)) {
                return;
            }

            _lastIndex++;
            FieldAccessor<PlayerHeightDetector, Action<float>>.Get(_playerHeightDetector, "playerHeightDidChangeEvent").Invoke(activeEvent.Height);
        }

        public void TimeUpdate(float newTime) {

            for (int c = 0; c < _sortedHeightEvents.Length; c++) {
                if (_sortedHeightEvents[c].Time >= newTime) {
                    _lastIndex = c;
                    Tick();
                    return;
                }
            }
            FieldAccessor<PlayerHeightDetector, Action<float>>.Get(_playerHeightDetector, "playerHeightDidChangeEvent").Invoke(_sortedHeightEvents.LastOrDefault().Height);
        }
    }
}