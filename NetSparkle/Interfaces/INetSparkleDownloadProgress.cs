using System.Net;
using System;

namespace NetSparkle.Interfaces
{
    /// <summary>
    /// Interface for UI element that shows the progress bar
    /// and a method to install and relaunch the appliction
    /// </summary>
    public interface INetSparkleDownloadProgress
    {
        /// <summary>
        /// event to fire when the form asks the application to be relaunched
        /// </summary>
        event EventHandler InstallAndRelaunch;

        /// <summary>
        /// Enable or disable the download and install button (such as when your "Can I gracefully close the window?" function is async and you don't
        /// want your user to click the button multiple times)
        /// </summary>
        /// <param name="shouldBeEnabled">True if the button should be enabled; false otherwise</param>
        void SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled);

        /// <summary>
        /// Show the UI and waits
        /// </summary>
        void ShowDialog();

        /// <summary>
        /// Called when the download progress changes
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">used to resolve the progress of the download. Also contains the total size of the download</param>
        void OnClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e);

        /// <summary>
        /// Force window close
        /// </summary>
        void ForceClose();

        /// <summary>
        /// Update UI to show file is downloaded and signature check result
        /// </summary>
        void ChangeDownloadState();
    }
}
