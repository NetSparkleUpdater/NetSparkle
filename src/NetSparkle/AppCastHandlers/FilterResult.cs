using System.Collections.Generic;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <summary>
    /// Result of the filtering process.  
    /// </summary>
    public struct FilterResult
    {
        /// <summary>
        /// When true this indicates to NetSparkle that the highest AppCastItem in the FilteredAppCastItems list is the
        /// target for installation.  When false, the results of the FilteredAppCastItems are ignored completely (and can
        /// even be null). 
        /// </summary>
        public readonly bool ForceInstallOfLatestInFilteredList;
        /// <summary>
        /// The filtered list of AppCastItem objects.  This can be null if DetectVersionFromFilteredList is false,
        /// otherwise it must be a list of the valid AppCastItem instances that can be installed.  
        /// </summary>
        public readonly IEnumerable<AppCastItem> FilteredAppCastItems;

        /// <summary>
        /// Construct a FilterResult. 
        /// </summary>
        /// <param name="filteredAppCastItems"></param>
        public FilterResult(IEnumerable<AppCastItem> filteredAppCastItems)
        {
            ForceInstallOfLatestInFilteredList = false;
            FilteredAppCastItems = filteredAppCastItems;
        }

        /// <summary>
        /// Construct a FilterResult. 
        /// </summary>
        /// <param name="forceInstallOfLatestInFilteredList"></param>
        /// <param name="filteredAppCastItems"></param>
        public FilterResult(IEnumerable<AppCastItem> filteredAppCastItems, bool forceInstallOfLatestInFilteredList)
        {
            FilteredAppCastItems = filteredAppCastItems;
            ForceInstallOfLatestInFilteredList = forceInstallOfLatestInFilteredList;
        }
    }
}