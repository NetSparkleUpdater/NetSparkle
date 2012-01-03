using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;

namespace AppLimit.NetSparkle
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
    public class NetSparkleConfiguration
    {
        /// <summary>
        /// The application name
        /// </summary>
        public string   ApplicationName     { get; private set; }
        /// <summary>
        /// The currently-installed version
        /// </summary>
        public string   InstalledVersion    { get; private set; }
        /// <summary>
        /// Flag to indicate if we should check for updates
        /// </summary>
        public bool     CheckForUpdate      { get; private set; }
        /// <summary>
        /// Last check time
        /// </summary>
        public DateTime LastCheckTime       { get; private set; }
        /// <summary>
        /// The last-skipped version number
        /// </summary>
        public string   SkipThisVersion     { get; private set; }
        /// <summary>
        /// The application ran once
        /// </summary>
        public bool     DidRunOnce           { get; private set; }
        /// <summary>
        /// Flag to indicate showing the diagnostic window
        /// </summary>
        public bool     ShowDiagnosticWindow { get; private set; }
        /// <summary>
        /// Last profile update
        /// </summary>
        public DateTime LastProfileUpdate   { get; private set; }

        /// <summary>
        /// If this property is true a reflection based accessor will be used
        /// to determine the assmebly name and verison, otherwise a System.Diagnostics
        /// based access will be used
        /// </summary>
        public Boolean UseReflectionBasedAssemblyAccessor { get; private set; }

        private String _referenceAssembly;

        /// <summary>
        /// The constructor reads out all configured values
        /// </summary>        
        /// <param name="referenceAssembly">the reference assembly name</param>
        public NetSparkleConfiguration(String referenceAssembly)
            : this(referenceAssembly, true)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="referenceAssembly">the name of hte reference assembly</param>
        /// <param name="isReflectionBasedAssemblyAccessorUsed"><c>true</c> if reflection is used to access the assembly.</param>
        public NetSparkleConfiguration(string referenceAssembly, bool isReflectionBasedAssemblyAccessorUsed)
        {
            // set the value
            this.UseReflectionBasedAssemblyAccessor = isReflectionBasedAssemblyAccessorUsed;

            // save the referecne assembly
            _referenceAssembly = referenceAssembly;

            try
            {
                // set default values
                InitWithDefaultValues();

                // set some value from the binary
                NetSparkleAssemblyAccessor accessor = new NetSparkleAssemblyAccessor(referenceAssembly, this.UseReflectionBasedAssemblyAccessor);
                ApplicationName     = accessor.AssemblyProduct;
                InstalledVersion    = accessor.AssemblyVersion;

                // build the reg path
                String regPath = BuildRegistryPath();

                // load the values
                LoadValuesFromPath(regPath);                                
            }
            catch (Exception e)
            {
                // disable update checks when exception was called 
                CheckForUpdate = false;

                if (e.Message.Contains("STOP:"))
                    throw e;
            }
        }

        /// <summary>
        /// Touches to profile time
        /// </summary>
        public void TouchProfileTime()
        {
            // set the prodilt update time
            LastProfileUpdate = DateTime.Now;

            // build path
            String path = BuildRegistryPath();

            // save the values
            SaveValuesToPath(path);
        }

        /// <summary>
        /// Touches the check time to now, should be used after a check directly
        /// </summary>
        public void TouchCheckTime()
        {
            // set the check tiem
            LastCheckTime = DateTime.Now;

            // build path
            String path = BuildRegistryPath();

            // save the values
            SaveValuesToPath(path);
        }

        /// <summary>
        /// This method allows to skip a specific version
        /// </summary>
        /// <param name="version"></param>
        public void SetVersionToSkip(String version)
        {
            // set the check tiem
            SkipThisVersion = version;

            // build path
            String path = BuildRegistryPath();

            // save the values
            SaveValuesToPath(path);
        }

        /// <summary>
        /// This function build a valid registry path in dependecy to the 
        /// assembly information
        /// </summary>
        /// <returns></returns>
        private String BuildRegistryPath()
        {
            NetSparkleAssemblyAccessor accessor = new NetSparkleAssemblyAccessor(_referenceAssembly, UseReflectionBasedAssemblyAccessor);

            if (accessor.AssemblyCompany == null || accessor.AssemblyCompany.Length == 0 ||
                    accessor.AssemblyProduct == null || accessor.AssemblyProduct.Length == 0)
                throw new Exception("STOP: Sparkle is missing the company or productname tag in " + _referenceAssembly);
            
            return "Software\\" + accessor.AssemblyCompany + "\\" + accessor.AssemblyProduct + "\\AutoUpdate";
        }

        /// <summary>
        /// This method set's default values for the config
        /// </summary>
        private void InitWithDefaultValues()
        {
            CheckForUpdate = true;
            LastCheckTime = new DateTime(0);
            SkipThisVersion = String.Empty;
            DidRunOnce = false;
            UseReflectionBasedAssemblyAccessor = true;
        }

        /// <summary>
        /// This method loads the values from registry
        /// </summary>
        /// <param name="regPath"></param>
        /// <returns></returns>
        private Boolean LoadValuesFromPath(String regPath)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(regPath);
            if (key == null)
                return false;
            else
            {                
                // read out                
                String strCheckForUpdate = key.GetValue("CheckForUpdate", "True") as String;
                String strLastCheckTime = key.GetValue("LastCheckTime", new DateTime(0).ToString()) as String;
                String strSkipThisVersion = key.GetValue("SkipThisVersion", "") as String;                
                String strDidRunOnc = key.GetValue("DidRunOnce", "False") as String;
                String strShowDiagnosticWindow = key.GetValue("ShowDiagnosticWindow", "False") as String;
                String strProfileTime = key.GetValue("LastProfileUpdate", new DateTime(0).ToString()) as String;                

                // convert th right datatypes
                CheckForUpdate = Convert.ToBoolean(strCheckForUpdate);
                LastCheckTime = Convert.ToDateTime(strLastCheckTime);
                SkipThisVersion = strSkipThisVersion;
                DidRunOnce = Convert.ToBoolean(strDidRunOnc);
                ShowDiagnosticWindow = Convert.ToBoolean(strShowDiagnosticWindow);
                LastProfileUpdate = Convert.ToDateTime(strProfileTime);                
                return true;
            }
        }

        /// <summary>
        /// This method store the information into registry
        /// </summary>
        /// <param name="regPath"></param>
        /// <returns></returns>
        private Boolean SaveValuesToPath(String regPath)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(regPath);
            if (key == null)
                return false;
            else
            {
                // convert to regsz
                String strCheckForUpdate    = CheckForUpdate.ToString();
                String strLastCheckTime     = LastCheckTime.ToString();
                String strSkipThisVersion   = SkipThisVersion.ToString();
                String strDidRunOnc         = DidRunOnce.ToString();
                String strProfileTime       = LastProfileUpdate.ToString();                

                // set the values
                key.SetValue("CheckForUpdate", strCheckForUpdate, RegistryValueKind.String);
                key.SetValue("LastCheckTime", strLastCheckTime, RegistryValueKind.String);
                key.SetValue("SkipThisVersion", strSkipThisVersion, RegistryValueKind.String);
                key.SetValue("DidRunOnce", strDidRunOnc, RegistryValueKind.String);
                key.SetValue("LastProfileUpdate", strProfileTime, RegistryValueKind.String);                

                return true;
            }
        }

    }
}
