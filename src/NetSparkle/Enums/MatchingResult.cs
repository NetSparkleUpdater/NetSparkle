using NetSparkleUpdater.Interfaces;

namespace NetSparkleUpdater.Enums
{
    /// <summary>
    /// Provides the return values for the GetAvailableUpdates call on the IAppCastHandler.  When an appcast is downloaded,
    /// the IAppCastHandler will work out which AppCastItem instances match criteria for an update.  
    /// </summary>
    /// <seealso cref="IAppCastHandler.GetAvailableUpdates"/>
    public enum MatchingResult
    {
        /// <summary>
        /// Indicates that the AppCastItem is a validate candidate for installation.
        /// </summary>
        MatchOk = 0,
        /// <summary>
        /// The AppCastItem is for a different operating system than this one, and must be ignored.
        /// </summary>
        NotThisPlatform = 1,
        /// <summary>
        /// The version indicated by the AppCastItem is less than or equal to the currently installed/running version.
        /// </summary>
        VersionIsOlderThanCurrent = 2,
        /// <summary>
        /// A signature is required to validate the item - but it's missing from the AppCastItem
        /// </summary>
        SignatureIsMissing = 3
    }
}
