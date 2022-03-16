using ScoreSaber.Extensions;
using System;
using System.Collections.Generic;
using Zenject;
using ScoreSaber.Core.ReplaySystem.Data;
using SiraUtil.Affinity;

namespace ScoreSaber.Core.ReplaySystem.Recorders
{

    internal class SwingFinisher : ISaberSwingRatingCounterDidFinishReceiver
    {
        public NoteID noteID;
        public NoteCutInfo noteCutInfo;
        public IReadonlyCutScoreBuffer cutScoreBuffer;
        public float timeWasCut;

        private Action<SwingFinisher> _didFinish;

        public void Init(NoteID noteID, IReadonlyCutScoreBuffer cutScoreBuffer, Action<SwingFinisher> didFinish, float timeWasCut) {

            this.noteID = noteID;
            _didFinish = didFinish;
            noteCutInfo = cutScoreBuffer.noteCutInfo;
            this.cutScoreBuffer = cutScoreBuffer;
            this.timeWasCut = timeWasCut;
        }

        public void HandleSaberSwingRatingCounterDidFinish(ISaberSwingRatingCounter saberSwingRatingCounter) {

            _didFinish?.Invoke(this);
            saberSwingRatingCounter?.UnregisterDidFinishReceiver(this);
        }
    }

    internal class NoteEventRecorder : TimeSynchronizer, IInitializable, IDisposable, IAffinity
    {
        private readonly List<NoteEvent> _noteKeyframes;
        private readonly ScoreController _scoreController;
        private readonly MemoryPool<SwingFinisher> _finisherPool;
        private readonly Dictionary<NoteData, NoteCutInfo> _collectedBadCutInfos = new Dictionary<NoteData, NoteCutInfo>();

        public NoteEventRecorder(ScoreController scoreController, MemoryPool<SwingFinisher> finisherPool) {

            _scoreController = scoreController;
            _finisherPool = finisherPool;
            _noteKeyframes = new List<NoteEvent>();
        }

        public void Initialize() {

            _scoreController.scoringForNoteFinishedEvent += ScoreController_scoringForNoteFinishedEvent;
        }

        private void ScoreController_scoringForNoteFinishedEvent(ScoringElement element) {

            var noteData = element.noteData;
            NoteID noteID = new NoteID() { Time = noteData.time, LineIndex = noteData.lineIndex, LineLayer = (int)noteData.noteLineLayer, ColorType = (int)noteData.colorType, CutDirection = (int)noteData.cutDirection };

            if (element is GoodCutScoringElement goodCut) {

                var finisher = _finisherPool.Spawn();
                finisher.Init(noteID, goodCut.cutScoreBuffer, Pog, audioTimeSyncController.songTime);
                
            } else if (element is BadCutScoringElement badCut) {

                var badCutEventType = noteData.colorType == ColorType.None ? NoteEventType.Bomb : NoteEventType.BadCut;
                var noteCutInfo = _collectedBadCutInfos[badCut.noteData];
                _collectedBadCutInfos.Remove(badCut.noteData);
                _noteKeyframes.Add(new NoteEvent() { NoteID = noteID, EventType = badCutEventType, CutPoint = noteCutInfo.cutPoint.Convert(), CutNormal = noteCutInfo.cutNormal.Convert(), SaberDirection = noteCutInfo.saberDir.Convert(), SaberType = (int)noteCutInfo.saberType, DirectionOK = noteCutInfo.directionOK, CutDirectionDeviation = noteCutInfo.cutDirDeviation, SaberSpeed = noteCutInfo.saberSpeed, CutAngle = noteCutInfo.cutAngle, CutDistanceToCenter = noteCutInfo.cutDistanceToCenter, BeforeCutRating = 0f, AfterCutRating = 0f, Time = audioTimeSyncController.songTime, UnityTimescale = UnityEngine.Time.timeScale, TimeSyncTimescale = audioTimeSyncController.timeScale });
            
            } else if (noteData.colorType != ColorType.None /* not bomb */ && element is MissScoringElement miss) {

                _noteKeyframes.Add(new NoteEvent() { NoteID = noteID, EventType = NoteEventType.Miss, CutPoint = VRPosition.None(), CutNormal = VRPosition.None(), SaberDirection = VRPosition.None(), SaberType = (int)noteData.colorType, DirectionOK = false, CutDirectionDeviation = 0f, SaberSpeed = 0f, CutAngle = 0f, CutDistanceToCenter = 0f, BeforeCutRating = 0f, AfterCutRating = 0f, Time = audioTimeSyncController.songTime, UnityTimescale = UnityEngine.Time.timeScale, TimeSyncTimescale = audioTimeSyncController.timeScale });
            }
        }

        [AffinityPrefix, AffinityPatch(typeof(ScoreController), nameof(ScoreController.HandleNoteWasCut))]
        protected void BadCutInfoCollector(NoteController noteController, in NoteCutInfo noteCutInfo) {

            _collectedBadCutInfos.Add(noteController.noteData, noteCutInfo);
        }

        private void Pog(SwingFinisher swingFinisher) {

            _noteKeyframes.Add(new NoteEvent() { NoteID = swingFinisher.noteID, EventType = NoteEventType.GoodCut, CutPoint = swingFinisher.noteCutInfo.cutPoint.Convert(), CutNormal = swingFinisher.noteCutInfo.cutNormal.Convert(), SaberDirection = swingFinisher.noteCutInfo.saberDir.Convert(), SaberType = (int)swingFinisher.noteCutInfo.saberType, DirectionOK = swingFinisher.noteCutInfo.directionOK, CutDirectionDeviation = swingFinisher.noteCutInfo.cutDirDeviation, SaberSpeed = swingFinisher.noteCutInfo.saberSpeed, CutAngle = swingFinisher.noteCutInfo.cutAngle, CutDistanceToCenter = swingFinisher.noteCutInfo.cutDistanceToCenter, BeforeCutRating = swingFinisher.cutScoreBuffer.beforeCutSwingRating, AfterCutRating = swingFinisher.cutScoreBuffer.afterCutSwingRating, Time = swingFinisher.timeWasCut, UnityTimescale = UnityEngine.Time.timeScale, TimeSyncTimescale = audioTimeSyncController.timeScale });
            _finisherPool.Despawn(swingFinisher);
        }

        public void Dispose() {

            _scoreController.scoringForNoteFinishedEvent -= ScoreController_scoringForNoteFinishedEvent;
        }

        public List<NoteEvent> Export() {

            return _noteKeyframes;
        }

    }
}
