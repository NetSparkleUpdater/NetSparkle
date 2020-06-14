using NetSparkleUpdater.AssemblyAccessors;
using NetSparkleUpdater.Interfaces;
#if NETFRAMEWORK
using Newtonsoft.Json;
#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
#if NETSTANDARD
using System.Text.Json;
#endif

namespace NetSparkleUpdater.Configurations
{
    class JSONConfiguration : Configuration
    {
        private string _savePath;

        /// <summary>
        /// Constructor for a configuration that saves and loads its configuration data to and
        /// from a JSON file that resides on disk. This Configuration can be used on any
        /// operating system.
        /// </summary>
        /// <param name="assemblyAccessor">Object that accesses version, title, etc. info for the application
        /// you would like to check for updates for</param>
        public JSONConfiguration(IAssemblyAccessor assemblyAccessor)
            : this(assemblyAccessor, string.Empty)
        { }

        /// <summary>
        /// Constructor for a configuration that saves and loads its configuration data to and
        /// from a JSON file that resides on disk. This Configuration can be used on any
        /// operating system.
        /// </summary>
        /// <param name="assemblyAccessor">Object that accesses version, title, etc. info for the application
        /// you would like to check for updates for</param>
        /// <param name="savePath">location to save the JSON configuration data to; can be null or empty string.
        /// If not null or empty string, must represent a valid path on disk (directories must already be created).
        /// This class will take care of creating/overwriting the file at that path if necessary.</param>
        public JSONConfiguration(IAssemblyAccessor assemblyAccessor, string savePath)
            : base(assemblyAccessor)
        {
            _savePath = savePath != null && string.IsNullOrWhiteSpace(savePath) ? savePath : GetSavePath();
            try
            {
                // get the save path
                _savePath = GetSavePath();
                // load the values
                LoadValuesFromPath(_savePath);
            }
            catch (NetSparkleException e)
            {
                // disable update checks when exception occurred -- can't read/save necessary update file 
                CheckForUpdate = false;
                throw new NetSparkleException("Can't read/save configuration data: " + e.Message);
            }
        }

        /// <summary>
        /// Touches to profile time
        /// </summary>
        public override void TouchProfileTime()
        {
            base.TouchProfileTime();
            // save the values
            SaveValuesToPath(GetSavePath());
        }

        /// <summary>
        /// Touches the check time to now, should be used after a check directly
        /// </summary>
        public override void TouchCheckTime()
        {
            base.TouchCheckTime();
            // save the values
            SaveValuesToPath(GetSavePath());
        }

        /// <summary>
        /// This method allows to skip a specific version
        /// </summary>
        /// <param name="version">the version to skeip</param>
        public override void SetVersionToSkip(string version)
        {
            base.SetVersionToSkip(version);
            SaveValuesToPath(GetSavePath());
        }

        /// <summary>
        /// Reloads the configuration object
        /// </summary>
        public override void Reload()
        {
            LoadValuesFromPath(GetSavePath());
        }

        /// <summary>
        /// This function build a valid registry path in dependecy to the 
        /// assembly information
        /// </summary>
        public virtual string GetSavePath()
        {
            if (!string.IsNullOrEmpty(_savePath))
            {
                return _savePath;
            }
            else
            {

                if (string.IsNullOrEmpty(AssemblyAccessor.AssemblyCompany) || string.IsNullOrEmpty(AssemblyAccessor.AssemblyProduct))
                {
                    throw new NetSparkleException("Error: NetSparkleUpdater is missing the company or productname tag in the assembly accessor ("
                        + AssemblyAccessor.GetType() + ")");
                }
                var applicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify);
                var saveFolder = Path.Combine(applicationFolder, AssemblyAccessor.AssemblyCompany, AssemblyAccessor.AssemblyProduct, "NetSparkleUpdater");
                if (!Directory.Exists(saveFolder))
                {
                    Directory.CreateDirectory(saveFolder);
                }
                var saveLocation = Path.Combine(saveFolder, "data.json");
                return saveLocation;
            }
        }

        /// <summary>
        /// This method loads the values from registry
        /// </summary>
        /// <param name="saveLocation">the saved file location</param>
        /// <returns><c>true</c> if the items were loaded</returns>
        private bool LoadValuesFromPath(string saveLocation)
        {
            if (File.Exists(saveLocation))
            {
                try
                {
                    string json = File.ReadAllText(saveLocation);
#if NETSTANDARD
                    var data = JsonSerializer.Deserialize<SavedConfigurationData>(json);
#else
                    var data = JsonConvert.DeserializeObject<SavedConfigurationData>(json);
#endif
                    CheckForUpdate = true;
                    LastCheckTime = data.LastCheckTime;
                    LastVersionSkipped = data.LastVersionSkipped;
                    DidRunOnce = data.DidRunOnce;
                    IsFirstRun = !DidRunOnce;
                    LastConfigUpdate = data.LastConfigUpdate;
                    PreviousVersionOfSoftwareRan = data?.PreviousVersionOfSoftwareRan ?? "";
                    if (IsFirstRun)
                    {
                        SaveDidRunOnceAsTrue(saveLocation);
                    }
                    else
                    {
                        SaveValuesToPath(saveLocation); // so PreviousVersion is set to proper value
                    }
                    return true;
                }
                catch (Exception) // just in case...
                {
                }
            }
            CheckForUpdate = true;
            LastCheckTime = DateTime.Now;
            LastVersionSkipped = string.Empty;
            DidRunOnce = false;
            IsFirstRun = true;
            SaveDidRunOnceAsTrue(saveLocation);
            LastConfigUpdate = DateTime.Now;
            PreviousVersionOfSoftwareRan = "";
            return true;
        }

        private void SaveDidRunOnceAsTrue(string saveLocation)
        {
            var initialValue = DidRunOnce;
            DidRunOnce = true;
            SaveValuesToPath(saveLocation); // save it so next time we load DidRunOnce is true
            DidRunOnce = initialValue; // so data is correct to user of Configuration class
        }

        /// <summary>
        /// This method stores the information to disk as json
        /// </summary>
        /// <param name="savePath">the save path to the json file</param>
        /// <returns><c>true</c> if the values were saved to disk</returns>
        private bool SaveValuesToPath(string savePath)
        {
            var savedConfig = new SavedConfigurationData()
            {
                CheckForUpdate = true,
                LastCheckTime = this.LastCheckTime,
                LastVersionSkipped = this.LastVersionSkipped,
                DidRunOnce = this.DidRunOnce,
                LastConfigUpdate = DateTime.Now,
                PreviousVersionOfSoftwareRan = InstalledVersion
            };
            LastConfigUpdate = savedConfig.LastConfigUpdate;

#if NETSTANDARD
            string json = JsonSerializer.Serialize(savedConfig);
#else
            string json = JsonConvert.SerializeObject(savedConfig);
#endif
            try
            {
                File.WriteAllText(savePath, json);
            } 
            catch (Exception) // just in case...
            {
                return false;
            }

            return true;
        }
    }
}
