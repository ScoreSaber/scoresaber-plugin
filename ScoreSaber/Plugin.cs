#region

using BeatSaberMarkupLanguage;
using HarmonyLib;
using IPA;
using IPA.Loader;
using ScoreSaber.Core;
using ScoreSaber.Core.Daemons;
using ScoreSaber.Core.Data.Internal;
using ScoreSaber.Core.ReplaySystem;
using ScoreSaber.Core.ReplaySystem.Installers;
using ScoreSaber.UI.Elements.Profile;
using SiraUtil.Zenject;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

#endregion

namespace ScoreSaber {
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin {
        internal static ReplayState ReplayState { get; set; }
        internal static Recorder ReplayRecorder { get; set; }
        internal static IPALogger Log { get; private set; }
        internal static Plugin Instance { get; private set; }

        internal static Settings Settings { get; private set; }

        internal static Http HttpInstance { get; private set; }

        internal static Material NoGlowMatRound { get; set; }

        internal Harmony harmony;

        [Init]
        public Plugin(IPALogger logger, PluginMetadata metadata, Zenjector zenjector) {
            Log = logger;
            Instance = this;

            zenjector.UseLogger(logger);
            zenjector.Expose<ComboUIController>("Environment");
            zenjector.Expose<GameEnergyUIPanel>("Environment");
            zenjector.Install<AppInstaller>(Location.App);
            zenjector.Install<MainInstaller>(Location.Menu);
            zenjector.Install<ImberInstaller>(Location.StandardPlayer);
            zenjector.Install<PlaybackInstaller>(Location.StandardPlayer);
            zenjector.Install<RecordInstaller, StandardGameplayInstaller>();
            zenjector.Install<RecordInstaller, MultiplayerLocalActivePlayerInstaller>();
            zenjector.UseAutoBinder();

            Version libVersion = Assembly.GetExecutingAssembly().GetName().Version;
            BSMLParser.instance.RegisterTypeHandler(new ProfileDetailViewTypeHandler());
            BSMLParser.instance.RegisterTag(new ProfileDetailViewTag(metadata.Assembly));

            HttpInstance = new Http(new HttpOptions { BaseURL = "https://scoresaber.com/api", ApplicationName = "ScoreSaber-PC", Version = libVersion });
        }

        [OnEnable]
        public void OnEnable() {

            SceneManager.sceneLoaded += SceneLoaded;

            Settings = Settings.LoadSettings();
            ReplayState = new ReplayState();
            switch (Settings.DisableScoreSaber) {
                case false:
                    harmony = new Harmony("com.umbranox.BeatSaber.ScoreSaber");
                    harmony.PatchAll(Assembly.GetExecutingAssembly());
                    PlayerPrefs.SetInt("lbPatched", 1);
                    break;
            }
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode) {

            switch (scene.name) {
                case "MainMenu":
                    SharedCoroutineStarter.instance.StartCoroutine(WaitForLeaderboard());
                    break;
            }
        }

        private IEnumerator WaitForLeaderboard() {
            // TODO: This is an expensive operation, perhaps this is cacheable?
            yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<PlatformLeaderboardViewController>().Any());
            NoGlowMatRound = Resources.FindObjectsOfTypeAll<Material>()
                .First(m => m.name == "UINoGlowRoundEdge");
        }

        [OnDisable]
        public void OnDisable() {

            SceneManager.sceneLoaded -= SceneLoaded;
        }


        private static bool _scoreSubmission = true;
        /// <summary>
        /// <c>ScoreSubmission</c> represents if the plugin should attempt to submit a new score.
        /// </summary>
        public static bool ScoreSubmission {
            get => _scoreSubmission;
            set {
                // canSet is True if BS_Utils or SiraUtil is being used
                bool canSet = new StackTrace().GetFrames().Select(frame => frame.GetMethod().ReflectedType.Namespace)
                    .Where(namespaceName => !string.IsNullOrEmpty(namespaceName)).Any(namespaceName =>
                        namespaceName.Contains("BS_Utils") || namespaceName.Contains("SiraUtil"));

                if (!canSet) {
                    return;
                }

                if (ReplayState.IsPlaybackEnabled) {
                    return;
                }

                var transitionSetup = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();
                var multiTransitionSetup = Resources.FindObjectsOfTypeAll<MultiplayerLevelScenesTransitionSetupDataSO>().FirstOrDefault();
                if (value) {
                    transitionSetup.didFinishEvent -= UploadDaemonHelper.StandardSceneTransitionInstance;
                    transitionSetup.didFinishEvent += UploadDaemonHelper.StandardSceneTransitionInstance;
                    multiTransitionSetup.didFinishEvent -= UploadDaemonHelper.MultiplayerSceneTransitionInstance;
                    multiTransitionSetup.didFinishEvent += UploadDaemonHelper.MultiplayerSceneTransitionInstance;
                } else {
                    transitionSetup.didFinishEvent -= UploadDaemonHelper.StandardSceneTransitionInstance;
                    multiTransitionSetup.didFinishEvent -= UploadDaemonHelper.MultiplayerSceneTransitionInstance;
                }
                _scoreSubmission = value;
            }
        }
    }
}