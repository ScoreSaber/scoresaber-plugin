#region

using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Extensions;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Recorders {
    internal class NoteEventRecorder : TimeSynchronizer, IInitializable, IDisposable, IAffinity {
        private readonly Dictionary<NoteData, NoteCutInfo> _collectedBadCutInfos =
            new Dictionary<NoteData, NoteCutInfo>();

        private readonly List<NoteEvent> _noteKeyframes;
        private readonly ScoreController _scoreController;

        private readonly Dictionary<GoodCutScoringElement, float> _scoringStartInfo =
            new Dictionary<GoodCutScoringElement, float>();

        public NoteEventRecorder(ScoreController scoreController) {
            _scoreController = scoreController;
            _noteKeyframes = new List<NoteEvent>();
        }

        public void Dispose() {
            _scoreController.scoringForNoteFinishedEvent -= ScoreController_scoringForNoteFinishedEvent;
            _scoreController.scoringForNoteStartedEvent -= ScoreController_scoringForNoteStartedEvent;
        }

        public void Initialize() {
            _scoreController.scoringForNoteStartedEvent += ScoreController_scoringForNoteStartedEvent;
            _scoreController.scoringForNoteFinishedEvent += ScoreController_scoringForNoteFinishedEvent;
        }

        private void ScoreController_scoringForNoteStartedEvent(ScoringElement element) {
            switch (element) {
                case GoodCutScoringElement goodCut:
                    _scoringStartInfo.Add(goodCut, audioTimeSyncController.songTime);
                    break;
            }
        }

        private void ScoreController_scoringForNoteFinishedEvent(ScoringElement element) {
            NoteData noteData = element.noteData;
            NoteID noteID = new NoteID {
                Time = noteData.time, LineIndex = noteData.lineIndex, LineLayer = (int)noteData.noteLineLayer,
                ColorType = (int)noteData.colorType, CutDirection = (int)noteData.cutDirection
            };

            switch (element) {
                case GoodCutScoringElement goodCut: {
                    float cutTime = _scoringStartInfo[goodCut];
                    NoteCutInfo noteCutInfo = goodCut.cutScoreBuffer.noteCutInfo;
                    _scoringStartInfo.Remove(goodCut);

                    _noteKeyframes.Add(new NoteEvent {
                        NoteID = noteID,
                        EventType = NoteEventType.GoodCut,
                        CutPoint = noteCutInfo.cutPoint.Convert(),
                        CutNormal = noteCutInfo.cutNormal.Convert(),
                        SaberDirection = noteCutInfo.saberDir.Convert(),
                        SaberType = (int)noteCutInfo.saberType,
                        DirectionOK = noteCutInfo.directionOK,
                        CutDirectionDeviation = noteCutInfo.cutDirDeviation,
                        SaberSpeed = noteCutInfo.saberSpeed,
                        CutAngle = noteCutInfo.cutAngle,
                        CutDistanceToCenter = noteCutInfo.cutDistanceToCenter,
                        BeforeCutRating = goodCut.cutScoreBuffer.beforeCutSwingRating,
                        AfterCutRating = goodCut.cutScoreBuffer.afterCutSwingRating,
                        Time = cutTime,
                        UnityTimescale = Time.timeScale,
                        TimeSyncTimescale = audioTimeSyncController.timeScale
                    });
                    break;
                }
                case BadCutScoringElement badCut: {
                    NoteEventType badCutEventType =
                        noteData.colorType == ColorType.None ? NoteEventType.Bomb : NoteEventType.BadCut;
                    NoteCutInfo noteCutInfo = _collectedBadCutInfos[badCut.noteData];
                    _collectedBadCutInfos.Remove(badCut.noteData);
                    _noteKeyframes.Add(new NoteEvent {
                        NoteID = noteID,
                        EventType = badCutEventType,
                        CutPoint = noteCutInfo.cutPoint.Convert(),
                        CutNormal = noteCutInfo.cutNormal.Convert(),
                        SaberDirection = noteCutInfo.saberDir.Convert(),
                        SaberType = (int)noteCutInfo.saberType,
                        DirectionOK = noteCutInfo.directionOK,
                        CutDirectionDeviation = noteCutInfo.cutDirDeviation,
                        SaberSpeed = noteCutInfo.saberSpeed,
                        CutAngle = noteCutInfo.cutAngle,
                        CutDistanceToCenter = noteCutInfo.cutDistanceToCenter,
                        BeforeCutRating = 0f,
                        AfterCutRating = 0f,
                        Time = audioTimeSyncController.songTime,
                        UnityTimescale = Time.timeScale,
                        TimeSyncTimescale = audioTimeSyncController.timeScale
                    });
                    break;
                }
                default: {
                    if (noteData.colorType != ColorType.None /* not bomb */ && element is MissScoringElement) {
                        _noteKeyframes.Add(new NoteEvent {
                            NoteID = noteID,
                            EventType = NoteEventType.Miss,
                            CutPoint = VRPosition.None(),
                            CutNormal = VRPosition.None(),
                            SaberDirection = VRPosition.None(),
                            SaberType = (int)noteData.colorType,
                            DirectionOK = false, CutDirectionDeviation = 0f,
                            SaberSpeed = 0f,
                            CutAngle = 0f,
                            CutDistanceToCenter = 0f,
                            BeforeCutRating = 0f,
                            AfterCutRating = 0f,
                            Time = audioTimeSyncController.songTime,
                            UnityTimescale = Time.timeScale,
                            TimeSyncTimescale = audioTimeSyncController.timeScale
                        });
                    }

                    break;
                }
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(ScoreController), nameof(ScoreController.HandleNoteWasCut))]
        protected void BadCutInfoCollector(NoteController noteController, in NoteCutInfo noteCutInfo) {
            _collectedBadCutInfos.Add(noteController.noteData, noteCutInfo);
        }

        public List<NoteEvent> Export() {
            return _noteKeyframes;
        }
    }
}