using System;
using static ScoreSaber.UI.Leaderboard.ScoreSaberLeaderboardViewController;

namespace ScoreSaber.Core.Daemons {
    internal interface IUploadDaemon {
        bool uploading { get; set; }
        event Action<UploadStatus, string> UploadStatusChanged;
    }
}
