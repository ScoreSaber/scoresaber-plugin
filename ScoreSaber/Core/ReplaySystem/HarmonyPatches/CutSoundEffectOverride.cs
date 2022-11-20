#region

using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace ScoreSaber.Core.ReplaySystem.HarmonyPatches {
    [HarmonyPatch(typeof(NoteCutSoundEffectManager), nameof(NoteCutSoundEffectManager.HandleNoteWasSpawned))]
    internal class CutSoundEffectOverride {
        private static IEnumerator _buffer;
        private static NoteCutSoundEffectManager _spawnEffectManager;
        private static readonly Queue<NoteController> _effects = new Queue<NoteController>();
        internal static bool Buffer { get; set; }

        internal static bool Prefix(NoteCutSoundEffectManager __instance, NoteController noteController) {

            if (!Plugin.ReplayState.IsPlaybackEnabled || Plugin.ReplayState.IsLegacyReplay) {
                return true;
            }

            if (_spawnEffectManager == null || _spawnEffectManager != __instance) {
                _spawnEffectManager = __instance;
                _effects.Clear();
                _buffer = null;
                Buffer = false;
                return true;
            }

            if (!Buffer)
                return true;

            if (_effects.Contains(noteController)) {
                return true;
            }

            _effects.Enqueue(noteController);
            if (_buffer != null) {
                return false;
            }

            _buffer = BufferNoteSpawn(__instance);
            __instance.StartCoroutine(_buffer);
            return false;
        }

        private static IEnumerator BufferNoteSpawn(NoteCutSoundEffectManager manager) {

            while (_effects.Count > 0) {
                var effect = _effects.Peek();
                manager.HandleNoteWasSpawned(effect);
                _effects.Dequeue();
                yield return new WaitForEndOfFrame();
            }
            Buffer = false;
            _buffer = null;
        }
    }
}