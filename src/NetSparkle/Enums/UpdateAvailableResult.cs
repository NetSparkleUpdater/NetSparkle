using NetSparkleUpdater.Interfaces;

namespace NetSparkleUpdater.Enums
{
    /// <summary>
    /// Possible result values for <see cref="IUpdateAvailable"/> implementation.
    /// </summary>
    public enum UpdateAvailableResult
    {
        /// <summary>
        /// No result specified. Default value.
        /// </summary>
        None = 0,

        /// <summary>
        /// User chose to install the update immediately.
        /// </summary>
        InstallUpdate,

        /// <summary>
        /// User chose to skip the update.
        /// </summary>
        SkipUpdate,

        /// <summary>
        /// User chose to remind them later about this update (e.g. close for now, but 
        /// feel free to tell me about it next time that the software checks for updates).
        /// </summary>
        RemindMeLater
    }
}
