using NetSparkle.Enums;
using System;
using System.Collections.Generic;

namespace NetSparkle.Events
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
        public Configuration ApplicationConfig { get; set; }
        /// <summary>
        /// The latest available version
        /// </summary>
        public AppCastItem LatestVersion { get; set; }

        /// <summary>
        /// All app cast items that were sent in the appcast
        /// </summary>
        public List<AppCastItem> AppCastItems { get; set; }
    }
}
