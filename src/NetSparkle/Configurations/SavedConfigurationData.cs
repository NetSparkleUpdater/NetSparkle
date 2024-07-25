﻿using System;

namespace NetSparkleUpdater.Configurations
{
    /// <summary>
    /// Configuration data for this software and NetSparkle instance.
    /// Allows you to get information on the versions that the user
    /// skipped, when the last update was performed, etc.
    /// Used by the <see cref="JSONConfiguration"/> class to save/load
    /// data easily to/from disk.
    /// </summary>
    public class SavedConfigurationData
    {
        /// <summary>
        /// Whether or not to check for an update
        /// </summary>
        public bool CheckForUpdate { get; set; }
        /// <summary>
        /// The last <see cref="DateTime"/> that an update check was performed
        /// </summary>
        public DateTime LastCheckTime { get; set; }
        /// <summary>
        /// The previous version of the software that the user ran
        /// </summary>
        public string? PreviousVersionOfSoftwareRan { get; set; }
        /// <summary>
        /// The last version (as a string) that the user chose
        /// to skip.
        /// Can be blank.
        /// </summary>
        public string? LastVersionSkipped { get; set; }
        /// <summary>
        /// Whether or not the software has run at least one time.
        /// </summary>
        public bool DidRunOnce { get; set; }
        /// <summary>
        /// Last <see cref="DateTime"/> that the configuration data was updated.
        /// </summary>
        public DateTime LastConfigUpdate { get; set; }

        /// <summary>
        /// Simple data holder constructor for <seealso cref="Configuration"/> data that is loaded/saved
        /// from various places for info on the app, what version the user has skipped, etc.
        /// </summary>
        public SavedConfigurationData()
        {
        }
    }
}
