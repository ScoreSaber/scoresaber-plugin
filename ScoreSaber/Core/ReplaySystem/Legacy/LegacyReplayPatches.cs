using HarmonyLib;
using ScoreSaber.Core.ReplaySystem.Legacy;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace ScoreSaber.Patches {

    internal class LegacyReplayPatches : IInitializable, IAffinity {

        private readonly LegacyReplayPlayer _legacyReplayPlayer;

        public LegacyReplayPatches(LegacyReplayPlayer legacyReplayPlayer) {
            _legacyReplayPlayer = legacyReplayPlayer;
        }

        public void Initialize() { }

        [AffinityPatch(typeof(ScoreController), nameof(ScoreController.HandleNoteWasCut))]
        [AffinityPrefix]
        void PatchNoteWasCut(NoteController noteController) {

            if (Plugin.ReplayState.IsPlaybackEnabled && Plugin.ReplayState.IsLegacyReplay) {
                NoteData noteData = noteController.noteData;
                if (noteData.colorType != ColorType.None) {
                    _legacyReplayPlayer.cutOrMissedNotes++;
                }
            }
        }

        [AffinityPatch(typeof(ScoreController), nameof(ScoreController.HandleNoteWasMissed))]
        [AffinityPrefix]
        bool PatchNoteWasMissed(NoteController noteController) {

            if (Plugin.ReplayState.IsPlaybackEnabled && Plugin.ReplayState.IsLegacyReplay) {
                NoteData noteData = noteController.noteData;
                if (noteData.colorType != ColorType.None) {
                    _legacyReplayPlayer.cutOrMissedNotes++;
                }
                return false;
            }
            return true;
        }

        [AffinityPatch(typeof(MissedNoteEffectSpawner), nameof(MissedNoteEffectSpawner.HandleNoteWasMissed))]
        [AffinityPrefix]
        bool PatchMissedNoteEffectSpawnerHandleNoteWasMissed() {

            if (Plugin.ReplayState.IsPlaybackEnabled && Plugin.ReplayState.IsLegacyReplay) {
                return _legacyReplayPlayer.IsRealMiss();
            }

            return true;
        }

        [AffinityPatch(typeof(FlyingSpriteSpawner), nameof(FlyingSpriteSpawner.SpawnFlyingSprite))]
        [AffinityPrefix]
        bool PatchSpawnFlyingSprite() {

            if (Plugin.ReplayState.IsPlaybackEnabled && Plugin.ReplayState.IsLegacyReplay) {
                return _legacyReplayPlayer.IsRealMiss();
            }

            return true;
        }

    }

    [HarmonyPatch(typeof(BombNoteController), nameof(BombNoteController.HandleWasCutBySaber))]
    internal class PatchBombNoteControllerWasCutBySaber {
        static bool Prefix() {

            if (Plugin.ReplayState.IsPlaybackEnabled && Plugin.ReplayState.IsLegacyReplay) {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ScoreUIController), nameof(ScoreUIController.HandleScoreDidChangeRealtime))]
    internal class PatchScoreUIController {
        static bool Prefix() {
            if (Plugin.ReplayState.IsPlaybackEnabled && Plugin.ReplayState.IsLegacyReplay) {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RelativeScoreAndImmediateRankCounter), nameof(RelativeScoreAndImmediateRankCounter.HandleScoreDidChange))]
    internal class PatchRelativeScoreAndImmediateRankCounter {
        static bool Prefix() {
            if (Plugin.ReplayState.IsPlaybackEnabled && Plugin.ReplayState.IsLegacyReplay) {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(StandardLevelGameplayManager), nameof(StandardLevelGameplayManager.HandleGameEnergyDidReach0))]
    internal class PatchStandardLevelGameplayManagerHandleGameEnergyDidReach0 {
        static bool Prefix() {
            if (Plugin.ReplayState.IsPlaybackEnabled && Plugin.ReplayState.IsLegacyReplay) {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(NoteCutSoundEffect), nameof(NoteCutSoundEffect.NoteWasCut))]
    internal class PatchNoteCutSoundEffect {
        static bool Prefix(NoteController noteController, in NoteCutInfo noteCutInfo, ref AudioSource ____audioSource, ref bool ____goodCut, float ____goodCutVolume, ref bool ____noteWasCut, ref NoteController ____noteController, ref NoteCutSoundEffect __instance) {

            if (Plugin.ReplayState.IsPlaybackEnabled && Plugin.ReplayState.IsLegacyReplay) {
                if (____noteController != noteController) {
                    return false;
                }
                ____noteWasCut = true;
                ____audioSource.priority = 24;
                ____audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.2f);
                ____goodCut = true;
                ____audioSource.volume = ____goodCutVolume;
                __instance.transform.position = noteCutInfo.cutPoint;
                return false;
            }

            return true;
        }
    }
}