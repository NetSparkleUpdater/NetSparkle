#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using NetSparkleUpdater.AssemblyAccessors;
using NetSparkleUpdater.Interfaces;

namespace NetSparkleUpdater.Configurations
{
    /// <summary>
    /// Abstract class to handle update intervals and know which is the currently installed version
    /// of the software.
    /// </summary>
    public abstract class Configuration
    {
        /// <summary>
        /// The application name
        /// </summary>
        public string ApplicationName { get; protected set; }
        /// <summary>
        /// The previous version of the software that the user ran, e.g. "1.5.2"
        /// </summary>
        public string? PreviousVersionOfSoftwareRan { get; protected set; }
        /// <summary>
        /// The currently-installed version, e.g. "1.4.3"
        /// </summary>
        public string InstalledVersion { get; protected set; }
        /// <summary>
        /// Flag to indicate if we should check for updates
        /// </summary>
        public bool CheckForUpdate { get; protected set; }
        /// <summary>
        /// True if this is the first time the application has been run based on save config data; false otherwise
        /// </summary>
        public bool IsFirstRun { get; protected set; }
        /// <summary>
        /// <seealso cref="DateTime"/> of the last update check
        /// </summary>
        public DateTime LastCheckTime { get; protected set; }
        /// <summary>
        /// The version number of the update that was skipped last.
        /// If the user skipped an update, then the version to ignore is stored here (e.g. "1.4.3")
        /// </summary>
        public string? LastVersionSkipped { get; protected set; }
        /// <summary>
        /// Whether or not the application has run at least one time
        /// </summary>
        public bool DidRunOnce { get; protected set; }
        /// <summary>
        /// Last profile update
        /// </summary>
        public DateTime LastConfigUpdate { get; protected set; }
        /// <summary>
        /// Object that accesses version, title, etc. info for the currently running application
        /// (or some other application). NOTE: For the <seealso cref="JSONConfiguration"/> and 
        /// <seealso cref="RegistryConfiguration"/>, AssemblyCompany and AssemblyProduct must
        /// return valid, non-whitespace results if you do not set the save/registry path yourself!
        /// </summary>
        public IAssemblyAccessor AssemblyAccessor { get; set; }
        
        /// <summary>
        /// Constructor for Configuration -- should load all pertinent values by the end of the constructor!
        /// If any exception is thrown during construction of this object (e.g. from the assembly accessors),
        /// then <see cref="CheckForUpdate"/> is set to false. NOTE: For the 
        /// <seealso cref="JSONConfiguration"/> and <seealso cref="RegistryConfiguration"/>, 
        /// AssemblyCompany and AssemblyProduct must return valid, non-whitespace results if you do not set 
        /// the save/registry path yourself!
        /// </summary>
        /// <param name="assemblyAccessor">Object that accesses version, title, etc. info for the application
        /// you would like to check for updates for</param>
        public Configuration(IAssemblyAccessor assemblyAccessor)
        {
            // set default values
            InitWithDefaultValues();
            AssemblyAccessor = assemblyAccessor;
            ApplicationName = assemblyAccessor.AssemblyProduct;
            InstalledVersion = assemblyAccessor.AssemblyVersion;
        }

        /// <summary>
        /// Set the last configuration update time to <see cref="DateTime.Now"/>.
        /// </summary>
        public virtual void TouchProfileTime()
        {
            LastConfigUpdate = DateTime.Now;
        }

        /// <summary>
        /// Set the last time we checked for updates to <see cref="DateTime.Now"/>.
        /// Should be used after an update check has been made.
        /// </summary>
        public virtual void TouchCheckTime()
        {
            LastCheckTime = DateTime.Now;
        }

        /// <summary>
        /// Set the given version as the version that should be skipped (ignored)
        /// when looking at available updates
        /// </summary>
        /// <param name="version">the version to skip. e.g. "1.2.3"</param>
        public virtual void SetVersionToSkip(string version)
        {
            LastVersionSkipped = version;
        }

        /// <summary>
        /// Reloads the configuration object from disk/memory/etc.
        /// </summary>
        public abstract void Reload();

        /// <summary>
        /// Set the configuration values to their default values (should check for update,
        /// last check time was in the past, no version skipped, and have never run the
        /// software).
        /// </summary>
        protected void InitWithDefaultValues()
        {
            CheckForUpdate = true;
            LastCheckTime = new DateTime(0);
            LastVersionSkipped = string.Empty;
            DidRunOnce = false;
        }
    }
}
