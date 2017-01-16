using System;
using System.Windows.Forms;

namespace NetSparkle.Interfaces
{
    /// <summary>
    /// Interface for UI element that shows the release notes, 
    /// and the skip, install, and later buttons
    /// </summary>
    public interface INetSparkleForm
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
        /// Gets the result for skip, later, or install
        /// </summary>
        /// <value>Valid results are install = yes, skip = no, later = retry</value>
        DialogResult Result { get; }

        /// <summary>
        /// Gets or sets the current item being installed
        /// </summary>
        NetSparkleAppCastItem CurrentItem { get; }

        /// <summary>
        /// Brings the form to the front of all windows
        /// </summary>
        void BringToFront();

        /// <summary>
        /// Close the form
        /// </summary>
        void Close();
    }
}
