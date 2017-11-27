using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkle.Enums
{
    /// <summary>
    /// Possible Result values for IUpdateAvailable implementation.
    /// </summary>
    public enum UpdateAvailableResult
    {
        /// <summary>
        /// No result specified. Default value.
        /// </summary>
        None = 0,

        /// <summary>
        /// User chose to install the update immediatelly.
        /// </summary>
        InstallUpdate,

        /// <summary>
        /// Used chose to skip the update.
        /// </summary>
        SkipUpdate,

        /// <summary>
        /// User chose to remind her later.
        /// </summary>
        RemindMeLater
    }
}
