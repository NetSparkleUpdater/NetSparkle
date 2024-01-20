using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <summary>
    /// Implementations of AppCastReducers
    /// </summary>
    public static class AppCastReducers
    {
        /// <summary>
        /// Choose only retail versions
        /// </summary>
        /// <param name="installed"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IEnumerable<AppCastItem> OnlyRetailVersions(SemVerLike installed, IEnumerable<AppCastItem> items)
        {
            return items.Where(it => SemVerLike.Parse(it.Version).AllSuffixes.Length == 0);
        }

        /// <summary>
        /// Choose only pre-release versions
        /// </summary>
        /// <param name="installed"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IEnumerable<AppCastItem> OnlyPreReleasedVersions(SemVerLike installed, IEnumerable<AppCastItem> items)
        {
            return items.Where(it => SemVerLike.Parse(it.Version).AllSuffixes.Length != 0);
        }

        /// <summary>
        /// Move newest version at top of the collecion
        /// </summary>
        /// <param name="installed"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IEnumerable<AppCastItem> NewerFirst(SemVerLike installed, IEnumerable<AppCastItem> items)
        {
            return items.OrderByDescending(it => SemVerLike.Parse(it.Version));
        }

        /// <summary>
        /// Remove older versions
        /// </summary>
        /// <param name="installed"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IEnumerable<AppCastItem> RemoveOlderVersions(SemVerLike installed, IEnumerable<AppCastItem> items)
        {
            return items.Where(it => installed.CompareTo(SemVerLike.Parse(it.Version)) < 0);
        }

        /// <summary>
        /// Compose one or more AppCastReducers into one
        /// </summary>
        /// <param name="allAnd"></param>
        /// <returns></returns>
        public static AppCastReducerDelegate Mix(params AppCastReducerDelegate[] allAnd)
        {
            return (installed, versions) => allAnd.Aggregate(
                versions,
                (filteredVersions, reducer) => reducer(installed, filteredVersions)
            );
        }
    }
}
