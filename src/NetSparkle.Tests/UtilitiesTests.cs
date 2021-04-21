using System;
using Xunit;

namespace NetSparkleUnitTests
{
    public class UtilitiesTests
    {
        [Fact]
        public void TestGetVersionString()
        {
            var versionString = NetSparkleUpdater.Utilities.GetVersionString(new Version(1, 0, 0, 0));
            Assert.Equal("1.0", versionString);
            versionString = NetSparkleUpdater.Utilities.GetVersionString(new Version(0, 1, 0, 0));
            Assert.Equal("0.1", versionString);
            versionString = NetSparkleUpdater.Utilities.GetVersionString(new Version(1, 1, 0, 0));
            Assert.Equal("1.1", versionString);
            versionString = NetSparkleUpdater.Utilities.GetVersionString(new Version(0, 0, 1, 0));
            Assert.Equal("0.0.1", versionString);
            versionString = NetSparkleUpdater.Utilities.GetVersionString(new Version(1, 1, 1, 1));
            Assert.Equal("1.1.1.1", versionString);
            versionString = NetSparkleUpdater.Utilities.GetVersionString(new Version(1, 0, 0, 1));
            Assert.Equal("1.0.0.1", versionString);
            versionString = NetSparkleUpdater.Utilities.GetVersionString(new Version(1, 0, 1, 0));
            Assert.Equal("1.0.1", versionString);
            versionString = NetSparkleUpdater.Utilities.GetVersionString(new Version(0, 0, 0, 1));
            Assert.Equal("0.0.0.1", versionString);
        }

        [Fact]
        public void TestGetAbsoluteURL()
        {
            var abosluteURL = NetSparkleUpdater.Utilities.GetAbsoluteURL("https://example.com/program.exe", "https://example.com/appcast.xml");
            Assert.Equal("https://example.com/program.exe", abosluteURL.ToString());
            abosluteURL = NetSparkleUpdater.Utilities.GetAbsoluteURL("program.exe", "https://example.com/appcast.xml");
            Assert.Equal("https://example.com/program.exe", abosluteURL.ToString());
            abosluteURL = NetSparkleUpdater.Utilities.GetAbsoluteURL("program.exe", "https://example.com/subfolder/appcast.xml");
            Assert.Equal("https://example.com/subfolder/program.exe", abosluteURL.ToString());
            abosluteURL = NetSparkleUpdater.Utilities.GetAbsoluteURL("../program.exe", "https://example.com/subfolder/appcast.xml");
            Assert.Equal("https://example.com/program.exe", abosluteURL.ToString());
            abosluteURL = NetSparkleUpdater.Utilities.GetAbsoluteURL("./program.exe", "https://example.com/subfolder/appcast.xml");
            Assert.Equal("https://example.com/subfolder/program.exe", abosluteURL.ToString());
        }
    }
}
