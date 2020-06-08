using System;
using Foundation;
using NetSparkleUpdater.Configurations;

namespace NetSparkle.Xamarin
{
    public class InfoPlistConfiguration : Configuration
    {
        private Data data;

        public InfoPlistConfiguration(string companyName) : base("Info.plist", false)
        {
            data = new Data();

            InstalledVersion = data.InstalledVersion;
        }

        private void LoadValuesFromInfoPlist()
        {

            CheckForUpdate = true;
            LastCheckTime = data.LastCheckTime;
            LastVersionSkipped = data.LastVersionSkipped;
            DidRunOnce = data.DidRunOnce;
            IsFirstRun = !DidRunOnce;
            LastConfigUpdate = data.LastConfigUpdate;
            PreviousVersionOfSoftwareRan = data?.PreviousVersionOfSoftwareRan ?? "";
        }

        public override void Reload()
        {
            LoadValuesFromInfoPlist();
        }
    }
}
