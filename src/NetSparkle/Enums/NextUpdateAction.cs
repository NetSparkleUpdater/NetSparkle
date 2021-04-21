namespace NetSparkleUpdater.Enums
{
    /// <summary>
    /// Every time when <see cref="SparkleUpdater"/> detects an update, the
    /// consumer can decide what should happen next with the help
    /// of the <see cref="UpdateDetected"/> event
    /// </summary>
    public enum NextUpdateAction
    {
        /// <summary>
        /// Show the user interface
        /// </summary>
        ShowStandardUserInterface = 1,
        /// <summary>
        /// Perform an unattended install
        /// </summary>
        PerformUpdateUnattended = 2,
        /// <summary>
        /// Prohibit (don't allow) the update
        /// </summary>
        ProhibitUpdate = 3
    }
}
