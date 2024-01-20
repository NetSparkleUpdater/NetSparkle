using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <summary>
    /// Filter AppCast with SemVerLike version specification.
    /// </summary>
    /// <remarks>
    /// Implementor has responsibility to do:
    /// - Order the version.
    /// - Filter the version.
    /// 
    /// Consider these use cases:
    /// - If there is no interesting in older versions, exclude them from collection.
    /// - If there is no interesting version to be installed, return empty collection.
    /// - If there is an intension to install a Beta version, return it at first of the collection.
    /// - If there is an intension to reject any Beta versions, return non Beta versions as a collection.
    /// </remarks>
    /// <param name="installed">Installed version of app</param>
    /// <param name="items">AppCast candidate updates</param>
    /// <returns>An enumerable of app casts NetSparkle should use</returns>
    public delegate IEnumerable<AppCastItem> AppCastReducerDelegate(SemVerLike installed, IEnumerable<AppCastItem> items);
}
