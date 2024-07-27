using System;

namespace NetSparkleUpdater.Interfaces
{
    /// <summary>
    /// Interface for UIs that tell the user that <see cref="SparkleUpdater"/> 
    /// is checking for updates
    /// </summary>
    public interface ICheckingForUpdates
    {
        /// <summary>
        /// Event to fire when the checking for updates UI is closing
        /// </summary>
        event EventHandler? UpdatesUIClosing;

        /// <summary>
        /// Show the checking for updates UI
        /// </summary>
        void Show();

        /// <summary>
        /// Close the window/UI that shows the checking for updates UI
        /// </summary>
        void Close();
    }
}
