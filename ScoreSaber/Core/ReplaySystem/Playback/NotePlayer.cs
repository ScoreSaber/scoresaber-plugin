using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using ScoreSaber.Extensions;
using System.Linq;
using Zenject;

namespace ScoreSaber.Core.ReplaySystem.Playback
{
    internal class NotePlayer : TimeSynchronizer, ITickable, IScroller
    {
        private int _lastIndex = 0;
        private readonly SaberManager _saberManager;
        private readonly NoteEvent[] _sortedNoteEvents;
        private readonly SaberSwingRatingCounter.Pool _swingCounterPool;
        private readonly MemoryPoolContainer<GameNoteController> _gameNotePool;
        private readonly MemoryPoolContainer<BombNoteController> _bombNotePool;

        public NotePlayer(ReplayFile file, SaberManager saberManager, SaberSwingRatingCounter.Pool swingCounterPool, BasicBeatmapObjectManager basicBeatmapObjectManager) {

            _saberManager = saberManager;
            _swingCounterPool = swingCounterPool;
            _gameNotePool = Accessors.GameNotePool(ref basicBeatmapObjectManager);
            _bombNotePool = Accessors.BombNotePool(ref basicBeatmapObjectManager);
            _sortedNoteEvents = file.noteKeyframes.OrderBy(nk => nk.Time).ToArray();
        }

        public void Tick() {

            if (_lastIndex >= _sortedNoteEvents.Length)
                return;

            while (audioTimeSyncController.songTime >= _sortedNoteEvents[_lastIndex].Time) {
                NoteEvent activeEvent = _sortedNoteEvents[_lastIndex++];
                ProcessEvent(activeEvent);

                if (_lastIndex >= _sortedNoteEvents.Length)
                    break;
            }
        }

        private bool ProcessEvent(NoteEvent activeEvent) {

            bool foundNote = false;
            if (activeEvent.EventType == NoteEventType.GoodCut || activeEvent.EventType == NoteEventType.BadCut) {
                foreach (var noteController in _gameNotePool.activeItems) {
                    if (HandleEvent(activeEvent, noteController)) {
                        foundNote = true;
                        break;
                    }
                }
            } else if (activeEvent.EventType == NoteEventType.Bomb) {
                foreach (var bombController in _bombNotePool.activeItems) {
                    if (HandleEvent(activeEvent, bombController)) {
                        foundNote = true;
                        break;
                    }
                }
            }
            return foundNote;
        }

        private bool HandleEvent(NoteEvent activeEvent, NoteController noteController) {

            if (DoesNoteMatchID(activeEvent.NoteID, noteController.noteData)) {
                Saber correctSaber = noteController.noteData.colorType == ColorType.ColorA ? _saberManager.leftSaber : _saberManager.rightSaber;

                SaberSwingRatingCounter swingCounter = null;
                if (noteController is GameNoteController gameNote) {
                    swingCounter = _swingCounterPool.Spawn();
                    swingCounter.Init(correctSaber.movementData, noteController.noteTransform, true, true);
                    Accessors.BeforeCutRating(ref swingCounter) = activeEvent.BeforeCutRating;
                    Accessors.AfterCutRating(ref swingCounter) = activeEvent.AfterCutRating;
                    swingCounter.RegisterDidFinishReceiver(gameNote);
                }

                NoteCutInfo noteCutInfo = new NoteCutInfo(
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
                    swingCounter
                );

                noteController.InvokeMethod<object, NoteController>("SendNoteWasCutEvent", noteCutInfo);

                if (swingCounter != null) {
                    var receivers = Accessors.ChangeReceivers(ref swingCounter);
                    for (int i = receivers.items.Count - 1; i >= 0; i--) {
                        receivers.items[i].HandleSaberSwingRatingCounterDidChange(swingCounter, swingCounter.afterCutRating);
                    }
                    swingCounter.Finish();
                }
                return true;
            }
            return false;
        }

        private static bool DoesNoteMatchID(NoteID id, NoteData note) {

            return new NoteID { Time = note.time, LineIndex = note.lineIndex, LineLayer = (int)note.noteLineLayer, ColorType = (int)note.colorType, CutDirection = (int)note.cutDirection } == id;
        }

        public void TimeUpdate(float newTime) {

            for (int c = 0; c < _sortedNoteEvents.Length; c++) {
                if (_sortedNoteEvents[c].Time >= newTime) {
                    _lastIndex = c;
                    Tick();
                    return;
                }
            }
            _lastIndex = _sortedNoteEvents.Count() != 0 ? _sortedNoteEvents.Length - 1 : 0;
        }
    }
}