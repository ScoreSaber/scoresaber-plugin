#region

using System;
using static ScoreSaber.UI.Leaderboard.ScoreSaberLeaderboardViewController;

#endregion

namespace ScoreSaber.Core.Daemons {
    internal interface IUploadDaemon {
        bool Uploading { get; set; }
        event Action<UploadStatus, string> UploadStatusChanged;
    }
}