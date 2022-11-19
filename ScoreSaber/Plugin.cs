#region

using BeatSaberMarkupLanguage;
using HarmonyLib;
using IPA;
using IPA.Loader;
using IPA.Utilities.Async;
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
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

#endregion

namespace ScoreSaber {
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin {
        internal static Material Furry;
        internal static Material NonFurry;
        internal static Material NoGlowMatRound;

        private static bool _scoreSubmission = true;

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

            HttpInstance = new Http(new HttpOptions
                { baseURL = "https://scoresaber.com/api", applicationName = "ScoreSaber-PC", version = libVersion });
        }

        internal static ReplayState ReplayState { get; set; }
        internal static Recorder ReplayRecorder { get; set; }
        internal static IPALogger Log { get; private set; }
        internal static Plugin Instance { get; private set; }

        internal static Settings Settings { get; private set; }

        internal static Http HttpInstance { get; private set; }

        public static bool ScoreSubmission {
            get => _scoreSubmission;
            set {
                bool canSet = new StackTrace().GetFrames().Select(frame => frame.GetMethod().ReflectedType.Namespace)
                    .Where(namespaceName => !string.IsNullOrEmpty(namespaceName)).Any(namespaceName =>
                        namespaceName.Contains("BS_Utils") || namespaceName.Contains("SiraUtil"));

                switch (canSet) {
                    case true: {
                        switch (ReplayState.IsPlaybackEnabled) {
                            case false: {
                                StandardLevelScenesTransitionSetupDataSO transitionSetup = Resources
                                    .FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();
                                MultiplayerLevelScenesTransitionSetupDataSO multiTransitionSetup =
                                    Resources.FindObjectsOfTypeAll<MultiplayerLevelScenesTransitionSetupDataSO>()
                                        .FirstOrDefault();
                                switch (value) {
                                    case true:
                                        transitionSetup.didFinishEvent -= UploadDaemonHelper.ThreeInstance;
                                        transitionSetup.didFinishEvent += UploadDaemonHelper.ThreeInstance;
                                        multiTransitionSetup.didFinishEvent -= UploadDaemonHelper.FourInstance;
                                        multiTransitionSetup.didFinishEvent += UploadDaemonHelper.FourInstance;
                                        break;
                                    default:
                                        transitionSetup.didFinishEvent -= UploadDaemonHelper.ThreeInstance;
                                        multiTransitionSetup.didFinishEvent -= UploadDaemonHelper.FourInstance;
                                        break;
                                }

                                _scoreSubmission = value;
                                break;
                            }
                        }

                        break;
                    }
                }
            }
        }

        [OnEnable]
        public void OnEnable() {
            SceneManager.sceneLoaded += SceneLoaded;

            Settings = Settings.LoadSettings();
            ReplayState = new ReplayState();
            switch (Settings.disableScoreSaber) {
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
            yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<PlatformLeaderboardViewController>().Any());
            NoGlowMatRound = Resources.FindObjectsOfTypeAll<Material>()
                .First(m => m.name == "UINoGlowRoundEdge");
        }


        [OnDisable]
        public void OnDisable() {
            SceneManager.sceneLoaded -= SceneLoaded;
        }

        internal static async Task<Material> GetFurryMaterial() {
            if (Furry != null) {
                return Furry;
            }

            AssetBundle bundle = null;

            IEnumerator SeriouslyUnityMakeSomethingBetter() {
                AssetBundleCreateRequest bundleContainer = AssetBundle.LoadFromMemoryAsync(
                    Utilities.GetResource(Assembly.GetExecutingAssembly(), "ScoreSaber.Resources.cyanisa.furry"));
                yield return bundleContainer;
                bundle = bundleContainer.assetBundle;
            }

            await Coroutines.AsTask(SeriouslyUnityMakeSomethingBetter());
            Furry = new Material(bundle.LoadAsset<Material>("FurMat"));
            bundle.Unload(false);
            NonFurry = BeatSaberUI.MainTextFont.material;
            Furry.mainTexture = BeatSaberUI.MainTextFont.material.mainTexture;

            return Furry;
        }
    }
}