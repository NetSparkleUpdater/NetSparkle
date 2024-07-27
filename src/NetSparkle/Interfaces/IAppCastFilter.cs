using System.Collections.Generic;
using NetSparkleUpdater.AppCastHandlers;

namespace NetSparkleUpdater.Interfaces
{
    /// <summary>
    /// Provides a way to filter out AppCast item instances that do not apply to the current update process.  
    /// Can be used to make it possible to revert from a "beta" version to a "stable" one - where the current runtime
    /// System.Version value of the <seealso cref="Configurations">Configurations</seealso> is by definition higher
    /// than anything in the "stable" app cast list.
    /// </summary>
    public interface IAppCastFilter
    {
        /// <summary>
        /// Filter AppCast with SemVerLike version specification.
        /// </summary>
        /// <remarks>
        /// Implementor has responsibility to both order the versions in the app cast 
        /// (put the ones you want in order starting at index 0) and filter out items you don't want at all.
        /// 
        /// Consider these use cases:
        /// - If there is no interest in older versions, exclude them from collection.
        /// - If there is no interest in a version to be installed, return empty collection.
        /// - If there is an intention to install a Beta version, return it as first of the collection.
        /// - If there is an intention to reject any Beta versions, return non Beta versions as a collection.
        /// </remarks>
        /// <param name="installed">Installed version of app</param>
        /// <param name="items">AppCastItem candidate updates</param>
        /// <returns>An enumerable of app casts NetSparkle should use</returns>
        IEnumerable<AppCastItem> GetFilteredAppCastItems(SemVerLike installed, IEnumerable<AppCastItem> items);
    }
}
