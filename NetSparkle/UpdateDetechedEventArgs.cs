using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppLimit.NetSparkle
{
    /// <summary>
    /// Contains all information for the update detected event
    /// </summary>
    public class UpdateDetectedEventArgs : EventArgs
    {
        /// <summary>
        /// The next action
        /// </summary>
        public NextUpdateAction NextAction { get; set; }
        /// <summary>
        /// The application configuration
        /// </summary>
        public NetSparkleConfiguration ApplicationConfig { get; set; }
        /// <summary>
        /// The latest available version
        /// </summary>
        public NetSparkleAppCastItem LatestVersion { get; set; }
    }
}
