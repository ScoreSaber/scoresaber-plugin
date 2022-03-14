using ScoreSaber.UI.ViewControllers;
using System;

namespace ScoreSaber.Core.Daemons {
    internal class MockUploadDaemon : IUploadDaemon {
        public bool uploading { get; set; }
        public event Action<ScoreSaberLeaderboardViewController.UploadStatus, string> UploadStatusChanged;
    }
}
