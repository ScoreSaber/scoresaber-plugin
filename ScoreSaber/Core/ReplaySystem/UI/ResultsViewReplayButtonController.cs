using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using IPA.Utilities.Async;
using ScoreSaber.Core.Daemons;
using ScoreSaber.Core.Services;
using ScoreSaber.Extensions;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ScoreSaber.Core.ReplaySystem.UI {
    internal class ResultsViewReplayButtonController : IInitializable, IDisposable {
        [UIComponent("watch-replay-button")]
        protected readonly Button watchReplayButton = null;

        private ResultsViewController _resultsViewController;
        private BeatmapLevel _beatmapLevel;
        private BeatmapKey _beatmapKey;
        private LevelCompletionResults _levelCompletionResults;
        private readonly PlayerService _playerService;
        private readonly ReplayLoader _replayLoader;
        private readonly ReplayService _replayService;

        private byte[] _serializedReplay;

        public ResultsViewReplayButtonController(ResultsViewController resultsViewController, PlayerService playerService, ReplayLoader replayLoader, ReplayService replayService) {

            _resultsViewController = resultsViewController;
            _playerService = playerService;
            _replayLoader = replayLoader;
            _replayService = replayService;
        }

        public void Initialize() {

            _resultsViewController.didActivateEvent += ResultsViewController_didActivateEvent;
            _resultsViewController.continueButtonPressedEvent += ResultsViewController_continueButtonPressedEvent;
            _resultsViewController.restartButtonPressedEvent += ResultsViewController_restartButtonPressedEvent;
            _replayService.ReplaySerialized += UploadDaemon_ReplaySerialized;
        }

        private void ResultsViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {

            if (firstActivation) {
                BSMLParser.Instance.Parse(
                    "<button-with-icon id=\"watch-replay-button\" icon=\"ScoreSaber.Resources.replay.png\" hover-hint=\"Watch Replay\" pref-width=\"15\" pref-height=\"13\" interactable=\"false\" on-click=\"replay-click\" />",
                    _resultsViewController.gameObject,
                    this
                );
                watchReplayButton.transform.localScale *= 0.4f;
                watchReplayButton.transform.localPosition = new Vector2(42.5f, 27f);
            }
            watchReplayButton.interactable = _serializedReplay != null;
            _beatmapLevel = _resultsViewController._beatmapLevel;
            _beatmapKey = _resultsViewController._beatmapKey;
            _levelCompletionResults = _resultsViewController._levelCompletionResults;
            WaitForReplay().RunTask();
        }

        private void ResultsViewController_restartButtonPressedEvent(ResultsViewController obj) {

            _serializedReplay = null;
        }

        private void ResultsViewController_continueButtonPressedEvent(ResultsViewController obj) {

            _serializedReplay = null;
        }

        private void UploadDaemon_ReplaySerialized(byte[] replay) {

            _serializedReplay = replay;
        }

        private async Task WaitForReplay() {

            await TaskEx.WaitUntil(() => _serializedReplay != null);
            watchReplayButton.interactable = true;
        }

        public void Dispose() {

            _resultsViewController.didActivateEvent -= ResultsViewController_didActivateEvent;
            _resultsViewController.continueButtonPressedEvent -= ResultsViewController_continueButtonPressedEvent;
            _resultsViewController.restartButtonPressedEvent -= ResultsViewController_restartButtonPressedEvent;
            _replayService.ReplaySerialized -= UploadDaemon_ReplaySerialized;
        }

        [UIAction("replay-click")]
        protected void ClickedReplayButton() {
            Plugin.ReplayState.isUsersReplay = true;
            _replayLoader.Load(_serializedReplay, _beatmapLevel, _beatmapKey, _levelCompletionResults.gameplayModifiers, _playerService.localPlayerInfo.playerName).RunTask();
            watchReplayButton.interactable = false;
       
        }
    }
}
