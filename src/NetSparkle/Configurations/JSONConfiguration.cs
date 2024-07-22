#nullable enable

using NetSparkleUpdater.Interfaces;
using System;
using System.IO;
using System.Text.Json;

namespace NetSparkleUpdater.Configurations
{
    /// <summary>
    /// A configuration subclsas that can save and load its data from a JSON
    /// file that lives on disk. This type of <see cref="Configuration"/> can
    /// be used on any operating system where you can read/write files.
    /// </summary>
    public class JSONConfiguration : Configuration
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
        /// <exception cref="NetSparkleException">Thrown when the configuration data cannot be read or saved</exception>
        public JSONConfiguration(IAssemblyAccessor assemblyAccessor, string? savePath)
            : base(assemblyAccessor)
        {
            _savePath = savePath != null && !string.IsNullOrWhiteSpace(savePath) ? savePath : GetSavePath();
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

        /// <inheritdoc/>
        public override void TouchProfileTime()
        {
            base.TouchProfileTime();
            // save the values
            SaveValuesToPath(GetSavePath());
        }

        /// <inheritdoc/>
        public override void TouchCheckTime()
        {
            base.TouchCheckTime();
            // save the values
            SaveValuesToPath(GetSavePath());
        }

        /// <inheritdoc/>
        public override void SetVersionToSkip(string version)
        {
            base.SetVersionToSkip(version);
            SaveValuesToPath(GetSavePath());
        }

        /// <inheritdoc/>
        public override void Reload()
        {
            LoadValuesFromPath(GetSavePath());
        }

        private string? GetDirectory(Environment.SpecialFolder specialFolder)
        {
            try {
                var applicationFolder = Environment.GetFolderPath(specialFolder, Environment.SpecialFolderOption.DoNotVerify);
                var saveFolder = Path.Combine(applicationFolder, AssemblyAccessor.AssemblyCompany, AssemblyAccessor.AssemblyProduct, "NetSparkleUpdater");
                if (!Directory.Exists(saveFolder))
                {
                    Directory.CreateDirectory(saveFolder);
                }
                return saveFolder;
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Get the full file path to the location and file name on disk
        /// where the JSON configuration data should be saved.
        /// By default, stored in <seealso cref="Environment.SpecialFolder.ApplicationData"/> in
        /// the "NetSparkleUpdater" folder in the "data.json" file.
        /// </summary>
        /// <exception cref="NetSparkleException">Thrown when the assembly accessor does not have the company or product name
        /// information available</exception>
        public virtual string GetSavePath()
        {
            if (!string.IsNullOrEmpty(_savePath))
            {
                return _savePath;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(AssemblyAccessor?.AssemblyCompany) || 
                    string.IsNullOrWhiteSpace(AssemblyAccessor?.AssemblyProduct))
                {
                    throw new NetSparkleException("Error: NetSparkleUpdater is missing the company or product name tag in the assembly accessor ("
                        + (AssemblyAccessor?.GetType().ToString() ?? "[null]") + ")");
                }
                var saveFolder = GetDirectory(Environment.SpecialFolder.ApplicationData);
                if (saveFolder == null)
                {
                    saveFolder = GetDirectory(Environment.SpecialFolder.Personal);
                    if (saveFolder == null)
                    {
                        saveFolder = GetDirectory(Environment.SpecialFolder.LocalApplicationData);
                    }
                }
                if (saveFolder == null)
                {
                    throw new NetSparkleException("Unable to create folder to store JSONConfiguration data");
                }
                var saveLocation = Path.Combine(saveFolder, "data.json");
                return saveLocation;
            }
        }

        /// <summary>
        /// Load configuration values from the json file at the given path.
        /// </summary>
        /// <param name="saveLocation">the configuration file location</param>
        /// <returns><c>true</c> if the items were loaded, false if the file didn't exist or was unable to be loaded</returns>
        private bool LoadValuesFromPath(string saveLocation)
        {
            if (File.Exists(saveLocation))
            {
                try
                {
                    string json = File.ReadAllText(saveLocation);
#if NETFRAMEWORK || NETSTANDARD
                    var data = JsonSerializer.Deserialize<SavedConfigurationData>(json);
#else
                    var data = JsonSerializer.Deserialize<SavedConfigurationData>(json, SourceGenerationContext.Default.SavedConfigurationData);
#endif
                    CheckForUpdate = true;
                    LastCheckTime = data?.LastCheckTime ?? new DateTime(0);
                    LastVersionSkipped = data?.LastVersionSkipped ?? string.Empty;
                    DidRunOnce = data?.DidRunOnce ?? false;
                    IsFirstRun = !DidRunOnce;
                    LastConfigUpdate = data?.LastConfigUpdate ?? new DateTime(0);
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
        /// Store the configuration information to disk as json
        /// </summary>
        /// <param name="savePath">the save path to the json file</param>
        /// <returns><c>true</c> if the values were saved to dis, false otherwise</returns>
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

#if NETFRAMEWORK || NETSTANDARD
            string json = JsonSerializer.Serialize(savedConfig);
#else
            string json = JsonSerializer.Serialize(savedConfig, SourceGenerationContext.Default.SavedConfigurationData);
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
