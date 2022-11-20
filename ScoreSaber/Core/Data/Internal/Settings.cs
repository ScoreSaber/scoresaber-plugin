#region

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#endregion

namespace ScoreSaber.Core.Data.Internal {
    internal class Settings {
        private static int CurrentVersion => 6;

        public bool HideReplayUI { get; set; } = false;

        public int FileVersion { get; set; }
        public bool DisableScoreSaber { get; set; }
        public bool ShowLocalPlayerRank { get; set; }
        public bool ShowScorePP { get; set; }
        public bool ShowStatusText { get; set; }
        public bool SaveLocalReplays { get; set; }
        public bool EnableCountryLeaderboards { get; set; }
        public float ReplayCameraFOV { get; set; }
        public float ReplayCameraXOffset { get; set; }
        public float ReplayCameraYOffset { get; set; }
        public float ReplayCameraZOffset { get; set; }
        public float ReplayCameraXRotation { get; set; }
        public float ReplayCameraYRotation { get; set; }
        public float ReplayCameraZRotation { get; set; }
        public bool EnableReplayFrameRenderer { get; set; }
        public string ReplayFramePath { get; set; }
        public bool HideNAScoresFromLeaderboard { get; set; }
        public bool HasClickedScoreSaberLogo { get; set; }
        public bool HasOpenedReplayUI { get; set; }
        public bool LeftHandedReplayUI { get; set; }
        public bool LockedReplayUIMode { get; set; }
        public List<SpectatorPoseRoot> SpectatorPositions { get; set; }

        internal static string DataPath => "UserData";
        internal static string ConfigPath => DataPath + @"\ScoreSaber";
        internal static string ReplayPath => ConfigPath + @"\Replays";

        public void SetDefaults() {

            DisableScoreSaber = false;
            ShowLocalPlayerRank = true;
            ShowScorePP = true;
            ShowStatusText = true;
            SaveLocalReplays = true;
            EnableCountryLeaderboards = true;
            ReplayCameraFOV = 70f;
            ReplayCameraXOffset = 0.0f;
            ReplayCameraYOffset = 0.0f;
            ReplayCameraZOffset = 0.0f;
            ReplayCameraXRotation = 0.0f;
            ReplayCameraYRotation = 0.0f;
            ReplayCameraZRotation = 0.0f;
            EnableReplayFrameRenderer = false;
            ReplayFramePath = "Z:\\Example\\Directory\\";
            HideNAScoresFromLeaderboard = false;
            HasClickedScoreSaberLogo = false;
            HasOpenedReplayUI = false;
            LeftHandedReplayUI = false;
            LockedReplayUIMode = false;
            SetDefaultSpectatorPositions();
        }

        public void SetDefaultSpectatorPositions() {

            SpectatorPositions = new List<SpectatorPoseRoot> {
                new SpectatorPoseRoot(new SpectatorPose(new Vector3(0f, 0f, -2f)), "Main"),
                new SpectatorPoseRoot(new SpectatorPose(new Vector3(0f, 4f, 0f)), "Bird's Eye"),
                new SpectatorPoseRoot(new SpectatorPose(new Vector3(-3f, 0f, 0f)), "Left"),
                new SpectatorPoseRoot(new SpectatorPose(new Vector3(3f, 0f, 0f)), "Right")
            };
        }

        internal static Settings LoadSettings() {

            try {
                if (!Directory.Exists(DataPath)) {
                    Directory.CreateDirectory(DataPath);
                }

                if (!Directory.Exists(ConfigPath)) {
                    Directory.CreateDirectory(ConfigPath);
                }

                if (!Directory.Exists(ReplayPath)) {
                    Directory.CreateDirectory(ReplayPath);
                }

                if (!File.Exists(ConfigPath + @"\ScoreSaber.json")) {
                    var settings = new Settings();
                    settings.SetDefaults();
                    SaveSettings(settings);
                    return settings;
                }

                var decoded = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(ConfigPath + @"\ScoreSaber.json"));

                //Upgrade settings if old
                if (decoded.FileVersion >= CurrentVersion) {
                    return decoded;
                }

                if (decoded.SpectatorPositions == null) {
                    decoded.SetDefaultSpectatorPositions();
                }
                SaveSettings(decoded);
                return decoded;
            } catch (Exception ex) {
                Plugin.Log.Error("Failed to load settings " + ex);
                return new Settings();
            }
        }

        internal static void SaveSettings(Settings settings) {

            try {
                settings.FileVersion = CurrentVersion;
                var serializerSettings = new JsonSerializerSettings {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                string serialized = JsonConvert.SerializeObject(settings, serializerSettings);
                File.WriteAllText(ConfigPath + @"\ScoreSaber.json", serialized);
            } catch (Exception ex) {
                Plugin.Log.Error("Failed to save settings " + ex);
            }
        }

        internal struct SpectatorPoseRoot {
            [JsonProperty("name")]
            internal string Name { get; set; }
            [JsonProperty("spectatorPose")]
            internal SpectatorPose SpectatorPose { get; set; }

            internal SpectatorPoseRoot(SpectatorPose spectatorPose, string name) {

                Name = name;
                SpectatorPose = spectatorPose;
            }
        }

        internal struct SpectatorPose {
            [JsonProperty("x")]
            internal float X { get; set; }
            [JsonProperty("y")]
            internal float Y { get; set; }
            [JsonProperty("z")]
            internal float Z { get; set; }

            internal SpectatorPose(Vector3 position) {

                X = position.x;
                Y = position.y;
                Z = position.z;
            }

            internal Vector3 ToVector3() {

                return new Vector3(X, Y, Z);
            }
        }
    }
}