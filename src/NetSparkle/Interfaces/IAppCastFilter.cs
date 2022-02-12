using System;
using System.Collections.Generic;
using System.Text;

namespace NetSparkleUpdater.Interfaces
{
    /// <summary>
    /// This provides a way to filter out AppCast item instances that do not apply to the current update process.  Originally
    /// used to make it possible to revert from a "beta" version to a "stable" one - where the current runtime System.Version value 
    /// is by definition higher than anything in the "stable" app cast list.
    /// 
    /// To force NetSparkle to upgrade to a lower version number, return a System.Version("0.0.0.0") and a list of app cast items that are valid candidates to update - the
    /// GetFilteredAppCastItems() method allows you to remove elements from this list before the standard NetSparkle code does its thing.  
    /// 
    /// </summary>
    public interface IAppCastFilter
    {
        /// <summary>
        /// Returns a new "minimum" version number and a potentially modified list of app cast items, e.g. maybe you want to remove the beta ones when reverting to stable.
        /// </summary>
        /**
            <example>
            This code shows how to do no filtering at all.
            <code>
            if(noFilteringRequired)
            {
                return new Tuple&lt;installed, List&lt;AppCastItem&gt;&gt;(installed, items);
            }
            </code> 
            </example>
        */
        /// <remarks>The app must return a version and list of app cast items.  If there is no need to filter then simply return the values that were passed in.</remarks>
        /// <remarks>Note: calls to methods on this interface are made from background threads - dont access UI objects from within this method</remarks>
        /// <param name="installed">The currently detected version of this application</param>
        /// <param name="items">The current set of AppCastItem objects</param>
        /// <returns>A replacement list of items derived from the input set</returns>
        Tuple<Version, List<AppCastItem>> GetFilteredAppCastItems(Version installed, List<AppCastItem> items);
    }
}
