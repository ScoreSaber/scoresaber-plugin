using System;
using HarmonyLib;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Phoenix;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using WebSocketSharp;
using Zenject;
using System.Net.Sockets;
using Socket = Phoenix.Socket;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace ScoreSaber.Core.Services {
    internal class ScoreSaberRichPresenceService : IDisposable {
        [Inject] private readonly SiraLog _log = null;
        [Inject] private readonly PlayerService _playerService = null;

        private Socket _socket = null;

        private bool _isDisposed = false;
        private bool _isEnabled = true;

        private Channel _userProfileChannel = null;

        public string TimeRightNow => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

        public void Initialize() {

            var socketOptions = new Socket.Options(new JsonMessageSerializer());
            var socketAddress = "wss://gwuff.fairy-shark.ts.net/socket";
            var socketFactory = new WebsocketSharpFactory();
            var socket = new Socket(socketAddress, null, socketFactory, socketOptions);

            _socket = socket;

            socket.OnOpen += onOpenCallback;
            socket.OnMessage += onMessageCallback;
            socket.OnClose += onCloseCallback;
            socket.Connect();

            _log.Notice("Rich presence service initialized");
        }

        public void ToggleRichPresence(bool enable) {
            if (_isEnabled == enable) {
                _log.Info("Rich presence toggle state unchanged.");
                return;
            }

            _isEnabled = enable;

            if (_isEnabled) {
                _log.Info("Enabling rich presence service...");
                if (_socket == null) {
                    Initialize();
                } else {
                    _socket.Connect();
                }
            } else {
                _log.Info("Disabling rich presence service...");
                DisconnectSocket();
            }
        }

        private void DisconnectSocket() {
            if (_socket != null) {
                _socket.Disconnect();
                _userProfileChannel = null;
                _log.Info("Socket disconnected. Service is now offline.");
            }
        }

        private void SendMenuEvent() {
            if (_playerService.localPlayerInfo == null) {
                _log.Warn("Local player info is null, unable to send rich presence data.");
                return;
            }

            if (_socket == null) {
                _log.Warn("Socket is null, unable to send rich presence data.");
                return;
            }

            var jsonObject = new SceneChangeEvent() {
                Timestamp = TimeRightNow,
                Scene = Scene.menu
            };

            SendUserProfileChannel("scene_change", jsonObject);
        }


        int maxRetries = 5;
        int retryAttempt = 0;

        private void onCloseCallback(ushort code, string message) {
            _log.Warn("Rich presence socket closed: " + code + " - " + message);
            if (_isDisposed) return;
            if(retryAttempt > maxRetries) {
                _log.Error("Max reconnection attempts reached. Unable to reconnect.");
                return;
            }
            _log.Warn($"Attempting to reconnect to rich presence socket... Attempt {retryAttempt + 1}");
            _socket.Connect();
            retryAttempt++;
        }


        public void SendUserProfileChannel(string @event, object message, TimeSpan? timeSpan = default) {
            _userProfileChannel.Push(@event, message, timeSpan);
        }

        private void onMessageCallback(Message message) {
            //_log.Info("Received message: " + message.Payload.ToString());
        }

        private void onOpenCallback() {
            _log.Notice("Connected to the rich presence socket");

            if(_playerService.localPlayerInfo == null) {
                _log.Warn("Local player info is null, unable to send rich presence data");
                return;
            }

            if(_socket == null) {
                _log.Warn("Socket is null, unable to send rich presence data");
                return;
            }

            _userProfileChannel = _socket.Channel($"user_profile:{_playerService.localPlayerInfo.playerId}", new System.Collections.Generic.Dictionary<string, object>() {
                {
                "sid", _playerService.localPlayerInfo.serverKey
                }
            });

            _userProfileChannel.Join();
            SendMenuEvent();
        }

        public void Dispose() {
            _isDisposed = true;
            _socket.Disconnect();
        }
    }

    public class MenuPresencePatches : IAffinity {

        [Inject] private readonly ScoreSaberRichPresenceService _richPresenceService = null;
        [Inject] private readonly SiraLog _log = null;

        [AffinityPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator), "HandleStandardLevelDidFinish")]
        [AffinityPostfix]
        public void HandleStopStandardLevelPostfix() {
            var jsonObject = new SceneChangeEvent() {
                Timestamp = _richPresenceService.TimeRightNow,
                Scene = Scene.menu
            };

            _richPresenceService.SendUserProfileChannel("scene_change", jsonObject);
            _log.Notice("Sent scene change event to rich presence service");
        }

        [AffinityPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator), nameof(SinglePlayerLevelSelectionFlowCoordinator.StartLevel))]
        [AffinityPostfix]
        public void HandleStartLevelPostfix(SinglePlayerLevelSelectionFlowCoordinator __instance, Action beforeSceneSwitchCallback, bool practice) {

            string hash = string.Empty;

            if (!__instance.selectedBeatmapLevel.levelID.StartsWith("custom_level_")) {
                hash = "#" + __instance.selectedBeatmapLevel.levelID;
            } else {
                hash = __instance.selectedBeatmapLevel.levelID.Substring(13);
            }

            bool isPractice = practice;
            Services.GameMode gameMode = Services.GameMode.solo;

            if (isPractice) {
                gameMode = Services.GameMode.practice;

                var songeventSilly = new SongStartEvent(_richPresenceService.TimeRightNow, gameMode,
                                    "HYPER SONIC MEGA DEATH **** CORE",
                                    string.Empty,
                                    "oce v...",
                                    "echelon#6295",
                                    "Standard",
                                    "A297DECDFB0B3FE6F14F0BEF788AEBAC978E825E",
                                    (int)2000000000,
                                    ((1 + 1 - 1) * 0) + 1, // those who know 💀💀💀 🥭🥭🥭
                                    0,
                                    1);

                _richPresenceService.SendUserProfileChannel("start_song", songeventSilly);
                return;
            }

            var songevent = new SongStartEvent(_richPresenceService.TimeRightNow, gameMode,
                                                __instance.selectedBeatmapLevel.songName,
                                                __instance.selectedBeatmapLevel.songSubName,
                                                __instance.selectedBeatmapLevel.allMappers.Join().ToString(),
                                                __instance.selectedBeatmapLevel.songAuthorName,
                                                "Standard",
                                                hash,
                                                (int)__instance.selectedBeatmapLevel.songDuration,
                                                ((int)__instance.selectedBeatmapKey.difficulty * 2) + 1,
                                                0,
                                                1);

            _richPresenceService.SendUserProfileChannel("start_song", songevent);
        }
    }

    public class GamePresencePatches : IAffinity {

        [AffinityPatch(typeof(GamePause), nameof(GamePause.Pause))]
        [AffinityPostfix]
        public void HandlePausePostfix() {
            // send rich presence message that the level has been paused
        }

        [AffinityPatch(typeof(GamePause), nameof(GamePause.Resume))]
        [AffinityPostfix]
        public void HandleResumePostfix() {
            // send rich presence message that the level has been resumed
        }
    }


    public sealed class WebsocketSharpAdapter : IWebsocket {
        private readonly WebsocketConfiguration _config;

        private readonly WebSocket _ws;

        public WebsocketSharpAdapter(WebSocket ws, WebsocketConfiguration config) {
            _ws = ws;
            _config = config;

            ws.OnOpen += OnWebsocketOpen;
            ws.OnClose += OnWebsocketClose;
            ws.OnError += OnWebsocketError;
            ws.OnMessage += OnWebsocketMessage;
        }

        public WebsocketState State {
            get {
                switch (_ws.ReadyState) {
                    case WebSocketState.Connecting:
                        return WebsocketState.Connecting;
                    case WebSocketState.Open:
                        return WebsocketState.Open;
                    case WebSocketState.Closing:
                        return WebsocketState.Closing;
                    case WebSocketState.Closed:
                        return WebsocketState.Closed;
                    default:
                        throw new NotImplementedException();

                }
            }
        }

        public void Connect() {
            _ws.Connect();
        }

        public void Send(string message) {
            _ws.Send(message);
        }

        public void Close(ushort? code = null, string message = null) {
            _ws.Close();
        }

        private void OnWebsocketOpen(object sender, EventArgs args) {
            _config.onOpenCallback(this);
        }

        private void OnWebsocketClose(object sender, CloseEventArgs args) {
            _config.onCloseCallback(this, args.Code, args.Reason);
        }

        private void OnWebsocketError(object sender, ErrorEventArgs args) {
            _config.onErrorCallback(this, args.Message);
        }

        private void OnWebsocketMessage(object sender, MessageEventArgs args) {
            _config.onMessageCallback(this, args.Data);
        }
    }

    public sealed class WebsocketSharpFactory : IWebsocketFactory {
        public IWebsocket Build(WebsocketConfiguration config) {
            var socket = new WebSocket(config.uri.AbsoluteUri);
            socket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            return new WebsocketSharpAdapter(socket, config);
        }
    }

    public enum Scene {
        [JsonProperty("menu")]
        menu,
        [JsonProperty("playing")]
        playing
    }

    /// <summary>
    /// Unix timestamp serialized to a string
    /// </summary>
    public class Timestamp {
        public string Value { get; set; } = string.Empty;
    }

    public class SceneChangeEvent {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonProperty("scene")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Scene Scene { get; set; }
    }

    public enum GameMode {
        [JsonProperty("solo")]
        solo,
        [JsonProperty("multi")]
        multiplayer,
        [JsonProperty("practice")]
        practice
    }

    public class SongStartEvent {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonProperty("mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public GameMode Mode { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("subName")]
        public string SubName { get; set; } = string.Empty;

        [JsonProperty("authorName")]
        public string AuthorName { get; set; } = string.Empty;

        [JsonProperty("artist")]
        public string Artist { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty; // Standard, Lawless, OneSaber etc

        [JsonProperty("hash")]
        public string Hash { get; set; } = string.Empty;

        [JsonProperty("duration")]
        public int Duration { get; set; } // Song duration in seconds

        [JsonProperty("difficulty")]
        public int Difficulty { get; set; } = -1; // Difficulty, 0-9, odd numba

        [JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
        public int? StartTime { get; set; } // Start time if in practice mode

        [JsonProperty("playSpeed", NullValueHandling = NullValueHandling.Ignore)]
        public double? PlaySpeed { get; set; } // Playback speed, from either practice mode or speed modifies


        public SongStartEvent(string timestamp, GameMode mode, string name, string subName, string authorName, string artist, string type, string hash, int duration, int difficulty, int? startTime, double? playSpeed) {
            Timestamp = timestamp;
            Mode = mode;
            Name = name;
            SubName = subName;
            AuthorName = authorName;
            Artist = artist;
            Type = type;
            Hash = hash;
            Duration = duration;
            Difficulty = difficulty;
            StartTime = startTime;
            PlaySpeed = playSpeed;
        }
    }

    public enum PauseType {
        [JsonProperty("pause")]
        Pause,
        [JsonProperty("unpause")]
        Unpause
    }

    public class PauseUnpauseEvent {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonProperty("songTime")]
        public double SongTime { get; set; } // Time since start of song in seconds

        [JsonProperty("eventType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PauseType EventType { get; set; }
    }
}
