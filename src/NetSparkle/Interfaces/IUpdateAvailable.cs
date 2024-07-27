using NetSparkleUpdater.Enums;

namespace NetSparkleUpdater.Interfaces
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
        event UserRespondedToUpdate? UserResponded;

        /// <summary>
        /// Show the UI that displays release notes, etc.
        /// </summary>
        void Show(bool IsOnMainThread);

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
        /// Gets the user choice on how to handle this update (e.g. skip, remind me later)
        /// </summary>
        UpdateAvailableResult Result { get; }

        /// <summary>
        /// Gets or sets the current item being installed 
        /// (the item that the user should update to)
        /// </summary>
        AppCastItem CurrentItem { get; }

        /// <summary>
        /// Brings the update info UI to the front of all windows
        /// </summary>
        void BringToFront();

        /// <summary>
        /// Close the UI that shows update information
        /// </summary>
        void Close();
    }
}
