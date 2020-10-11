using NetSparkleUpdater.Configurations;
using NetSparkleUpdater.Enums;
using System;
using System.Collections.Generic;

namespace NetSparkleUpdater.Events
{
    /// <summary>
    /// Contains all information for the update detected event
    /// </summary>
    public class UpdateDetectedEventArgs : EventArgs
    {
        /// <summary>
        /// The next action to execute after the app user decides how to
        /// handle an update
        /// </summary>
        public NextUpdateAction NextAction { get; set; }
        /// <summary>
        /// The application configuration (stores data on last time updates were
        /// checked, etc.)
        /// </summary>
        public Configuration ApplicationConfig { get; set; }
        /// <summary>
        /// The latest available version in the app cast
        /// </summary>
        public AppCastItem LatestVersion { get; set; }

        /// <summary>
        /// All app cast items that were sent in the appcast
        /// </summary>
        public List<AppCastItem> AppCastItems { get; set; }
    }
}
