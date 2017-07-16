using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkle.Enums
{
    /// <summary>
    /// Possibilities for the status of an update request
    /// </summary>
    public enum UpdateStatus
    {
        /// <summary>
        /// An update is available
        /// </summary>
        UpdateAvailable,
        /// <summary>
        /// No updates are available
        /// </summary>
        UpdateNotAvailable,
        /// <summary>
        /// An update is available, but the user has chosen to skip this version
        /// </summary>
        UserSkipped,
        /// <summary>
        /// There was a problem fetching the appcast
        /// </summary>
        CouldNotDetermine
    }
}
