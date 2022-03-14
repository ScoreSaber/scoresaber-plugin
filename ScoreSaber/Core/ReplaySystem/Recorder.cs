using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Core.ReplaySystem.Recorders;
using ScoreSaber.Extensions;
using System;
using System.Threading.Tasks;
using Zenject;

namespace ScoreSaber.Core.ReplaySystem
{
    internal class Recorder : IInitializable, IDisposable
    {
        private string _requestId;
        private readonly ReplayFileWriter _replayFileWriter;
        private readonly PoseRecorder _poseRecorder;
        private readonly MetadataRecorder _metadataRecorder;
        private readonly NoteEventRecorder _noteEventRecorder;
        private readonly ScoreEventRecorder _scoreEventRecorder;
        private readonly HeightEventRecorder _heightEventRecorder;
        private readonly EnergyEventRecorder _energyEventRecorder;

        public Recorder(PoseRecorder poseRecorder, MetadataRecorder metadataRecorder, NoteEventRecorder noteEventRecorder, ScoreEventRecorder scoreEventRecorder, HeightEventRecorder heightEventRecorder, EnergyEventRecorder energyEventRecorder) {

            _poseRecorder = poseRecorder;
            _metadataRecorder = metadataRecorder;
            _noteEventRecorder = noteEventRecorder;
            _scoreEventRecorder = scoreEventRecorder;
            _heightEventRecorder = heightEventRecorder;
            _energyEventRecorder = energyEventRecorder;
            _replayFileWriter = new ReplayFileWriter();
        }

        public void Initialize() {
            Plugin.ReplayRecorder = this;
            Plugin.ReplayState.serializedReplay = null;
       }

        public ReplayFile Export() {

            return new ReplayFile() {
                metadata = _metadataRecorder.Export(),
                poseKeyframes = _poseRecorder.Export(),
                heightKeyframes = _heightEventRecorder.Export(),
                noteKeyframes = _noteEventRecorder.Export(),
                scoreKeyframes = _scoreEventRecorder.ExportScoreKeyframes(),
                comboKeyframes = _scoreEventRecorder.ExportComboKeyframes(),
                multiplierKeyframes = _scoreEventRecorder.ExportMultiplierKeyframes(),
                energyKeyframes = _energyEventRecorder.Export()
            };
        }

        public void Write() {

            try {
                _requestId = Guid.NewGuid().ToString();
                Plugin.ReplayState.currentRequestId = _requestId;
                WriteReplay(Export()).RunTask();
            } catch (Exception ex) {
                Plugin.Log.Error($"[Write] Failed to write replay {ex}");
            }
        }

        private async Task WriteReplay(ReplayFile file) {

            try {
                await Task.Run(() => {
                    byte[] replayCustom = _replayFileWriter.Write(file);
                    if (_requestId == Plugin.ReplayState.currentRequestId) {
                        Plugin.ReplayState.serializedReplay = replayCustom;
                    } else {
                        Plugin.Log.Error("Failed to write replay: Replay request id missmatch");
                    }
                });
                Dispose();
            } catch (Exception ex) {
                Plugin.Log.Error($"[WriteReplay] Failed to write replay {ex}");
            }
           
        }

        public void Dispose() { }
    }
}
