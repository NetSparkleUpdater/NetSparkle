using NetSparkle.AssemblyAccessors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace NetSparkle.Configurations
{
    class JSONConfiguration : Configuration
    {
        private string _savePath;

        /// <summary>
        /// The constructor reads out all configured values
        /// </summary>        
        /// <param name="referenceAssembly">the reference assembly name</param>
        public JSONConfiguration(string referenceAssembly)
            : this(referenceAssembly, true)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="referenceAssembly">the name of the reference assembly</param>
        /// <param name="isReflectionBasedAssemblyAccessorUsed"><c>true</c> if reflection is used to access the assembly.</param>
        public JSONConfiguration(string referenceAssembly, bool isReflectionBasedAssemblyAccessorUsed)
            : this(referenceAssembly, isReflectionBasedAssemblyAccessorUsed, string.Empty)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="referenceAssembly">the name of hte reference assembly</param>
        /// <param name="isReflectionBasedAssemblyAccessorUsed"><c>true</c> if reflection is used to access the assembly.</param>
        /// <param name="savePath"><c>true</c> if reflection is used to access the assembly.</param>
        public JSONConfiguration(string referenceAssembly, bool isReflectionBasedAssemblyAccessorUsed, string savePath)
            : base(referenceAssembly, isReflectionBasedAssemblyAccessorUsed)
        {
            _savePath = savePath ?? GetSavePath();
            try
            {
                // get the save path
                _savePath = GetSavePath();
                // load the values
                LoadValuesFromPath(_savePath);
            }
            catch (NetSparkleException)
            {
                // disable update checks when exception occurred -- can't read/save necessary update file 
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
                AssemblyAccessor accessor = new AssemblyAccessor(ReferenceAssembly, UseReflectionBasedAssemblyAccessor);

                if (string.IsNullOrEmpty(accessor.AssemblyCompany) || string.IsNullOrEmpty(accessor.AssemblyProduct))
                {
                    throw new NetSparkleException("STOP: Sparkle is missing the company or productname tag in " + ReferenceAssembly);
                }
                var applicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify);
                var saveFolder = Path.Combine(applicationFolder, accessor.AssemblyCompany, accessor.AssemblyProduct, "NetSparkleUpdater");
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
                    var data = JsonConvert.DeserializeObject<SavedConfigurationData>(json);
                    CheckForUpdate = data.CheckForUpdate;
                    LastCheckTime = data.LastCheckTime;
                    LastVersionSkipped = data.LastVersionSkipped;
                    DidRunOnce = data.DidRunOnce;
                    LastConfigUpdate = data.LastConfigUpdate;
                }
                catch (Exception) // just in case...
                {
                }
            }
            CheckForUpdate = false;
            LastCheckTime = DateTime.Now;
            LastVersionSkipped = string.Empty;
            DidRunOnce = false;
            LastConfigUpdate = DateTime.Now;
            return true;
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
                CheckForUpdate = this.CheckForUpdate,
                LastCheckTime = this.LastCheckTime,
                LastVersionSkipped = this.LastVersionSkipped,
                DidRunOnce = this.DidRunOnce,
                LastConfigUpdate = DateTime.Now
            };
            LastConfigUpdate = savedConfig.LastConfigUpdate;

            string json = JsonConvert.SerializeObject(savedConfig);
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
