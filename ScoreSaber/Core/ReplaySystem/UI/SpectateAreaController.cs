#region

using System;
using System.Linq;
using Tweening;
using UnityEngine;
using Zenject;
using static ScoreSaber.Core.Data.Internal.Settings;

#endregion

namespace ScoreSaber.Core.ReplaySystem.UI {
    internal class SpectateAreaController : ITickable, IDisposable {
        private static readonly int _colorID = Shader.PropertyToID("_Color");
        private readonly TimeTweeningManager _timeTweeningManager;
        private readonly GameNoteController.Pool _gameNoteControllerPool;
        public event Action<Vector3, Quaternion> DidUpdatePlayerSpectatorPose;

        private GameNoteController _activeNote = null;
        // Unused?
        private Quaternion _initialQuaternion;
        private Tween _movementTween = null;
        private Tween _statusTween = null;
        private bool _despawned = false;

        public SpectateAreaController(DiContainer diContainer, TimeTweeningManager timeTweeningManager) {

            _timeTweeningManager = timeTweeningManager;
            _gameNoteControllerPool = diContainer.ResolveId<GameNoteController.Pool>(NoteData.GameplayType.Normal);
        }

        public void AnimateTo(string poseID) {

            if (!TryGetPose(poseID, out SpectatorPoseRoot pose))
                return;

            _statusTween?.Kill();
            if (_activeNote == null) {
                _activeNote = _gameNoteControllerPool.Spawn();
                _activeNote.enabled = false;
                _activeNote.transform.localScale = Vector3.zero;
                _initialQuaternion = _activeNote.noteTransform.localRotation;
                _activeNote.transform.SetLocalPositionAndRotation(pose.SpectatorPose.ToVector3(), Quaternion.identity);
                _activeNote.noteTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(45f, 45f, 45f));

                var visuals = _activeNote.GetComponent<ColorNoteVisuals>();
                Accessors.SetCircleVisibility(ref visuals, false);
                Accessors.SetArrowVisibility(ref visuals, false);
                var color = Accessors.NoteColor(ref visuals) = Color.cyan.ColorWithAlpha(3f);

                foreach (var block in Accessors.NoteMaterialBlocks(ref visuals)) {
                    block.materialPropertyBlock.SetColor(_colorID, color);
                    block.ApplyChanges();
                }

                _despawned = true;
            }

            if (_despawned) {
                _activeNote.gameObject.SetActive(true);
                _statusTween = new Vector3Tween(Vector3.zero, Vector3.one, UpdateNoteScale, 0.5f, EaseType.OutElastic);
                _timeTweeningManager.AddTween(_statusTween, _activeNote);
                _despawned = false;
            }

            _movementTween?.Kill();
            _activeNote.gameObject.SetActive(true);
            _movementTween = new Vector3Tween(_activeNote.transform.localPosition, pose.SpectatorPose.ToVector3(),
                val => { _activeNote.transform.localPosition = val; }, 0.75f, EaseType.OutQuart);
            _timeTweeningManager.AddTween(_movementTween, _activeNote);
        }

        public void JumpToCallback(string poseID) {

            if (TryGetPose(poseID, out SpectatorPoseRoot pose)) {
                DidUpdatePlayerSpectatorPose?.Invoke(pose.SpectatorPose.ToVector3(), Quaternion.identity);
            }
        }

        public void Dismiss() {

            if (_activeNote == null)
                return;

            _despawned = true;
            _statusTween?.Kill();
            _movementTween?.Kill();
            _statusTween = new Vector3Tween(Vector3.one, Vector3.zero, UpdateNoteScale, 0.5f, EaseType.OutQuart) {
                onCompleted = DespawnActiveNote
            };
            _timeTweeningManager.AddTween(_statusTween, _activeNote);
        }

        public void Tick() {

            if (_activeNote != null) {
                _activeNote.transform.Rotate(Vector3.up * 0.5f);
            }
        }

        private void UpdateNoteScale(Vector3 scale) {

            if (_activeNote != null) {
                _activeNote.transform.localScale = scale;
            }
        }

        private void DespawnActiveNote() {

            _despawned = true;
            _statusTween?.Kill();
            _movementTween?.Kill();
            _activeNote.gameObject.SetActive(false);
        }

        private bool TryGetPose(string poseID, out SpectatorPoseRoot pose) {

            pose = Plugin.Settings.SpectatorPositions.FirstOrDefault(sp => sp.Name == poseID);
            return pose.Name != null;
        }

        public void Dispose() {

            if (_activeNote != null) {
                _timeTweeningManager.KillAllTweens(_activeNote);
            }
        }
    }
}