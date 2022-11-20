#region

using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.Data;
using System;
using System.Linq;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Playback {
    internal class ComboPlayer : TimeSynchronizer, IScroller {
        private ComboController _comboController;
        private ComboUIController _comboUIController;
        private readonly NoteEvent[] _sortedNoteEvents;
        private readonly ComboEvent[] _sortedComboEvents;

        public ComboPlayer(ReplayFile file, ComboController comboController, ComboUIController comboUIController) {

            _comboController = comboController;
            _comboUIController = comboUIController;
            _sortedNoteEvents = file.noteKeyframes.ToArray();
            _sortedComboEvents = file.comboKeyframes.ToArray();
        }

        public void TimeUpdate(float newTime) {

            for (int c = 0; c < _sortedComboEvents.Length; c++) {
                if (_sortedComboEvents[c].Time >= newTime) {
                    UpdateCombo(newTime, c != 0 ? _sortedComboEvents[c - 1].Combo : 0);
                    return;
                }
            }
            UpdateCombo(newTime, _sortedComboEvents.LastOrDefault().Combo);
        }

        private void UpdateCombo(float time, int combo) {

            var previousComboEvents = _sortedNoteEvents.Where(ne => ne.EventType != NoteEventType.None && time > ne.Time);
            int cutOrMissRecorded = previousComboEvents.Count(ne => ne.EventType == NoteEventType.BadCut || ne.EventType == NoteEventType.GoodCut || ne.EventType == NoteEventType.Miss);

            Accessors.Combo(ref _comboController) = combo;
            Accessors.MaxCombo(ref _comboController) = cutOrMissRecorded;
            FieldAccessor<ComboController, Action<int>>.Get(ref _comboController, "comboDidChangeEvent").Invoke(combo);

            bool didLoseCombo = _sortedComboEvents.Any(sce => time > sce.Time && sce.Combo == 0);
            if ((combo == 0 && cutOrMissRecorded == 0) || !didLoseCombo) {
                Accessors.ComboAnimator(ref _comboUIController).Rebind();
                Accessors.ComboWasLost(ref _comboUIController) = false;
            } else {
                Accessors.ComboAnimator(ref _comboUIController).SetTrigger(Accessors.TriggerID(ref _comboUIController));
                Accessors.ComboWasLost(ref _comboUIController) = true;
            }
        }
    }
}