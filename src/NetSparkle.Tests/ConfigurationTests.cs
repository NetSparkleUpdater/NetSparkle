using System;
using System.IO;
using NetSparkleUpdater.Configurations;
using Xunit;

namespace NetSparkleUnitTests
{
    public class ConfigurationTests
    {
        [Fact]
        public void CanUseCustomJsonConfigPath()
        {
            // create tmp file 
            var path = Path.Combine(Path.GetTempPath(), "SparkleUpdaterTesting");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            try
            {
                var jsonConfig = new JSONConfiguration(new FakeTestDataAssemblyAccessor()
                {
                    AssemblyCompany = "UnitTest",
                    AssemblyProduct = "UnitTest"
                }, path);
                Assert.Equal(path, jsonConfig.GetSavePath());
            }
            finally
            {
                // clean up created signature manager even if test fails
                // get rid of temp file
                Directory.Delete(path);
            }
        }
    }
}
