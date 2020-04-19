using NetSparkleUpdater.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetSparkleUpdater.Interfaces
{
    public interface IAppCastHandler
    {
        /// <summary>
        /// Setup the app cast handler for downloading and parsing app cast information
        /// </summary>
        /// <param name="dataDownloader">downloader that will manage the app cast download</param>
        /// <param name="castUrl">URL to the app cast file</param>
        /// <param name="config">configuration for handling update intervals/checks 
        /// (user skipped versions, etc.)</param>
        /// <param name="dsaChecker">Object to check DSA signatures</param>
        /// <param name="logWriter">object that you can utilize to do any necessary logging</param>
        void SetupAppCastHandler(IAppCastDataDownloader dataDownloader, string castUrl, Configuration config,
            ISignatureVerifier dsaChecker, LogWriter logWriter = null);
        /// <summary>
        /// Download the app cast file via an IAppCastDataDownloader object and parse it.
        /// If this function is successful, NetSparkle will call GetNeededUpdates() to
        /// get the AppCastItem information.
        /// </summary>
        /// <returns>true if downloading and parsing succeeded; false otherwise</returns>
        bool DownloadAndParse();
        /// <summary>
        /// Retrieve the available updates from the app cast.
        /// This should be called after DownloadAndParse() has
        /// successfully completed.
        /// </summary>
        /// <returns>a list of AppCastItem updates. Can be empty if no updates available.</returns>
        List<AppCastItem> GetAvailableUpdates();
    }
}
