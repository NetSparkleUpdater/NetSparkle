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

        /// <summary>
        /// Basic constructor for UpdateDetectedEventArgs
        /// </summary>
        /// <param name="action">Next update action for this update (e.g. show UI, perform unattended)</param>
        /// <param name="config"><seealso cref="Configuration"/> for the current application</param>
        /// <param name="item"><seealso cref="AppCastItem"/> that represents an update for the user</param>
        /// <param name="items">All <seealso cref="AppCastItem"/> objects available.</param>
        public UpdateDetectedEventArgs(NextUpdateAction action, Configuration config, AppCastItem item, List<AppCastItem> items)
        {
            NextAction = action;
            ApplicationConfig = config;
            LatestVersion = item;
            AppCastItems = items;
        }
    }
}
