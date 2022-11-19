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
            switch (Plugin.ReplayState.IsPlaybackEnabled) {
                case true when !Plugin.ReplayState.IsLegacyReplay: {
                    if (_spawnEffectManager == null || _spawnEffectManager != __instance) {
                        _spawnEffectManager = __instance;
                        _effects.Clear();
                        _buffer = null;
                        Buffer = false;
                        return true;
                    }

                    switch (Buffer) {
                        case false:
                            return true;
                    }

                    if (!_effects.Contains(noteController)) {
                        _effects.Enqueue(noteController);
                        switch (_buffer) {
                            case null:
                                _buffer = BufferNoteSpawn(__instance);
                                __instance.StartCoroutine(_buffer);
                                break;
                        }

                        return false;
                    }

                    return true;
                }
                default:
                    return true;
            }
        }

        private static IEnumerator BufferNoteSpawn(NoteCutSoundEffectManager manager) {
            while (_effects.Count > 0) {
                NoteController effect = _effects.Peek();
                manager.HandleNoteWasSpawned(effect);
                _effects.Dequeue();
                yield return new WaitForEndOfFrame();
            }

            Buffer = false;
            _buffer = null;
        }
    }
}