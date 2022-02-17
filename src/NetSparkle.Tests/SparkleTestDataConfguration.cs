using NetSparkleUpdater.Configurations;
using NetSparkleUpdater.Interfaces;

namespace NetSparkleUnitTests
{
    public class SparkleTestDataAssemblyAccessor : IAssemblyAccessor
    {
        public string AssemblyCompany { get; set; }
        public string AssemblyCopyright { get; set; }
        public string AssemblyDescription { get; set; }
        public string AssemblyTitle { get; set; }
        public string AssemblyProduct { get; set; }
        public string AssemblyVersion { get; set; }
    }
    
    public class SparkleTestDataConfguration : Configuration
    {
        public SparkleTestDataConfguration(IAssemblyAccessor accessor) : base(accessor) 
        {
        }

        public override void Reload()
        {
        }
    }
}