using System;
using System.Drawing;

namespace NetSparkle.Interfaces
{
    /// <summary>
    /// Universal interface for creating UI utilized by Sparkle
    /// </summary>
    public interface INetSparkleUIFactory
    {
        /// <summary>
        /// Create sparkle form implementation
        /// </summary>
        /// <param name="currentItem">App cast item to show</param>
        /// <param name="applicationIcon">Icon</param>
        /// <returns></returns>
        INetSparkleForm CreateSparkleForm(NetSparkleAppCastItem currentItem, Icon applicationIcon);

        /// <summary>
        /// Create download progress window
        /// </summary>
        /// <param name="item">Appcast item to download</param>
        /// <param name="applicationIcon">Application icon to use</param>
        /// <returns></returns>
        INetSparkleDownloadProgress CreateProgressWindow(NetSparkleAppCastItem item, Icon applicationIcon);

        /// <summary>
        /// Initialize UI. Called when Sparkle is constructed.
        /// </summary>
        void Init();

        /// <summary>
        /// Show user a message saying downloaded update format is unknown
        /// </summary>
        /// <param name="downloadFileName"></param>
        void ShowUnknownInstallerFormatMessage(string downloadFileName);

        /// <summary>
        /// Show user that current installed version is up-to-date
        /// </summary>
        void ShowVersionIsUpToDate();

        /// <summary>
        /// Show message that latest update was skipped by user
        /// </summary>
        void ShowVersionIsSkippedByUserRequest();

        /// <summary>
        /// Show message that appcast is not available
        /// </summary>
        /// <param name="appcastUrl"></param>
        void ShowCannotDownloadAppcast(string appcastUrl);

        /// <summary>
        /// Show 'toast' window to notify new version is available
        /// </summary>
        /// <param name="item">Appcast item</param>
        /// <param name="applicationIcon">Icon to use in window</param>
        /// <param name="clickHandler">handler for click</param>
        void ShowToast(NetSparkleAppCastItem item, Icon applicationIcon, EventHandler clickHandler);

        /// <summary>
        /// Show message on download error
        /// </summary>
        /// <param name="message">Error message from exception</param>
        /// <param name="appCastUrl"></param>
        void ShowDownloadErrorMessage(string message, string appCastUrl);
    }
}
