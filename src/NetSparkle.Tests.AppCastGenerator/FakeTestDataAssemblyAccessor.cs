using NetSparkleUpdater.Interfaces;

namespace NetSparkle.Tests.AppCastGenerator
{
    public class FakeTestDataAssemblyAccessor : IAssemblyAccessor
    {
        public string AssemblyCompany { get; set; }
        public string AssemblyCopyright { get; set; }
        public string AssemblyDescription { get; set; }
        public string AssemblyTitle { get; set; }
        public string AssemblyProduct { get; set; }
        public string AssemblyVersion { get; set; }
    }
}