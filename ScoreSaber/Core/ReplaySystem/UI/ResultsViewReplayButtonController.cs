#region

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using ScoreSaber.Core.Services;
using ScoreSaber.Extensions;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.UI {
    internal class ResultsViewReplayButtonController : IInitializable, IDisposable {
        private readonly PlayerService _playerService;
        private readonly ReplayLoader _replayLoader;
        private readonly ReplayService _replayService;

        [UIComponent("watch-replay-button")] protected readonly Button watchReplayButton = null;

        private IDifficultyBeatmap _difficultyBeatmap;
        private LevelCompletionResults _levelCompletionResults;

        private ResultsViewController _resultsViewController;

        private byte[] _serializedReplay;

        public ResultsViewReplayButtonController(ResultsViewController resultsViewController,
            PlayerService playerService, ReplayLoader replayLoader, ReplayService replayService) {
            _resultsViewController = resultsViewController;
            _playerService = playerService;
            _replayLoader = replayLoader;
            _replayService = replayService;
        }

        public void Dispose() {
            _resultsViewController.didActivateEvent -= ResultsViewController_didActivateEvent;
            _resultsViewController.continueButtonPressedEvent -= ResultsViewController_continueButtonPressedEvent;
            _resultsViewController.restartButtonPressedEvent -= ResultsViewController_restartButtonPressedEvent;
            _replayService.ReplaySerialized -= UploadDaemon_ReplaySerialized;
        }

        public void Initialize() {
            _resultsViewController.didActivateEvent += ResultsViewController_didActivateEvent;
            _resultsViewController.continueButtonPressedEvent += ResultsViewController_continueButtonPressedEvent;
            _resultsViewController.restartButtonPressedEvent += ResultsViewController_restartButtonPressedEvent;
            _replayService.ReplaySerialized += UploadDaemon_ReplaySerialized;
        }

        private void ResultsViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy,
            bool screenSystemEnabling) {
            switch (firstActivation) {
                case true:
                    BSMLParser.instance.Parse(
                        "<button-with-icon id=\"watch-replay-button\" icon=\"ScoreSaber.Resources.replay.png\" hover-hint=\"Watch Replay\" pref-width=\"15\" pref-height=\"13\" interactable=\"false\" on-click=\"replay-click\" />",
                        _resultsViewController.gameObject,
                        this
                    );
                    watchReplayButton.transform.localScale *= 0.4f;
                    watchReplayButton.transform.localPosition = new Vector2(42.5f, 27f);
                    break;
            }

            _difficultyBeatmap = Accessors.resultsViewControllerDifficultyBeatmap(ref _resultsViewController);
            _levelCompletionResults = Accessors.resultsViewControllerLevelCompletionResults(ref _resultsViewController);
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

        [UIAction("replay-click")]
        protected void ClickedReplayButton() {
            _replayLoader.Load(_serializedReplay, _difficultyBeatmap, _levelCompletionResults.gameplayModifiers,
                _playerService.localPlayerInfo.playerName).RunTask();
            watchReplayButton.interactable = false;
        }
    }
}