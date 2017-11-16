using System;

namespace NetSparkle.Interfaces
{
    /// <summary>
    /// Interface for UI element that shows the release notes, 
    /// and the skip, install, and later buttons
    /// </summary>
    public interface IUpdateAvailable
    {
        /// <summary>
        /// Event fired when the user has responded to the 
        /// skip, later, install question.
        /// </summary>
        event EventHandler UserResponded;

        /// <summary>
        /// Show the UI
        /// </summary>
        void Show();

        /// <summary>
        /// Hides the release notes 
        /// </summary>
        void HideReleaseNotes();

        /// <summary>
        /// Hides the remind me later button
        /// </summary>
        void HideRemindMeLaterButton();

        /// <summary>
        /// Hides the skip update button
        /// </summary>
        void HideSkipButton();

        /// <summary>
        /// Gets the result for skip, later, or install
        /// </summary>
        UpdateAvailableResult Result { get; }

        /// <summary>
        /// Gets or sets the current item being installed
        /// </summary>
        AppCastItem CurrentItem { get; }

        /// <summary>
        /// Brings the form to the front of all windows
        /// </summary>
        void BringToFront();

        /// <summary>
        /// Close the form
        /// </summary>
        void Close();
    }

    /// <summary>
    /// Possible Result values for IUpdateAvailable implementation.
    /// </summary>
    public enum UpdateAvailableResult
    {
        /// <summary>
        /// No result specified. Default value.
        /// </summary>
        None = 0,

        /// <summary>
        /// User chose to install the update immediatelly.
        /// </summary>
        InstallUpdate,

        /// <summary>
        /// Used chose to skip the update.
        /// </summary>
        SkipUpdate,

        /// <summary>
        /// User chose to remind her later.
        /// </summary>
        RemindMeLater
    }
}
