using System;
using HarmonyLib;
using Newtonsoft.Json.Converters;
#nullable enable
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
using ScoreSaber.Core.Phoenix;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Http;

namespace ScoreSaber.Core.Services {
    internal class RichPresenceService : IDisposable {
        private readonly SiraLog _log;
        private readonly PlayerService _playerService;
        private readonly ScoreSaberHttpClient _client;
        private Socket? _socket;
        private bool _isDisposed;
        private bool _isEnabled = true;
        private Channel? _userProfileChannel;

        private const int MaxRetries = 5;
        private int _retryAttempt = 0;
        private const string _protocolAndSubdomain = "wss://realtime.";
        private const string _socketAddress = "/socket"; // change to scoresaber subdomain once ready. wss://realtime.scoresaber.com/socket

        public string TimeRightNow => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss zzz");

        public RichPresenceService(SiraLog log,
                                   PlayerService playerService,
                                   ScoreSaberHttpClient client) {
            _log = log;
            _playerService = playerService;
            _client = client;
        }


        public void Initialize() {
            if (_socket != null) {
                _log.Warn("Socket already initialized.");
                return;
            }

            DisconnectSocket();

            if (!Plugin.Settings.enableRichPresence) {
                _log.Warn("Rich presence is disabled in settings.");
                return;
            }

            try {
                var socketOptions = new Socket.Options(new JsonMessageSerializer());
                var socketFactory = new WebsocketSharpFactory();
                _socket = new Socket(
                    new Http.Endpoints.Realtime.SocketPath().BuildUrl(),
                    new System.Collections.Generic.Dictionary<string, string>(),
                    socketFactory,
                    socketOptions
                );

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
}
