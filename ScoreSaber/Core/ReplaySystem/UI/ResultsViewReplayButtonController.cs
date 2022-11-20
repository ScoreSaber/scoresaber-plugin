#region

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using ScoreSaber.Core.Services;
using ScoreSaber.Extensions;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

namespace ScoreSaber.Core.ReplaySystem.UI {
    internal class ResultsViewReplayButtonController : IInitializable, IDisposable {
        [UIComponent("watch-replay-button")]
        protected readonly Button watchReplayButton = null;

        private ResultsViewController _resultsViewController;
        private IDifficultyBeatmap _difficultyBeatmap;
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
                BSMLParser.instance.Parse(
                    // Converted to XElement to avoid having long string literals
                    new XElement("button-with-icon", new XAttribute("id", "watch-replay-button"),
                        new XAttribute("icon", "ScoreSaber.Resources.replay.png"),
                        new XAttribute("hover-hint", "Watch Replay"), new XAttribute("pref-width", "15"),
                        new XAttribute("pref-height", "13"), new XAttribute("interactable", "false"),
                        new XAttribute("on-click", "replay-click")).ToString(),
                    _resultsViewController.gameObject,
                    this
                );
                watchReplayButton.transform.localScale *= 0.4f;
                watchReplayButton.transform.localPosition = new Vector2(42.5f, 27f);
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

        public void Dispose() {

            _resultsViewController.didActivateEvent -= ResultsViewController_didActivateEvent;
            _resultsViewController.continueButtonPressedEvent -= ResultsViewController_continueButtonPressedEvent;
            _resultsViewController.restartButtonPressedEvent -= ResultsViewController_restartButtonPressedEvent;
            _replayService.ReplaySerialized -= UploadDaemon_ReplaySerialized;
        }

        [UIAction("replay-click")]
        protected void ClickedReplayButton() {

            _replayLoader.Load(_serializedReplay, _difficultyBeatmap, _levelCompletionResults.gameplayModifiers, _playerService.LocalPlayerInfo.PlayerName).RunTask();
            watchReplayButton.interactable = false;

        }
    }
}