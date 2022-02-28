using System;
using System.Collections.Generic;

namespace NetSparkleUpdater.Interfaces
{
    /// <summary>
    /// Provides a way to filter out AppCast item instances that do not apply to the current update process.  
    /// Used to make it possible to revert from a "beta" version to a "stable" one - where the current runtime
    /// System.Version value of the <seealso cref="Configurations">Configurations</seealso> is by definition higher
    /// than anything in the "stable" app cast list.
    /// 
    /// To indicate to NetSparkle that you want to upgrade to a lower version number, return true
    /// in GetFilteredAppCastItems and filter out any AppCastItem elements from the provided list of items that are
    /// higher than the version you want to install.  When you return true, NetSparkle will force re-install
    /// the highest app cast element held in this list.
    ///
    /// If you only want filtering; then return false from the method but make sure to return a filtered set of app cast items.
    /// </summary>
    public interface IAppCastFilter
    {
        /// <summary>
        /// Returns a filtered list of AppCastItem elements.  The filtering system result can also be used to
        /// indicate that a forced install of the latest version is required in the situation where a rollback is
        /// being performed.
        /// </summary>
        /// <example>
        /// This code shows how to do no filtering at all, by returning false for the first parameter of the
        /// FilterResult object.  
        /// <code>
        /// if(noFilteringRequired)
        /// {
        ///     return new FilterResult(false);
        /// }
        /// else
        /// {
        ///    // remove all beta items from the app cast we were given, if the download link contains the word
        ///    // beta then we strip that element out.
        ///    List&lt;AppCastItem&gt; itemsWithoutBeta = items.Where((item) =&gt;
        ///    {
        ///        if (item.DownloadLink.Contains("/beta/"))
        ///            return false;
        ///        return true;
        ///    }).ToList();
        ///
        ///    // note: returning true here will FORCE the installation of whatever is the latest version in itemsWithoutBeta
        ///    return new FilterResult(true, itemsWithoutBeta); 
        /// }
        /// </code> 
        /// </example>
        ///  <remarks>You must always return a list of filtered items - if null is returned then the whole method is treated as a no-op</remarks>
        ///  <remarks>This methods being called on a background thread - do not access UI objects directly from within this method</remarks>
        ///  <param name="installed">The currently detected version of this application</param>
        ///  <param name="items">The current set of AppCastItem objects</param>
        ///  <returns>A FilterResult instance.  The bool indicates whether or not NetSparkle should force install the latest
        ///  version in the resulting app cast list, and the List&lt;AppCastItem&gt; is the replacement list of
        ///  items that NetSparkle should use for the rest of the update process</returns>
        FilterResult GetFilteredAppCastItems(Version installed, List<AppCastItem> items);
    }
}
