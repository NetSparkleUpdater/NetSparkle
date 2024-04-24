namespace NetSparkleUpdater.Enums
{
    /// <summary>
    /// Failures that could happen as a result of calling SparkleUpdater.InstallUpdate
    /// </summary>
    public enum InstallUpdateFailureReason
    {
        /// <summary>
        /// The installer file failed its signature validation check
        /// </summary>
        InvalidSignature = 0,
        /// <summary>
        /// The installer file path points to a file path that does not exist or is null
        /// </summary>
        FileNotFound = 1,
        /// <summary>
        /// Installer command couldn't be built, e.g. due to an invalid installer file type that is
        /// not supported
        /// </summary>
        CouldNotBuildInstallerCommand = 2,
        /// <summary>
        /// Install was canceled by user via an event, e.g. InstallerProcessAboutToStart
        /// </summary>
        CanceledByUserViaEvent = 3,
    }
}