#region

using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.HarmonyPatches;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.Playback {
    internal class ReplayTimeSyncController : TimeSynchronizer, ITickable {
        private readonly List<IScroller> _scrollers;
        private readonly AudioManagerSO _audioManagerSO;
        private readonly AudioTimeSyncController.InitData _audioInitData;
        private BasicBeatmapObjectManager _basicBeatmapObjectManager;
        private NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private BeatmapCallbacksController.InitData _callbackInitData;
        private BeatmapCallbacksController _beatmapObjectCallbackController;
        private readonly BeatmapObjectSpawnController _beatmapObjectSpawnController;
        private bool _paused;

        public ReplayTimeSyncController(List<IScroller> scrollers, BasicBeatmapObjectManager basicBeatmapObjectManager, NoteCutSoundEffectManager noteCutSoundEffectManager, BeatmapObjectSpawnController beatmapObjectSpawnController, AudioTimeSyncController.InitData audioInitData, BeatmapCallbacksController.InitData initData, BeatmapCallbacksController beatmapObjectCallbackController) {

            _scrollers = scrollers;
            _callbackInitData = initData;
            _audioInitData = audioInitData;
            _basicBeatmapObjectManager = basicBeatmapObjectManager;
            _noteCutSoundEffectManager = noteCutSoundEffectManager;
            _beatmapObjectSpawnController = beatmapObjectSpawnController;
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
            _audioManagerSO = Accessors.AudioManager(ref noteCutSoundEffectManager);
        }

        public void Tick() {

            int index = -1;
            if (Input.GetKeyDown(KeyCode.Alpha1))
                index = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                index = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                index = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                index = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                index = 4;
            else if (Input.GetKeyDown(KeyCode.Alpha6))
                index = 5;
            else if (Input.GetKeyDown(KeyCode.Alpha7))
                index = 6;
            else if (Input.GetKeyDown(KeyCode.Alpha8))
                index = 7;
            else if (Input.GetKeyDown(KeyCode.Alpha9))
                index = 8;
            else if (Input.GetKeyDown(KeyCode.Alpha0))
                index = 9;

            if (index != -1) {
                OverrideTime(audioTimeSyncController.songLength * (index * 0.1f));
            }

            if (Input.GetKeyDown(KeyCode.Minus)) {
                if (audioTimeSyncController.timeScale > 0.1f) {
                    OverrideTimeScale(audioTimeSyncController.timeScale - 0.1f);
                }
            }

            if (Input.GetKeyDown(KeyCode.Equals)) {
                if (audioTimeSyncController.timeScale < 2.0f) {
                    OverrideTimeScale(audioTimeSyncController.timeScale + 0.1f);
                }
            }

            if (Input.GetKeyDown(KeyCode.R)) {
                OverrideTime(0f);
            }

            if (Input.GetKeyDown(KeyCode.Space)) {
                if (_paused) {
                    audioTimeSyncController.Resume();
                } else {
                    CancelAllHitSounds();
                    audioTimeSyncController.Pause();
                }
                _paused = !_paused;
            }
        }

        private void UpdateTimes() {

            foreach (var scroller in _scrollers)
                scroller.TimeUpdate(audioTimeSyncController.songTime);
        }

        public void OverrideTime(float time) {

            if (Mathf.Abs(time - audioTimeSyncController.songTime) <= 0.25f)
                return;

            var _audioTimeSyncController = audioTimeSyncController; // UMBRAMEGALUL
            CutSoundEffectOverride.Buffer = true;
            CancelAllHitSounds();

            // Forcibly enabling all the note/obstacle components to ensure their dissolve coroutine executes (it no likey when game pausey).
            foreach (var item in Accessors.GameNotePool(ref _basicBeatmapObjectManager).activeItems) {

                item.Hide(false);
                item.Pause(false);
                item.enabled = true;
                item.gameObject.SetActive(true);
                item.Dissolve(0f);
            }
            foreach (var item in Accessors.BombNotePool(ref _basicBeatmapObjectManager).activeItems) {

                item.Hide(false);
                item.Pause(false);
                item.enabled = true;
                item.gameObject.SetActive(true);
                item.Dissolve(0f);
            }
            foreach (var item in _basicBeatmapObjectManager.activeObstacleControllers) {

                item.Hide(false);
                item.Pause(false);
                item.enabled = true;
                item.gameObject.SetActive(true);
                item.Dissolve(0f);
            }

            var previousState = audioTimeSyncController.state;

            audioTimeSyncController.Pause();
            audioTimeSyncController.SeekTo(time / audioTimeSyncController.timeScale);

            if (previousState == AudioTimeSyncController.State.Playing)
                audioTimeSyncController.Resume();

            Accessors.InitialStartFilterTime(ref _callbackInitData) = time;
            Accessors.CallbackStartFilterTime(ref _beatmapObjectCallbackController) = time;

            foreach (KeyValuePair<float, CallbacksInTime> callback in Accessors
                         .CallbacksInTime(ref _beatmapObjectCallbackController).Where(callback =>
                             callback.Value.lastProcessedNode != null &&
                             callback.Value.lastProcessedNode.Value.time > time)) {
                callback.Value.lastProcessedNode = null;
            }

            Accessors.AudioSongTime(ref _audioTimeSyncController) = time;

            audioTimeSyncController.Update();
            UpdateTimes();
        }

        public void OverrideTimeScale(float newScale) {

            CancelAllHitSounds();
            var _audioTimeSyncController = audioTimeSyncController; // UMBRAMEGALUL
            Accessors.AudioSource(ref _audioTimeSyncController).pitch = newScale;

            Accessors.AudioTimeScale(ref _audioTimeSyncController) = newScale;
            Accessors.AudioStartOffset(ref _audioTimeSyncController)
                = (Time.timeSinceLevelLoad * _audioTimeSyncController.timeScale) - (_audioTimeSyncController.songTime + _audioInitData.songTimeOffset);

            _audioManagerSO.musicPitch = 1f / newScale;
            _audioTimeSyncController.Update();
        }

        public void CancelAllHitSounds() {

            var activeItems = Accessors.NoteCutPool(ref _noteCutSoundEffectManager).activeItems;
            foreach (var effect in activeItems.Where(effect => effect.isActiveAndEnabled)) {
                effect.StopPlayingAndFinish();
            }
            _noteCutSoundEffectManager.SetField("_prevNoteATime", -1f);
            _noteCutSoundEffectManager.SetField("_prevNoteBTime", -1f);
        }
    }
}