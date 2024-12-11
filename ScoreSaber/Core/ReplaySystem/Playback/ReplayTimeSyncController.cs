using IPA.Utilities;
using ScoreSaber.Core.ReplaySystem.UI;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;
using Zenject;

namespace ScoreSaber.Core.ReplaySystem.Playback
{
    internal class ReplayTimeSyncController : TimeSynchronizer, ITickable
    {
        private readonly List<IScroller> _scrollers;
        private readonly AudioManagerSO _audioManagerSO;
        private AudioTimeSyncController.InitData _audioInitData;
        private BasicBeatmapObjectManager _basicBeatmapObjectManager;
        private NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private BeatmapCallbacksController.InitData _callbackInitData;
        private BeatmapCallbacksController _beatmapObjectCallbackController;
        private readonly BeatmapObjectSpawnController _beatmapObjectSpawnController;
        private readonly BeatmapCallbacksUpdater _beatmapCallbacksUpdater = null;
        private readonly IReadonlyBeatmapData _beatmapData = null;

        private bool _paused => audioTimeSyncController.state == AudioTimeSyncController.State.Paused;

        public ReplayTimeSyncController(List<IScroller> scrollers, BasicBeatmapObjectManager basicBeatmapObjectManager, NoteCutSoundEffectManager noteCutSoundEffectManager, BeatmapObjectSpawnController beatmapObjectSpawnController, AudioTimeSyncController.InitData audioInitData, BeatmapCallbacksController.InitData initData, BeatmapCallbacksController beatmapObjectCallbackController, BeatmapCallbacksUpdater beatmapCallbacksUpdater, IReadonlyBeatmapData readonlyBeatmapData) {
            _scrollers = scrollers;
            _callbackInitData = initData;
            _audioInitData = audioInitData;
            _basicBeatmapObjectManager = basicBeatmapObjectManager;
            _noteCutSoundEffectManager = noteCutSoundEffectManager;
            _beatmapObjectSpawnController = beatmapObjectSpawnController;
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
            _beatmapCallbacksUpdater = beatmapCallbacksUpdater;
            _beatmapData = readonlyBeatmapData;
            _audioManagerSO = noteCutSoundEffectManager._audioManager;
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
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                OverrideTime(audioTimeSyncController.songTime - 5f);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                OverrideTime(audioTimeSyncController.songTime + 5f);
            }
        }

        private void UpdateTimes() {
            foreach (var scroller in _scrollers)
                scroller.TimeUpdate(audioTimeSyncController.songTime);
        }

        internal void OverrideTime(float time) {
            if (float.IsInfinity(time) || float.IsNaN(time) || Mathf.Abs(time - audioTimeSyncController._songTime) < 0.001f) return;
            time = Mathf.Clamp(time, audioTimeSyncController._startSongTime, audioTimeSyncController.songEndTime);

            var _audioTimeSyncController = audioTimeSyncController; // UMBRAMEGALUL
            HarmonyPatches.CutSoundEffectOverride.Buffer = true;
            CancelAllHitSounds();

            var previousState = audioTimeSyncController.state;

            _beatmapCallbacksUpdater.Pause();

            _basicBeatmapObjectManager._basicGameNotePoolContainer.activeItems.ForEach(x => _basicBeatmapObjectManager.Despawn(x));
            _basicBeatmapObjectManager._burstSliderHeadGameNotePoolContainer.activeItems.ForEach(x => _basicBeatmapObjectManager.Despawn(x));
            _basicBeatmapObjectManager._burstSliderGameNotePoolContainer.activeItems.ForEach(x => _basicBeatmapObjectManager.Despawn(x));
            _basicBeatmapObjectManager._bombNotePoolContainer.activeItems.ForEach(x => _basicBeatmapObjectManager.Despawn(x));
            _basicBeatmapObjectManager._obstaclePoolContainer.activeItems.ForEach(x => _basicBeatmapObjectManager.Despawn(x));

            audioTimeSyncController.Pause();
            _audioTimeSyncController._prevAudioSamplePos = -1;
            audioTimeSyncController.SeekTo(time / audioTimeSyncController.timeScale);
            _beatmapObjectCallbackController._prevSongTime = float.MinValue;

            var beatmapDataCache = LocateBeatmapData(time);

            foreach (var callback in _beatmapObjectCallbackController._callbacksInTimes) {
                callback.Value.lastProcessedNode = beatmapDataCache;
            }

            if (previousState == AudioTimeSyncController.State.Playing)
                audioTimeSyncController.Resume();

            _beatmapCallbacksUpdater.LateUpdate();
            _beatmapCallbacksUpdater.Resume();

            UpdateTimes();
        }

        private LinkedListNode<BeatmapDataItem> _lastLocatedNode = null;

        private LinkedListNode<BeatmapDataItem> LocateBeatmapData(float targetTime) {
            _lastLocatedNode ??= _beatmapData.allBeatmapDataItems.First;

            while (_lastLocatedNode != null && _lastLocatedNode.Value.time < targetTime) {
                _lastLocatedNode = _lastLocatedNode.Next;
            }

            while (_lastLocatedNode != null && _lastLocatedNode.Value.time > targetTime) {
                _lastLocatedNode = _lastLocatedNode.Previous;
            }

            return _lastLocatedNode;
        }




        public void OverrideTimeScale(float newScale) {

            CancelAllHitSounds();
            var _audioTimeSyncController = audioTimeSyncController; // UMBRAMEGALUL
            _audioTimeSyncController._audioSource.pitch = newScale;

            _audioTimeSyncController._timeScale = newScale;
            _audioTimeSyncController._audioStartTimeOffsetSinceStart = (Time.timeSinceLevelLoad * _audioTimeSyncController.timeScale) - (_audioTimeSyncController.songTime + _audioInitData.songTimeOffset);

            _audioManagerSO.musicPitch = 1f / newScale;
            _audioTimeSyncController.Update();
        }

        public void CancelAllHitSounds() {

            var activeItems = _noteCutSoundEffectManager._noteCutSoundEffectPoolContainer.activeItems;
            for (int i = 0; i < activeItems.Count; i++) {
                var effect = activeItems[i];
                if (effect.isActiveAndEnabled)
                    effect.StopPlayingAndFinish();
            }
            _noteCutSoundEffectManager.SetField("_prevNoteATime", -1f);
            _noteCutSoundEffectManager.SetField("_prevNoteBTime", -1f);
        }
    }
}