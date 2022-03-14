using ScoreSaber.Extensions;
using System;
using System.Collections.Generic;
using Zenject;
using ScoreSaber.Core.ReplaySystem.Data;

namespace ScoreSaber.Core.ReplaySystem.Recorders
{

    internal class SwingFinisher : ISaberSwingRatingCounterDidFinishReceiver
    {
        public NoteID noteID;
        public NoteCutInfo noteCutInfo;
        public ISaberSwingRatingCounter saberSwingRatingCounter;
        public float timeWasCut;

        private Action<SwingFinisher> _didFinish;

        public void Init(NoteID noteID, NoteCutInfo noteCutInfo, Action<SwingFinisher> didFinish, float timeWasCut) {

            this.noteID = noteID;
            this._didFinish = didFinish;
            this.noteCutInfo = noteCutInfo;
            this.timeWasCut = timeWasCut;
            saberSwingRatingCounter = noteCutInfo.swingRatingCounter;
            saberSwingRatingCounter.RegisterDidFinishReceiver(this);
        }

        public void HandleSaberSwingRatingCounterDidFinish(ISaberSwingRatingCounter saberSwingRatingCounter) {

            _didFinish?.Invoke(this);
            saberSwingRatingCounter?.UnregisterDidFinishReceiver(this);
        }
    }

    internal class NoteEventRecorder : TimeSynchronizer, IInitializable, IDisposable
    {
        private readonly List<NoteEvent> _noteKeyframes;
        private readonly ScoreController _scoreController;
        private readonly MemoryPool<SwingFinisher> _finisherPool;

        public NoteEventRecorder(ScoreController scoreController, MemoryPool<SwingFinisher> finisherPool) {

            _scoreController = scoreController;
            _finisherPool = finisherPool;
            _noteKeyframes = new List<NoteEvent>();
        }

        public void Initialize() {

            _scoreController.noteWasCutEvent += ScoreController_noteWasCutEvent;
            _scoreController.noteWasMissedEvent += ScoreController_noteWasMissedEvent;
        }

        private void ScoreController_noteWasCutEvent(NoteData noteData, in NoteCutInfo noteCutInfo, int multiplier) {

            NoteID noteID = new NoteID() { Time = noteData.time, LineIndex = noteData.lineIndex, LineLayer = (int)noteData.noteLineLayer, ColorType = (int)noteData.colorType, CutDirection = (int)noteData.cutDirection };

            if (noteData.colorType == ColorType.None) {

                _noteKeyframes.Add(new NoteEvent() { NoteID = noteID, EventType = NoteEventType.Bomb, CutPoint = VRPosition.None(), CutNormal = VRPosition.None(), SaberDirection = VRPosition.None(), SaberType = (int)noteCutInfo.saberType, DirectionOK = noteCutInfo.directionOK, CutDirectionDeviation = noteCutInfo.cutDirDeviation, SaberSpeed = 0f, CutAngle = 0f, CutDistanceToCenter = 0f, BeforeCutRating = 0f, AfterCutRating = 0f, Time = audioTimeSyncController.songTime, UnityTimescale = UnityEngine.Time.timeScale, TimeSyncTimescale = audioTimeSyncController.timeScale });
                return;
            }

            if (noteCutInfo.allIsOK) {
                var finisher = _finisherPool.Spawn();
                finisher.Init(noteID, noteCutInfo, Pog, audioTimeSyncController.songTime);
            } else {
                _noteKeyframes.Add(new NoteEvent() { NoteID = noteID, EventType = NoteEventType.BadCut, CutPoint = noteCutInfo.cutPoint.Convert(), CutNormal = noteCutInfo.cutNormal.Convert(), SaberDirection = noteCutInfo.saberDir.Convert(), SaberType = (int)noteCutInfo.saberType, DirectionOK = noteCutInfo.directionOK, CutDirectionDeviation = noteCutInfo.cutDirDeviation, SaberSpeed = noteCutInfo.saberSpeed, CutAngle = noteCutInfo.cutAngle, CutDistanceToCenter = noteCutInfo.cutDistanceToCenter, BeforeCutRating = 0f, AfterCutRating = 0f, Time = audioTimeSyncController.songTime, UnityTimescale = UnityEngine.Time.timeScale, TimeSyncTimescale = audioTimeSyncController.timeScale });
            }
        }

        private void Pog(SwingFinisher swingFinisher) {

            _noteKeyframes.Add(new NoteEvent() { NoteID = swingFinisher.noteID, EventType = NoteEventType.GoodCut, CutPoint = swingFinisher.noteCutInfo.cutPoint.Convert(), CutNormal = swingFinisher.noteCutInfo.cutNormal.Convert(), SaberDirection = swingFinisher.noteCutInfo.saberDir.Convert(), SaberType = (int)swingFinisher.noteCutInfo.saberType, DirectionOK = swingFinisher.noteCutInfo.directionOK, CutDirectionDeviation = swingFinisher.noteCutInfo.cutDirDeviation, SaberSpeed = swingFinisher.noteCutInfo.saberSpeed, CutAngle = swingFinisher.noteCutInfo.cutAngle, CutDistanceToCenter = swingFinisher.noteCutInfo.cutDistanceToCenter, BeforeCutRating = swingFinisher.saberSwingRatingCounter.beforeCutRating, AfterCutRating = swingFinisher.saberSwingRatingCounter.afterCutRating, Time = swingFinisher.timeWasCut, UnityTimescale = UnityEngine.Time.timeScale, TimeSyncTimescale = audioTimeSyncController.timeScale });
            _finisherPool.Despawn(swingFinisher);
        }

        private void ScoreController_noteWasMissedEvent(NoteData noteData, int multiplier) {

            if (noteData.colorType == ColorType.None) {
                return;
            }

            NoteID noteID = new NoteID() { Time = noteData.time, LineIndex = noteData.lineIndex, LineLayer = (int)noteData.noteLineLayer, ColorType = (int)noteData.colorType, CutDirection = (int)noteData.cutDirection };

            _noteKeyframes.Add(new NoteEvent() { NoteID = noteID, EventType = NoteEventType.Miss, CutPoint = VRPosition.None(), CutNormal = VRPosition.None(), SaberDirection = VRPosition.None(), SaberType = (int)noteData.colorType, DirectionOK = false, CutDirectionDeviation = 0f, SaberSpeed = 0f, CutAngle = 0f, CutDistanceToCenter = 0f, BeforeCutRating = 0f, AfterCutRating = 0f, Time = audioTimeSyncController.songTime, UnityTimescale = UnityEngine.Time.timeScale, TimeSyncTimescale = audioTimeSyncController.timeScale });
        }

        public void Dispose() {

            _scoreController.noteWasCutEvent -= ScoreController_noteWasCutEvent;
            _scoreController.noteWasMissedEvent -= ScoreController_noteWasMissedEvent;
        }

        public List<NoteEvent> Export() {

            return _noteKeyframes;
        }

    }
}
