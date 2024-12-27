using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Extensions;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;
using Zenject;
using static NoteData;

namespace ScoreSaber.Core.ReplaySystem.Playback {
    internal class NotePlayer : TimeSynchronizer, ITickable, IScroller, IAffinity {
        private int _nextIndex = 0;
        private readonly SiraLog _siraLog;
        private readonly SaberManager _saberManager;
        private readonly NoteEvent[] _sortedNoteEvents;
        private readonly NoteID[] _sortedNoteIDs;

        private readonly NoteEvent[] _sortedNotMissedNoteEvents;
        private readonly NoteID[] _sortedMissedNoteIDs;

        private readonly MemoryPoolContainer<GameNoteController> _gameNotePool;
        private readonly MemoryPoolContainer<GameNoteController> _burstSliderHeadNotePool;
        private readonly MemoryPoolContainer<BurstSliderGameNoteController> _burstSliderNotePool;
        private readonly MemoryPoolContainer<BombNoteController> _bombNotePool;

        private readonly Dictionary<NoteCutInfo, NoteEvent> _recognizedNoteCutInfos = new Dictionary<NoteCutInfo, NoteEvent>();
        private readonly Dictionary<NoteID, NoteCutInfo> _noteCutInfoCache = new Dictionary<NoteID, NoteCutInfo>();

        private readonly Dictionary<string, Dictionary<int, List<int>>> versionConversion = new Dictionary<string, Dictionary<int, List<int>>>() {
            { "3.0.0", new Dictionary<int, List<int>>() { // 3.0.0 -> latest
                { 2, new List<int> { 2, 6 } }, // Scoring type 2 can be represented as 2 or 6
                { 3, new List<int> { 3, 6 } }, // Scoring type 3 can be represented as 3 or 6
                { 6, new List<int> { 6 } }      // Scoring type 6 can only be represented as 6
            } }
        };

        public NotePlayer(SiraLog siraLog, ReplayFile file, SaberManager saberManager, BasicBeatmapObjectManager basicBeatmapObjectManager) {

            _siraLog = siraLog;
            _saberManager = saberManager;
            _gameNotePool = basicBeatmapObjectManager._basicGameNotePoolContainer;
            _burstSliderHeadNotePool = basicBeatmapObjectManager._burstSliderHeadGameNotePoolContainer;
            _burstSliderNotePool = basicBeatmapObjectManager._burstSliderGameNotePoolContainer;
            _bombNotePool = basicBeatmapObjectManager._bombNotePoolContainer;
            _sortedNoteEvents = file.noteKeyframes.OrderBy(nk => nk.Time).ToArray();
            _sortedNoteIDs = _sortedNoteEvents.Select(ne => ne.NoteID).ToArray();

            _sortedNotMissedNoteEvents = _sortedNoteEvents.Where(nk => nk.EventType != NoteEventType.Miss).ToArray();
            _sortedMissedNoteIDs = _sortedNotMissedNoteEvents.Select(ne => ne.NoteID).ToArray();
        }

        public void Tick() {
            while (_nextIndex < _sortedNoteEvents.Length && audioTimeSyncController.songTime >= _sortedNoteEvents[_nextIndex].Time) {
                NoteEvent activeEvent = _sortedNoteEvents[_nextIndex++];
                ProcessEvent(activeEvent);
            }
        }

        private void ProcessEvent(NoteEvent activeEvent) {
            // dont process events that are too far away from the current audio time
            if (Mathf.Abs(activeEvent.Time - audioTimeSyncController.songTime) > 0.2f) {
                return;
            }

            switch (activeEvent.EventType) {
                case NoteEventType.BadCut:
                case NoteEventType.GoodCut:
                    ProcessRelevantNotes(activeEvent);
                    break;
                case NoteEventType.Bomb:
                    ProcessRelevantBombNotes(activeEvent);
                    break;
                default:
                    break;
            }
        }

        private void ProcessRelevantNotes(NoteEvent activeEvent) {
            var activeNoteControllers = _gameNotePool.activeItems.Cast<NoteController>()
                .Concat(_burstSliderHeadNotePool.activeItems.Cast<NoteController>())
                .Concat(_burstSliderNotePool.activeItems.Cast<NoteController>());

            foreach (var noteController in activeNoteControllers) {
                if (DoesNoteMatchID(activeEvent.NoteID, noteController.noteData)) {
                    HandleEvent(activeEvent, noteController);
                }
            }
        }


        private void ProcessRelevantBombNotes(NoteEvent activeEvent) {
            foreach (var bombController in _bombNotePool.activeItems) {
                if (DoesNoteMatchID(activeEvent.NoteID, bombController.noteData)) {
                    HandleEvent(activeEvent, bombController);
                }
            }
        }

        private bool HandleEvent(NoteEvent activeEvent, NoteController noteController) {
            if (!_noteCutInfoCache.TryGetValue(activeEvent.NoteID, out NoteCutInfo noteCutInfo)) {

                noteCutInfo = GetNoteCutInfoFromNoteController(noteController, activeEvent);

                _noteCutInfoCache[activeEvent.NoteID] = noteCutInfo;
            }

            if(!_recognizedNoteCutInfos.ContainsKey(noteCutInfo)) {
                _recognizedNoteCutInfos.Add(noteCutInfo, activeEvent);
            }

            noteController.SendNoteWasCutEvent(noteCutInfo);
            return true;
        }

