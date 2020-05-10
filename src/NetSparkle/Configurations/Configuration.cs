using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using NetSparkleUpdater.AssemblyAccessors;

namespace NetSparkleUpdater.Configurations
{
    /// <summary>
    /// Abstract class to handle 
    /// update intervals.
    /// 
    /// CheckForUpdate  - Boolean    - Whether NetSparkle should check for updates
    /// LastCheckTime   - time_t     - Time of last check
    /// SkipThisVersion - String     - If the user skipped an update, then the version to ignore is stored here (e.g. "1.4.3")
    /// DidRunOnce      - Boolean    - Check only one time when the app launched
    /// </summary>    
    public abstract class Configuration
    {
        /// <summary>
        /// The application name
        /// </summary>
        public string ApplicationName { get; protected set; }
        /// <summary>
        /// The previous version of the software that the user ran
        /// </summary>
        public string PreviousVersionOfSoftwareRan { get; protected set; }
        /// <summary>
        /// The currently-installed version
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
        /// Last check time
        /// </summary>
        public DateTime LastCheckTime { get; protected set; }
        /// <summary>
        /// The last-skipped version number
        /// </summary>
        public string LastVersionSkipped { get; protected set; }
        /// <summary>
        /// Whether or not the application has run at least one time
        /// </summary>
        public bool DidRunOnce { get; protected set; }
        /// <summary>
        /// Last profile update
        /// </summary>
        public DateTime LastConfigUpdate { get; protected set; }

        /// <summary>
        /// If this property is true a reflection based accessor will be used
        /// to determine the assembly name and version, otherwise a System.Diagnostics
        /// based access will be used
        /// </summary>
        public bool UseReflectionBasedAssemblyAccessor { get; protected set; }

        /// <summary>
        /// The reference assembly name
        /// </summary>
        protected string ReferenceAssembly { get; set; }

        /// <summary>
        /// The constructor reads out all configured values
        /// </summary>        
        /// <param name="referenceAssembly">the reference assembly name</param>
        protected Configuration(string referenceAssembly)
            : this(referenceAssembly, true)
        { }

        /// <summary>
        /// Constructor for Configuration -- should load values by the end of the constructor!
        /// </summary>
        /// <param name="referenceAssembly">the name of the reference assembly</param>
        /// <param name="isReflectionBasedAssemblyAccessorUsed"><c>true</c> if reflection is used to access the assembly.</param>
        protected Configuration(string referenceAssembly, bool isReflectionBasedAssemblyAccessorUsed)
        {
            // set default values
            InitWithDefaultValues();

            // set the value
            UseReflectionBasedAssemblyAccessor = isReflectionBasedAssemblyAccessorUsed;
            // save the reference assembly
            ReferenceAssembly = referenceAssembly;

            try
            {
                // set some value from the binary
                AssemblyAccessor accessor = new AssemblyAccessor(referenceAssembly, UseReflectionBasedAssemblyAccessor);
                ApplicationName = accessor.AssemblyProduct;
                InstalledVersion = accessor.AssemblyVersion;
            }
            catch
            {
                CheckForUpdate = false;
            }

        }

        /// <summary>
        /// Touches to profile time
        /// </summary>
        public virtual void TouchProfileTime()
        {
            LastConfigUpdate = DateTime.Now;
        }

        /// <summary>
        /// Touches the check time to now, should be used after a check directly
        /// </summary>
        public virtual void TouchCheckTime()
        {
            LastCheckTime = DateTime.Now;
        }

        /// <summary>
        /// This method allows to skip a specific version
        /// </summary>
        /// <param name="version">the version to skeip</param>
        public virtual void SetVersionToSkip(String version)
        {
            LastVersionSkipped = version;
        }

        /// <summary>
        /// Reloads the configuration object
        /// </summary>
        public abstract void Reload();

        /// <summary>
        /// This method sets default values for the config
        /// </summary>
        protected void InitWithDefaultValues()
        {
            CheckForUpdate = true;
            LastCheckTime = new DateTime(0);
            LastVersionSkipped = string.Empty;
            DidRunOnce = false;
            UseReflectionBasedAssemblyAccessor = true;
        }
    }
}
