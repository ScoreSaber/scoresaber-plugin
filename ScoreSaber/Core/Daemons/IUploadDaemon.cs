using System;

namespace ScoreSaber.Core.Daemons {
    internal interface IUploadDaemon {
        bool uploading { get; set; }
        event Action<UploadStatus, string> UploadStatusChanged;
    }

    internal enum UploadStatus {
        Uploading,
        Success,
        Error
    }
}
