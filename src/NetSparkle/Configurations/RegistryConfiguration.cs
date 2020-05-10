using System;
using System.Globalization;
using Microsoft.Win32;
using NetSparkleUpdater.AssemblyAccessors;

namespace NetSparkleUpdater.Configurations
{
    /// <summary>
    /// This class handles all registry values which are used from sparkle to handle 
    /// update intervalls. All values are stored in HKCU\Software\Vendor\AppName which 
    /// will be read ot from the assembly information. All values are of the REG_SZ 
    /// type, no matter what their "logical" type is. The following options are
    /// available:
    /// 
    /// CheckForUpdate  - Boolean    - Whether NetSparkle should check for updates
    /// LastCheckTime   - time_t     - Time of last check
    /// SkipThisVersion - String     - If the user skipped an update, then the version to ignore is stored here (e.g. "1.4.3")
    /// DidRunOnce      - Boolean    - Check only one time when the app launched
    /// </summary>    
    public class RegistryConfiguration : Configuration
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private string _registryPath;

        /// <summary>
        /// The constructor reads out all configured values
        /// </summary>        
        /// <param name="referenceAssembly">the reference assembly name</param>
        public RegistryConfiguration(string referenceAssembly)
            : this(referenceAssembly, true)
        { }

        /// <inheritdoc/>
        public RegistryConfiguration(string referenceAssembly, bool isReflectionBasedAssemblyAccessorUsed)
            : this(referenceAssembly, isReflectionBasedAssemblyAccessorUsed, string.Empty)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="referenceAssembly">the name of hte reference assembly</param>
        /// <param name="isReflectionBasedAssemblyAccessorUsed"><c>true</c> if reflection is used to access the assembly.</param>
        /// <param name="registryPath"><c>true</c> if reflection is used to access the assembly.</param>
        public RegistryConfiguration(string referenceAssembly, bool isReflectionBasedAssemblyAccessorUsed, string registryPath)
            : base(referenceAssembly, isReflectionBasedAssemblyAccessorUsed)
        {
            _registryPath = registryPath;
            try
            {
                // build the reg path
                string regPath = BuildRegistryPath();

                // load the values
                LoadValuesFromPath(regPath);
            }
            catch (NetSparkleException)
            {
                // disable update checks when exception was called 
                CheckForUpdate = false;
                throw;
            }
        }

        /// <summary>
        /// Touches to profile time
        /// </summary>
        public override void TouchProfileTime()
        {
            base.TouchProfileTime();
            // save the values
            SaveValuesToPath(BuildRegistryPath());
        }

        /// <summary>
        /// Touches the check time to now, should be used after a check directly
        /// </summary>
        public override void TouchCheckTime()
        {
            base.TouchCheckTime();
            // save the values
            SaveValuesToPath(BuildRegistryPath());
        }

        /// <summary>
        /// This method allows to skip a specific version
        /// </summary>
        /// <param name="version">the version to skeip</param>
        public override void SetVersionToSkip(string version)
        {
            base.SetVersionToSkip(version);
            SaveValuesToPath(BuildRegistryPath());
        }

        /// <summary>
        /// Reloads the configuration object
        /// </summary>
        public override void Reload()
        {
            LoadValuesFromPath(BuildRegistryPath());
        }

        /// <summary>
        /// This function build a valid registry path in dependecy to the 
        /// assembly information
        /// </summary>
        public virtual string BuildRegistryPath()
        {
            if (!string.IsNullOrEmpty(_registryPath))
            {
                return _registryPath;
            }
            else
            {
                AssemblyAccessor accessor = new AssemblyAccessor(ReferenceAssembly, UseReflectionBasedAssemblyAccessor);

                if (string.IsNullOrEmpty(accessor.AssemblyCompany) || string.IsNullOrEmpty(accessor.AssemblyProduct))
                {
                    throw new NetSparkleException("Error: NetSparkle is missing the company or productname tag in " + ReferenceAssembly);
                }

                _registryPath = "Software\\" + accessor.AssemblyCompany + "\\" + accessor.AssemblyProduct + "\\AutoUpdate";
                return _registryPath;
            }
        }

