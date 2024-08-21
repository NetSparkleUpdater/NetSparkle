using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetSparkleUpdater.Interfaces
{
    /// <summary>
    /// Universal interface for creating UI utilized by SparkleUpdater
    /// </summary>
    public interface IUIFactory
    {
        /// <summary>
        /// Create window for showing updates that are available along with change logs.
        /// </summary>
        /// <param name="updates">Sorted array of updates from latest to previous</param>
        /// <param name="signatureVerifier"><seealso cref="ISignatureVerifier"/> to verify release note signatures (if needed)</param>
        /// <param name="currentVersion">Version string for the currently running/available version of the application</param>
        /// <param name="appName">Name of the application</param>
        /// <param name="isUpdateAlreadyDownloaded">If true, make sure UI text shows that the user is about to install the file instead of download it.</param>
        IUpdateAvailable CreateUpdateAvailableWindow(List<AppCastItem> updates, ISignatureVerifier? signatureVerifier, 
            string currentVersion = "", string appName = "the application", bool isUpdateAlreadyDownloaded = false);

        /// <summary>
        /// Create the window that shows the download progress to the user. Also allows users to cancel download
        /// and/or install the downloaded update.
        /// </summary>
        /// <param name="downloadTitle">Title shown at top of download window.
        /// Typically something like "Downloading " + your application name + the version of your application</param>
        /// <param name="actionButtonTitleAfterDownload">Button text for after download is finished.
        /// If the software will be rebooted after install, set to something like "Install and Relaunch". 
        /// Otherwise, "Install" works fine.</param>
        IDownloadProgress CreateProgressWindow(string downloadTitle, string actionButtonTitleAfterDownload);

        /// <summary>
        /// Inform user in some way that NetSparkle is checking for updates
        /// </summary>
        ICheckingForUpdates ShowCheckingForUpdates();

        /// <summary>
        /// Show user a message saying downloaded update format is unknown
        /// </summary>
        /// <param name="downloadFileName">file name for the download</param>
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
        /// <param name="appcastUrl">The URL to the app cast file</param>
        void ShowCannotDownloadAppcast(string? appcastUrl);

        /// <summary>
        /// See if this UIFactory can show toast messages
        /// </summary>
        /// <returns>true if the UIFactory can show for toast messages; false otherwise</returns>
        bool CanShowToastMessages();

        /// <summary>
        /// Show 'toast' window to notify new version is available (e.g. in bottom right corner of screen)
        /// </summary>
        /// <param name="clickHandler">Click handler to call when the user clicks the toast window
        /// (the core library uses this to show the update available window when clicked). Make sure to
        /// call this handler when the user clicks on your window if you want them to be able to pull up
        /// the update available window to download and install the update.</param>
        void ShowToast(Action clickHandler);

        /// <summary>
        /// Show message on download error
        /// </summary>
        /// <param name="message">Error message from exception</param>
        /// <param name="appcastUrl">the URL for the appcast file</param>
        void ShowDownloadErrorMessage(string message, string? appcastUrl);

        /// <summary>
        /// Shut down the UI so we can run an update.
        /// If in WPF, System.Windows.Application.Current.Shutdown().
        /// If in WinForms, Application.Exit().
        /// If in Avalonia, shuts down the current application lifetime like this or similar:
        /// (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
        /// </summary>
        void Shutdown();

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
        /// To work properly, you MUST have 4 placeholders in the template ({0}, {1}, {2}, {3}), 
        /// as this will be used in a string.Format() call.
        /// The only exception to this would be if you implement your own <see cref="ReleaseNotesGrabber"/>
        /// subclass that overrides 
        /// <see cref="ReleaseNotesGrabber.DownloadAllReleaseNotes(List{AppCastItem}, AppCastItem, System.Threading.CancellationToken)"/>.
        /// <para/>
        /// {0} = app cast item title (incl. version);
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
