using System;
using System.Collections.Generic;
using System.Text;

namespace NetSparkleUpdater.UI.Avalonia.Interfaces
{
    /// <summary>
    /// Interface for objects that have the ability to show release notes to the user.
    /// Used by the UpdateAvailableWindow and its view model to coordinate when the
    /// release notes should be shown.
    /// </summary>
    public interface IReleaseNotesUpdater
    {
        /// <summary>
        /// Show the given release notes to the user
        /// </summary>
        /// <param name="notes">string of HTML release notes to show to the end-user</param>
        void ShowReleaseNotes(string notes);
    }
}
