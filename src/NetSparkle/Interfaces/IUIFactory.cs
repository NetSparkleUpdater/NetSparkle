using System;
using System.Collections.Generic;

namespace NetSparkleUpdater.Interfaces
{
    /// <summary>
    /// Universal interface for creating UI utilized by SparkleUpdater
    /// </summary>
    public interface IUIFactory
    {
        /// <summary>
        /// Create sparkle form implementation. This is the form that tells the user that an update is available, shows changelogs if necessary, etc.
        /// </summary>
        /// <param name="sparkle">The <see cref="SparkleUpdater"/> instance controlling this UI</param>
        /// <param name="updates">Sorted array of updates from latest to previous</param>
        /// <param name="isUpdateAlreadyDownloaded">If true, make sure UI text shows that the user is about to install the file instead of download it.</param>
        IUpdateAvailable CreateUpdateAvailableWindow(SparkleUpdater sparkle, List<AppCastItem> updates, bool isUpdateAlreadyDownloaded = false);

        /// <summary>
        /// Create the download progress window
        /// </summary>
        /// <param name="sparkle">The <see cref="SparkleUpdater"/> instance controlling this UI</param>
        /// <param name="item">Appcast item to download</param>
        IDownloadProgress CreateProgressWindow(SparkleUpdater sparkle, AppCastItem item);

        /// <summary>
        /// Inform user in some way that NetSparkle is checking for updates
        /// </summary>
        /// <param name="sparkle">The <see cref="SparkleUpdater"/> instance controlling this UI</param>
        ICheckingForUpdates ShowCheckingForUpdates(SparkleUpdater sparkle);

        /// <summary>
        /// Initialize UI. Called when Sparkle is constructed and/or when the UIFactory is set.
        /// </summary>
        /// <param name="sparkle">The <see cref="SparkleUpdater"/> instance controlling this UI</param>
        void Init(SparkleUpdater sparkle);

        /// <summary>
        /// Show user a message saying downloaded update format is unknown
        /// </summary>
        /// <param name="sparkle">The <see cref="SparkleUpdater"/> instance controlling this UI</param>
        /// <param name="downloadFileName">file name for the download</param>
        void ShowUnknownInstallerFormatMessage(SparkleUpdater sparkle, string downloadFileName);

        /// <summary>
        /// Show user that current installed version is up-to-date
        /// </summary>
        /// <param name="sparkle">The <see cref="SparkleUpdater"/> instance controlling this UI</param>
        void ShowVersionIsUpToDate(SparkleUpdater sparkle);

        /// <summary>
        /// Show message that latest update was skipped by user
        /// </summary>
        /// <param name="sparkle">The <see cref="SparkleUpdater"/> instance controlling this UI</param>
        void ShowVersionIsSkippedByUserRequest(SparkleUpdater sparkle);

        /// <summary>
        /// Show message that appcast is not available
        /// </summary>
        /// <param name="sparkle">The <see cref="SparkleUpdater"/> instance controlling this UI</param>
        /// <param name="appcastUrl">The URL to the app cast file</param>
        void ShowCannotDownloadAppcast(SparkleUpdater sparkle, string? appcastUrl);

        /// <summary>
        /// See if this UIFactory can show toast messages
        /// </summary>
        /// <param name="sparkle">The <see cref="SparkleUpdater"/> instance controlling this UI</param>
        /// <returns>true if the UIFactory can show for toast messages; false otherwise</returns>
        bool CanShowToastMessages(SparkleUpdater sparkle);

        /// <summary>
        /// Show 'toast' window to notify new version is available
        /// </summary>
        /// <param name="sparkle">The <see cref="SparkleUpdater"/> instance controlling this UI</param>
        /// <param name="updates">Appcast updates</param>
        /// <param name="clickHandler">handler for click</param>
        void ShowToast(SparkleUpdater sparkle, List<AppCastItem> updates, Action<List<AppCastItem>>? clickHandler);

        /// <summary>
        /// Show message on download error
        /// </summary>
        /// <param name="sparkle">The <see cref="SparkleUpdater"/> instance controlling this UI</param>
        /// <param name="message">Error message from exception</param>
        /// <param name="appcastUrl">the URL for the appcast file</param>
        void ShowDownloadErrorMessage(SparkleUpdater sparkle, string message, string? appcastUrl);

        /// <summary>
        /// Shut down the UI so we can run an update.
        /// If in WPF, System.Windows.Application.Current.Shutdown().
        /// If in WinForms, Application.Exit().
        /// If in Avalonia, shuts down the current application lifetime if it
        /// implements IClassicDesktopStyleApplicationLifetime.
        /// </summary>
        /// <param name="sparkle">The <see cref="SparkleUpdater"/> instance controlling this UI</param>
        void Shutdown(SparkleUpdater sparkle);

        /// <summary>
        /// Hides the release notes view when an update is found.
        /// </summary>
        bool HideReleaseNotes { get; set; }

        /// <summary>
        /// Hides the skip this update button when an update is found.
        /// </summary>
        bool HideSkipButton { get; set; }

        /// <summary>
        /// Hides the remind me later button when an update is found.
        /// </summary>
        bool HideRemindMeLaterButton { get; set; }

        /// <summary>
        /// The HTML template to use for each changelog, version, etc. for every app cast
        /// item update. If you set this to "" or null, the default ReleaseNotesGrabber will use
        /// the default template.
        /// To work properly, you MUST have 3 placeholders in the template ({0}, {1}, {2}, {3}), 
        /// as this will be used in a string.Format() call.
        /// The only exception to this would be if you implement your own <see cref="ReleaseNotesGrabber"/>
        /// subclass that overrides 
        /// <see cref="ReleaseNotesGrabber.DownloadAllReleaseNotes(List{AppCastItem}, AppCastItem, System.Threading.CancellationToken)"/>.
        /// <para/>
        /// {0} = app cast item version;
        /// {1} = app cast publication date;
        /// {2} = the actual release notes;
        /// {3} = the background color for the release notes header.
        /// </summary>
        string? ReleaseNotesHTMLTemplate { get; set; }

        /// <summary>
        /// Any additional header information to stick in the HTML head element
        /// that will show up in the release notes (e.g. styles, etc.).
        /// Must be HTML formatted to work properly.
        /// Can be null or "".
        /// </summary>
        string? AdditionalReleaseNotesHeaderHTML { get; set; }
    }
}