        private string ConvertDateToString(DateTime dt)
        {
            return dt.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
        }

        private DateTime ConvertStringToDate(string str)
        {
            return DateTime.ParseExact(str, DateTimeFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// This method loads the values from registry
        /// </summary>
        /// <param name="regPath">the registry path</param>
        /// <returns><c>true</c> if the items were loaded</returns>
        private bool LoadValuesFromPath(string regPath)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(regPath);
            if (key == null)
            {
                SaveDidRunOnceAsTrue(regPath);
                return false;
            }
            else
            {
                // read out                
                string strCheckForUpdate = key.GetValue("CheckForUpdate", "True") as string;
                string strLastCheckTime = key.GetValue("LastCheckTime", ConvertDateToString(new DateTime(0))) as string;
                string strSkipThisVersion = key.GetValue("SkipThisVersion", "") as string;
                string strDidRunOnc = key.GetValue("DidRunOnce", "False") as string;
                string strProfileTime = key.GetValue("LastProfileUpdate", ConvertDateToString(new DateTime(0))) as string;
                string strPreviousVersion = key.GetValue("PreviousVersionRun", "") as string;

                // convert the right datatypes
                CheckForUpdate = Convert.ToBoolean(strCheckForUpdate);
                try
                {
                    LastCheckTime = ConvertStringToDate(strLastCheckTime);
                }
                catch (FormatException)
                {
                    LastCheckTime = new DateTime(0);
                }

                LastVersionSkipped = strSkipThisVersion;
                DidRunOnce = Convert.ToBoolean(strDidRunOnc);
                IsFirstRun = !DidRunOnce;
                PreviousVersionOfSoftwareRan = strPreviousVersion;
                if (IsFirstRun)
                {
                    SaveDidRunOnceAsTrue(regPath);
                }
                else
                {
                    SaveValuesToPath(regPath); // so PreviousVersionRun is saved
                }
                try
                {
                    LastConfigUpdate = ConvertStringToDate(strProfileTime);
                }
                catch (FormatException)
                {
                    LastConfigUpdate = new DateTime(0);
                }
            }
            return true;
        }

        private void SaveDidRunOnceAsTrue(string regPath)
        {
            var initialValue = DidRunOnce;
            DidRunOnce = true;
            SaveValuesToPath(regPath); // save it so next time we load DidRunOnce is true
            DidRunOnce = initialValue; // so data is correct to user of Configuration class
        }

        /// <summary>
        /// This method store the information into registry
        /// </summary>
        /// <param name="regPath">the registry path</param>
        /// <returns><c>true</c> if the values were saved to the registry</returns>
        private bool SaveValuesToPath(string regPath)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(regPath);
            if (key == null)
            { 
                return false;
            }

            // convert to regsz
            string strCheckForUpdate = CheckForUpdate.ToString();
            string strLastCheckTime = ConvertDateToString(LastCheckTime);
            string strSkipThisVersion = LastVersionSkipped;
            string strDidRunOnc = DidRunOnce.ToString();
            string strProfileTime = ConvertDateToString(LastConfigUpdate);
            string strPreviousVersion = InstalledVersion;

            // set the values
            key.SetValue("CheckForUpdate", strCheckForUpdate, RegistryValueKind.String);
            key.SetValue("LastCheckTime", strLastCheckTime, RegistryValueKind.String);
            key.SetValue("SkipThisVersion", strSkipThisVersion, RegistryValueKind.String);
            key.SetValue("DidRunOnce", strDidRunOnc, RegistryValueKind.String);
            key.SetValue("LastProfileUpdate", strProfileTime, RegistryValueKind.String);
            key.SetValue("PreviousVersionRun", strPreviousVersion, RegistryValueKind.String);

            return true;
        }
    }
}
