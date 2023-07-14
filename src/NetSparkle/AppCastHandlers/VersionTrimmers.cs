using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <summary>
    /// Implementations of VersionTrimmer
    /// </summary>
    public static class VersionTrimmers
    {
        /// <summary>
        /// Remove pre-build and build specification
        /// </summary>
        /// <param name="semVerLike"></param>
        /// <returns></returns>
        public static Version DefaultVersionTrimmer(SemVerLike semVerLike)
        {
            return string.IsNullOrWhiteSpace(semVerLike.Version)
                ? new Version(0, 0, 0, 0)
                : new Version(semVerLike.Version);
        }
    }
}
