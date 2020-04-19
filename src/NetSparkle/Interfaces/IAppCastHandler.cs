using NetSparkleUpdater.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetSparkleUpdater.Interfaces
{
    public interface IAppCastHandler
    {
        void SetupAppCastHandler(IAppCastDataDownloader dataDownloader, string castUrl, Configuration config, DSAChecker dsaChecker, LogWriter logWriter = null);
        /// <summary>
        /// Download the app cast file via an IAppCastDataDownloader object and parse it.
        /// If this function is successful, NetSparkle will call GetNeededUpdates() to
        /// get the AppCastItem information.
        /// </summary>
        /// <returns>true if downloading and parsing succeeded; false otherwise</returns>
        bool DownloadAndParse();
        List<AppCastItem> GetAvailableUpdates();
    }
}
