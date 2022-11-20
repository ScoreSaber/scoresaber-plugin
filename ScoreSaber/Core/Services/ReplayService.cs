#region

using ScoreSaber.Core.ReplaySystem;
using ScoreSaber.Core.ReplaySystem.Data;
using System;
using System.Threading.Tasks;

#endregion

namespace ScoreSaber.Core.Services {

    internal class ReplayService {

        public event Action<byte[]> ReplaySerialized;

        private string _currentPlayId;
        private Recorder _replayRecorder;

        public void NewPlayStarted(string playId, Recorder replayRecorder) {

            _currentPlayId = playId;
            _replayRecorder = replayRecorder;
            Plugin.Log.Debug($"New play started with id: {playId}");
        }

        public async Task<byte[]> WriteSerializedReplay() {

            var writer = new ReplayFileWriter();
            byte[] serializedReplay = null;
            Plugin.Log.Debug($"Writing replay with id: {_currentPlayId}");
            await Task.Run(() => {
                serializedReplay = writer.Write(_replayRecorder.Export());
            });
            Plugin.Log.Debug($"Replay written: {_currentPlayId}");
            ReplaySerialized?.Invoke(serializedReplay);
            return serializedReplay;
        }
    }
}