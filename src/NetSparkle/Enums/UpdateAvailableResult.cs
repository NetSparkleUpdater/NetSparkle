namespace NetSparkleUpdater.Enums
{
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
        /// User chose to install the update immediately.
        /// </summary>
        InstallUpdate,

        /// <summary>
        /// User chose to skip the update.
        /// </summary>
        SkipUpdate,

        /// <summary>
        /// User chose to remind them later about this update.
        /// </summary>
        RemindMeLater
    }
}
