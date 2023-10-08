using System;
using System.Globalization;
using Microsoft.Win32;
using NetSparkleUpdater.AssemblyAccessors;
using NetSparkleUpdater.Interfaces;
#if (NETSTANDARD || NET31 || NET5 || NET6 || NET7 || NET8)
using System.Runtime.InteropServices;
#endif

#pragma warning disable CA1416

namespace NetSparkleUpdater.Configurations
{
    /// <summary>
    /// This class handles all registry values which are used from sparkle to handle 
    /// update intervalls. All values are stored in HKCU\Software\Vendor\AppName which 
    /// will be read ot from the assembly information. All values are of the REG_SZ 
    /// type, no matter what their "logical" type is.
    /// This should only be used on Windows!
    /// </summary>    
    public class RegistryConfiguration : Configuration
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private string _registryPath;

        /// <summary>
        /// Constructor for a configuration that saves and loads information from the Windows registry.
        /// This should only be used on Windows!
        /// </summary>
        /// <param name="assemblyAccessor">Object that accesses version, title, etc. info for the application
        /// you would like to check for updates for</param>
        public RegistryConfiguration(IAssemblyAccessor assemblyAccessor)
            : this(assemblyAccessor, "")
        { }

        /// <summary>
        /// Constructor for a configuration that saves and loads information from the Windows registry.
        /// This should only be used on Windows!
        /// </summary>
        /// <param name="assemblyAccessor">Object that accesses version, title, etc. info for the application
        /// you would like to check for updates for</param>
        /// <param name="registryPath">Location in the registry where configuration data should be stored and
        /// loaded from</param>
        public RegistryConfiguration(IAssemblyAccessor assemblyAccessor, string registryPath)
            : base(assemblyAccessor)
        {
            _registryPath = registryPath;
            try
            {
                // build the reg path
                string regPath = BuildRegistryPath();

                // load the values
                LoadValuesFromPath(regPath);
            }
            catch (NetSparkleException e)
            {
                // disable update checks when exception occurred -- can't read/save necessary update file 
                CheckForUpdate = false;
                throw new NetSparkleException("Can't read/save configuration data: " + e.Message);
            }
        }

        /// <inheritdoc/>
        public override void TouchProfileTime()
        {
            base.TouchProfileTime();
            // save the values
            SaveValuesToPath(BuildRegistryPath());
        }

        /// <inheritdoc/>
        public override void TouchCheckTime()
        {
            base.TouchCheckTime();
            // save the values
            SaveValuesToPath(BuildRegistryPath());
        }

        /// <inheritdoc/>
        public override void SetVersionToSkip(string version)
        {
            base.SetVersionToSkip(version);
            SaveValuesToPath(BuildRegistryPath());
        }

        /// <inheritdoc/>
        public override void Reload()
        {
            LoadValuesFromPath(BuildRegistryPath());
        }

        /// <summary>
        /// Generate the path in the registry where data will be saved to/loaded from.
        /// </summary>
        /// <exception cref="NetSparkleException">Thrown when the assembly accessor does not have the company or product name
        /// information available</exception>
        public virtual string BuildRegistryPath()
        {
            if (!string.IsNullOrEmpty(_registryPath))
            {
                return _registryPath;
            }
            else
            {
                if (string.IsNullOrEmpty(AssemblyAccessor.AssemblyCompany) || string.IsNullOrEmpty(AssemblyAccessor.AssemblyProduct))
                {
                    throw new NetSparkleException("Error: NetSparkleUpdater is missing the company or productname tag in the assembly accessor ("
                        + AssemblyAccessor.GetType() + ")");
                }

                _registryPath = "Software\\";
                if (!string.IsNullOrWhiteSpace(AssemblyAccessor.AssemblyCompany))
                {
                    _registryPath += AssemblyAccessor.AssemblyCompany + "\\";
                }
                if (!string.IsNullOrWhiteSpace(AssemblyAccessor.AssemblyProduct))
                {
                    _registryPath += AssemblyAccessor.AssemblyProduct + "\\";
                }
                _registryPath += "AutoUpdate";
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
        /// Load values from the provided registry path
        /// </summary>
        /// <param name="regPath">the registry path</param>
        /// <returns><c>true</c> if the items were loaded successfully; false otherwise</returns>
        private bool LoadValuesFromPath(string regPath)
        {
#if (NETSTANDARD || NET31 || NET5 || NET6 || NET7 || NET8)
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }
#endif
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
        /// Stores the configuration data into the registry at the given path
        /// </summary>
        /// <param name="regPath">the registry path</param>
        /// <returns><c>true</c> if the values were saved to the registry; false otherwise</returns>
        private bool SaveValuesToPath(string regPath)
        {
#if (NETSTANDARD || NET31 || NET5 || NET6 || NET7 || NET8)
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }
#endif
            RegistryKey key = Registry.CurrentUser.CreateSubKey(regPath);
            if (key == null)
            { 
                return false;
            }

            // convert to regsz
            string strCheckForUpdate = true.ToString(); // always check for updates next time!
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
