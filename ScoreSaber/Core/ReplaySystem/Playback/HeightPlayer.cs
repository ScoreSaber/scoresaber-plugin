#region

using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using System;
using System.Linq;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Playback {
    internal class HeightPlayer : TimeSynchronizer, IInitializable, ITickable, IScroller {
        private readonly PlayerHeightDetector _playerHeightDetector;
        private readonly HeightEvent[] _sortedHeightEvents;
        private int _lastIndex;

        protected HeightPlayer(ReplayFile file, PlayerHeightDetector playerHeightDetector) {
            _playerHeightDetector = playerHeightDetector;
            _sortedHeightEvents = file.heightKeyframes.ToArray();
        }

        public void Initialize() {
            _playerHeightDetector.OnDestroy();
        }

        public void TimeUpdate(float newTime) {
            for (int c = 0; c < _sortedHeightEvents.Length; c++) {
                switch (_sortedHeightEvents[c].Time >= newTime) {
                    case true:
                        _lastIndex = c;
                        Tick();
                        return;
                }
            }

            FieldAccessor<PlayerHeightDetector, Action<float>>.Get(_playerHeightDetector, "playerHeightDidChangeEvent")
                .Invoke(_sortedHeightEvents.LastOrDefault().Height);
        }

        public void Tick() {
            switch (_lastIndex >= _sortedHeightEvents.Length - 1) {
                case true:
                    return;
            }

            HeightEvent activeEvent = _sortedHeightEvents[_lastIndex];
            switch (audioTimeSyncController.songEndTime >= activeEvent.Time) {
                case true:
                    _lastIndex++;
                    FieldAccessor<PlayerHeightDetector, Action<float>>
                        .Get(_playerHeightDetector, "playerHeightDidChangeEvent").Invoke(activeEvent.Height);
                    break;
            }
        }
    }
}