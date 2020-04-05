using System;
using System.Collections.Generic;
using System.Text;

namespace NetSparkle.Configurations
{
    public class SavedConfigurationData
    {
        public bool CheckForUpdate { get; set; }
        public DateTime LastCheckTime { get; set; }
        public string VersionToSkip { get; set; }
        public bool DidRunOnce { get; set; }
        public bool ShowDiagnosticWindow { get; set; }
        public DateTime LastConfigUpdate { get; set; }
    }
}
