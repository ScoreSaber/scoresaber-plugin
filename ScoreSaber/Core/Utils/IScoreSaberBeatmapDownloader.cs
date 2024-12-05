using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSaber.Core.Utils {
    // BIND THIS FROM AN INSTANCE TO THE APP CONTAINER, SCORESABER USES THIS TO DOWNLOAD BEATMAPS FROM ANY SOURCE
    public interface IScoreSaberBeatmapDownloader {
        Task<bool> DownloadBeatmapAsync(string hash, Action afterRefreshCallback = null);
    }
}
