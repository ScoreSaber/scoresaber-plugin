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

        private Socket _socket;
        private bool _isDisposed;
        private bool _isEnabled = true;
        private Channel _userProfileChannel;

        private const int MaxRetries = 5;
        private int _retryAttempt = 0;

        public string TimeRightNow => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

        public void Initialize() {
            if (_socket != null) {
                _log.Warn("Socket already initialized.");
                return;
            }

            DisconnectSocket();

            if (Plugin.Settings.enableRichPresence == false) {
                _log.Warn("Rich presence is disabled in settings.");
                return;
            }

            try {
                var socketOptions = new Socket.Options(new JsonMessageSerializer());
                var socketAddress = "wss://ssrt.bzaz.au/socket"; // change to scoresaber subdomain once ready.
                var socketFactory = new WebsocketSharpFactory();
                _socket = new Socket(socketAddress, new System.Collections.Generic.Dictionary<string, string>
                {
                { "sid", _playerService.localPlayerInfo.serverKey }
                }, socketFactory, socketOptions);

                _socket.OnOpen += OnOpenCallback;
                _socket.OnClose += OnCloseCallback;
                _socket.Connect();
                _log.Notice("Rich presence service initialized.");
            } catch (Exception ex) {
                _log.Error($"Failed to initialize socket: {ex.Message}");
            }
        }

        public void ToggleRichPresence(bool enable) {
            if (_isEnabled == enable) {
                _log.Info("Rich presence toggle state unchanged.");
                return;
            }

            _isEnabled = enable;

            if (_isEnabled) {
                _log.Info("Enabling rich presence service...");
                Initialize();
            } else {
                _log.Info("Disabling rich presence service...");
                DisconnectSocket();
            }
        }

        private void DisconnectSocket() {
            if (_socket != null) {
                _socket.Disconnect();
                _socket.OnOpen -= OnOpenCallback;
                _socket.OnClose -= OnCloseCallback;
                _socket = null;
                _userProfileChannel = null;
                _log.Info("Socket disconnected. Service is now offline.");
            }
        }

        private void SendMenuEvent() {
            if (_playerService?.localPlayerInfo == null) {
                _log.Warn("Local player info is null, unable to send rich presence data.");
                return;
            }

            if (_socket == null) {
                _log.Warn("Socket is null, unable to send rich presence data.");
                return;
            }

            var jsonObject = new SceneChangeEvent {
                Timestamp = TimeRightNow,
                Scene = Scene.menu
            };

            SendUserProfileChannel("scene_change", jsonObject);
        }

        private void OnCloseCallback(ushort code, string message) {
            _log.Warn($"Rich presence socket closed: {code} - {message}");
            if (_isDisposed) return;

            if (_retryAttempt >= MaxRetries) {
                _log.Error("Max reconnection attempts reached. Unable to reconnect.");
                Dispose();
                return;
            }

            _retryAttempt++;
            _log.Warn($"Attempting to reconnect to rich presence socket... Attempt {_retryAttempt}");
            _socket?.Connect();
        }

        public void SendUserProfileChannel(string @event, object message, TimeSpan? timeSpan = null) {
            if (_userProfileChannel == null) {
                _log.Warn("User profile channel is null, unable to send message.");
                return;
            }

            _userProfileChannel.Push(@event, message, timeSpan);
        }

        private void OnOpenCallback() {
            _log.Notice("Connected to the rich presence socket.");

            if (_playerService?.localPlayerInfo == null) {
                _log.Warn("Local player info is null, unable to send rich presence data.");
                return;
            }

            if (_socket == null) {
                _log.Warn("Socket is null, unable to send rich presence data.");
                return;
            }

            _userProfileChannel?.Leave();
            _userProfileChannel = _socket.Channel("player");

            _userProfileChannel.Join();
            SendMenuEvent();
        }

        public void Dispose() {
            if (_isDisposed) return;

            _isDisposed = true;
            DisconnectSocket();
            _log.Info("Rich presence service disposed.");
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
        [JsonProperty("offline")]
        offline,
        [JsonProperty("online")]
        online,
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

    public class SongRichPresenceInfo {
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


        public SongRichPresenceInfo(string timestamp, GameMode mode, string name, string subName, string authorName, string artist, string type, string hash, int duration, int difficulty, int? startTime, double? playSpeed) {
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


    public class RichPresenceResponse {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("state")]
        public State state { get; set; }
    }

    public class State {
        [JsonProperty("scene")]
        public Scene Scene { get; set; } = Scene.menu;

        [JsonProperty("currentMap")]
        public SongRichPresenceInfo currentMap { get; set; }
    }
}
