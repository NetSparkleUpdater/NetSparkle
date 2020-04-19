using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater.Enums
{
    /// <summary>
    /// Allows for updating the application with or without user interaction.
    /// </summary>
    public enum UserInteractionMode
    {
        /// <summary>
        /// Shows the change log UI automatically (this is the default)
        /// </summary>
        NotSilent,
        /// <summary>
        /// Downloads the latest update file and changelog automatically, but does not
        /// show any UI until asked to show UI.
        /// </summary>
        DownloadNoInstall,
        /// <summary>
        /// Downloads the latest update file and automatically runs it as an installer file.
        /// <para>WARNING: if you don't tell the user that the application is about to quit
        /// to update/run an installer, this setting might be quite the shock to the user!
        /// Make sure to implement AboutToExitForInstallerRun or AboutToExitForInstallerRunAsync
        /// so that you can show your users what is about to happen.</para>
        /// </summary>
        DownloadAndInstall,
    }
}