        NoteCutInfo GetNoteCutInfoFromNoteController(NoteController noteController, NoteEvent activeEvent) {

            Saber correctSaber = noteController.noteData.colorType == ColorType.ColorA ? _saberManager.leftSaber : _saberManager.rightSaber;
            var noteTransform = noteController.noteTransform;

            var noteCutInfo = new NoteCutInfo(noteController.noteData,
                activeEvent.SaberSpeed > 2f,
                activeEvent.DirectionOK,
                activeEvent.SaberType == (int)correctSaber.saberType,
                false,
                activeEvent.SaberSpeed,
                activeEvent.SaberDirection.Convert(),
                noteController.noteData.colorType == ColorType.ColorA ? SaberType.SaberA : SaberType.SaberB,
                noteController.noteData.time - activeEvent.Time,
                activeEvent.CutDirectionDeviation,
                activeEvent.CutPoint.Convert(),
                activeEvent.CutNormal.Convert(),
                activeEvent.CutDistanceToCenter,
                activeEvent.CutAngle,

                noteController.worldRotation,
                noteController.inverseWorldRotation,
                noteTransform.rotation,
                noteTransform.position,

                correctSaber.movementDataForLogic
            );

            return noteCutInfo;

        }

        bool DoesNoteMatchID(NoteID id, NoteData noteData) {
            if (!Mathf.Approximately(id.Time, noteData.time) || id.LineIndex != noteData.lineIndex || id.LineLayer != (int)noteData.noteLineLayer || id.ColorType != (int)noteData.colorType || id.CutDirection != (int)noteData.cutDirection)
                return false;

            if (id.GameplayType is int gameplayType && gameplayType != (int)noteData.gameplayType)
                return false;

            // check if we need to convert scoring type from a pre 1.40.0 replay
            if (versionConversion.TryGetValue(Plugin.ReplayState.LoadedReplayFile.metadata.Version, out Dictionary<int, List<int>> ScoringTypeConversions)) {
                if (id.ScoringType is int scoringType) {
                    if (ScoringTypeConversions.TryGetValue(scoringType, out List<int> allowedConversions)) {
                        if (!allowedConversions.Contains((int)noteData.scoringType)) {
                            return false;
                        }
                    } else if (scoringType != (int)noteData.scoringType) {
                        return false; // cant find conversion, strict matching
                    }
                }
            }
            else if (id.ScoringType is int scoringType && scoringType != (int)noteData.scoringType)
            return false; // strict matching like normal

            if (id.CutDirectionAngleOffset is float cutDirectionAngleOffset && !Mathf.Approximately(cutDirectionAngleOffset, noteData.cutDirectionAngleOffset))
                return false;

            return true;
        }


        [AffinityPostfix, AffinityPatch(typeof(GoodCutScoringElement), nameof(GoodCutScoringElement.Init))]
        protected void ForceCompleteGoodScoringElements(GoodCutScoringElement __instance, NoteCutInfo noteCutInfo, CutScoreBuffer ____cutScoreBuffer) {
            
            // Just in case someone else is creating their own scoring elements, we want to ensure that we're only force completing ones we know we've created
            if (!_recognizedNoteCutInfos.TryGetValue(noteCutInfo, out var activeEvent))
                return;

            _recognizedNoteCutInfos.Remove(noteCutInfo);

            if (!__instance.isFinished) {
                var ratingCounter = ____cutScoreBuffer._saberSwingRatingCounter;

                ratingCounter._afterCutRating = activeEvent.AfterCutRating;
                ratingCounter._beforeCutRating = activeEvent.BeforeCutRating;

                ____cutScoreBuffer.HandleSaberSwingRatingCounterDidFinish(ratingCounter);

                ScoringElement element = __instance;
                element.isFinished = true;
            }
        }

        [AffinityPrefix, AffinityPatch(typeof(GameNoteController), nameof(GameNoteController.NoteDidPassMissedMarker))]
        protected bool HandleGhostMissesIfNeeded(GameNoteController __instance) {
            // if a note is missed, check if its actually meant to be missed
            // only check the notes that we know arent missed, as theres no need to check missed notes
            // log(n) binary search
            int left = 0;
            int right = _sortedNotMissedNoteEvents.Length - 1;

            while (left <= right) {
                int mid = left + (right - left) / 2;
                NoteEvent middleEvent = _sortedNotMissedNoteEvents[mid];

                if (DoesNoteMatchID(middleEvent.NoteID, __instance.noteData)) {
                    if (middleEvent.EventType == NoteEventType.Miss) {
                        return true;
                    }
                    _siraLog.Warn("CATCHING MISSED NOTE");
                    NoteCutInfo noteCutInfo = GetNoteCutInfoFromNoteController(__instance, middleEvent);
                    __instance.SendNoteWasCutEvent(noteCutInfo);
                    return false;
                } else if (middleEvent.NoteID.Time < __instance.noteData.time) {
                    left = mid + 1;
                } else {
                    right = mid - 1;
                }
            }

            return true;
        }


        public void TimeUpdate(float newTime) {
            for (int c = 0; c < _sortedNoteEvents.Length; c++) {
                if (_sortedNoteEvents[c].Time > newTime) {
                    _nextIndex = c;
                    return;
                }
            }
            _nextIndex = _sortedNoteEvents.Length;
        }
    }
}