#region

using ScoreSaber.Core.ReplaySystem.Data;
using System;
using System.Collections.Generic;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Recorders {
    internal class HeightEventRecorder : TimeSynchronizer, IInitializable, IDisposable {
        private readonly PlayerHeightDetector _playerHeightDetector;
        private readonly List<HeightEvent> _heightKeyframes;

        public HeightEventRecorder([InjectOptional] PlayerHeightDetector playerHeightDetector) {

            _playerHeightDetector = playerHeightDetector;
            _heightKeyframes = new List<HeightEvent>();
        }

        public void Initialize() {

            if (_playerHeightDetector != null) {
                _playerHeightDetector.playerHeightDidChangeEvent += PlayerHeightDetector_playerHeightDidChangeEvent;
            }
        }

        public void Dispose() {

            if (_playerHeightDetector != null) {
                _playerHeightDetector.playerHeightDidChangeEvent -= PlayerHeightDetector_playerHeightDidChangeEvent;
            }
        }

        private void PlayerHeightDetector_playerHeightDidChangeEvent(float newHeight) {

            _heightKeyframes.Add(new HeightEvent { Height = newHeight, Time = audioTimeSyncController.songTime });
        }

        public List<HeightEvent> Export() {

            return _heightKeyframes;
        }

    }
}