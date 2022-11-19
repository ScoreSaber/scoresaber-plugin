#region

using HarmonyLib;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Legacy {
    internal class LegacyReplayPatches : IInitializable, IAffinity {
        private readonly LegacyReplayPlayer _legacyReplayPlayer;

        public LegacyReplayPatches(LegacyReplayPlayer legacyReplayPlayer) {
            _legacyReplayPlayer = legacyReplayPlayer;
        }

        public void Initialize() { }

        [AffinityPatch(typeof(ScoreController), nameof(ScoreController.HandleNoteWasCut))]
        [AffinityPrefix]
        private void PatchNoteWasCut(NoteController noteController) {
            switch (Plugin.ReplayState.IsPlaybackEnabled) {
                case true when Plugin.ReplayState.IsLegacyReplay: {
                    NoteData noteData = noteController.noteData;
                    if (noteData.colorType != ColorType.None) {
                        _legacyReplayPlayer.cutOrMissedNotes++;
                    }

                    break;
                }
            }
        }

        [AffinityPatch(typeof(ScoreController), nameof(ScoreController.HandleNoteWasMissed))]
        [AffinityPrefix]
        private bool PatchNoteWasMissed(NoteController noteController) {
            switch (Plugin.ReplayState.IsPlaybackEnabled) {
                case true when Plugin.ReplayState.IsLegacyReplay: {
                    NoteData noteData = noteController.noteData;
                    if (noteData.colorType != ColorType.None) {
                        _legacyReplayPlayer.cutOrMissedNotes++;
                    }

                    return false;
                }
                default:
                    return true;
            }
        }

        [AffinityPatch(typeof(MissedNoteEffectSpawner), nameof(MissedNoteEffectSpawner.HandleNoteWasMissed))]
        [AffinityPrefix]
        private bool PatchMissedNoteEffectSpawnerHandleNoteWasMissed() {
            switch (Plugin.ReplayState.IsPlaybackEnabled) {
                case true when Plugin.ReplayState.IsLegacyReplay:
                    return _legacyReplayPlayer.IsRealMiss();
                default:
                    return true;
            }
        }

        [AffinityPatch(typeof(FlyingSpriteSpawner), nameof(FlyingSpriteSpawner.SpawnFlyingSprite))]
        [AffinityPrefix]
        private bool PatchSpawnFlyingSprite() {
            switch (Plugin.ReplayState.IsPlaybackEnabled) {
                case true when Plugin.ReplayState.IsLegacyReplay:
                    return _legacyReplayPlayer.IsRealMiss();
                default:
                    return true;
            }
        }
    }

    [HarmonyPatch(typeof(BombNoteController), nameof(BombNoteController.HandleWasCutBySaber))]
    internal class PatchBombNoteControllerWasCutBySaber {
        private static bool Prefix() {
            switch (Plugin.ReplayState.IsPlaybackEnabled) {
                case true when Plugin.ReplayState.IsLegacyReplay:
                    return false;
                default:
                    return true;
            }
        }
    }

    [HarmonyPatch(typeof(ScoreUIController), nameof(ScoreUIController.HandleScoreDidChangeRealtime))]
    internal class PatchScoreUIController {
        private static bool Prefix() {
            switch (Plugin.ReplayState.IsPlaybackEnabled) {
                case true when Plugin.ReplayState.IsLegacyReplay:
                    return false;
                default:
                    return true;
            }
        }
    }

    [HarmonyPatch(typeof(RelativeScoreAndImmediateRankCounter),
        nameof(RelativeScoreAndImmediateRankCounter.HandleScoreDidChange))]
    internal class PatchRelativeScoreAndImmediateRankCounter {
        private static bool Prefix() {
            switch (Plugin.ReplayState.IsPlaybackEnabled) {
                case true when Plugin.ReplayState.IsLegacyReplay:
                    return false;
                default:
                    return true;
            }
        }
    }

    [HarmonyPatch(typeof(StandardLevelGameplayManager), nameof(StandardLevelGameplayManager.HandleGameEnergyDidReach0))]
    internal class PatchStandardLevelGameplayManagerHandleGameEnergyDidReach0 {
        private static bool Prefix() {
            switch (Plugin.ReplayState.IsPlaybackEnabled) {
                case true when Plugin.ReplayState.IsLegacyReplay:
                    return false;
                default:
                    return true;
            }
        }
    }

    [HarmonyPatch(typeof(NoteCutSoundEffect), nameof(NoteCutSoundEffect.NoteWasCut))]
    internal class PatchNoteCutSoundEffect {
        private static bool Prefix(NoteController noteController, in NoteCutInfo noteCutInfo,
            ref AudioSource ____audioSource, ref bool ____goodCut, float ____goodCutVolume, ref bool ____noteWasCut,
            ref NoteController ____noteController, ref NoteCutSoundEffect __instance) {
            switch (Plugin.ReplayState.IsPlaybackEnabled) {
                case true when Plugin.ReplayState.IsLegacyReplay: {
                    if (____noteController != noteController) {
                        return false;
                    }

                    ____noteWasCut = true;
                    ____audioSource.priority = 24;
                    ____audioSource.pitch = Random.Range(0.9f, 1.2f);
                    ____goodCut = true;
                    ____audioSource.volume = ____goodCutVolume;
                    __instance.transform.position = noteCutInfo.cutPoint;
                    return false;
                }
                default:
                    return true;
            }
        }
    }
}