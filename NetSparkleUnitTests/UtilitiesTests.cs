using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetSparkleUnitTests
{
    [TestClass]
    public class UtilitiesTests
    {
        [TestMethod]
        public void TestGetVersionString()
        {
            var versionString = NetSparkle.Utilities.GetVersionString(new Version(1, 0, 0, 0));
            Assert.AreEqual("1.0", versionString);
            versionString = NetSparkle.Utilities.GetVersionString(new Version(0, 1, 0, 0));
            Assert.AreEqual("0.1", versionString);
            versionString = NetSparkle.Utilities.GetVersionString(new Version(1, 1, 0, 0));
            Assert.AreEqual("1.1", versionString);
            versionString = NetSparkle.Utilities.GetVersionString(new Version(0, 0, 1, 0));
            Assert.AreEqual("0.0.1", versionString);
            versionString = NetSparkle.Utilities.GetVersionString(new Version(1, 1, 1, 1));
            Assert.AreEqual("1.1.1.1", versionString);
            versionString = NetSparkle.Utilities.GetVersionString(new Version(1, 0, 0, 1));
            Assert.AreEqual("1.0.0.1", versionString);
            versionString = NetSparkle.Utilities.GetVersionString(new Version(1, 0, 1, 0));
            Assert.AreEqual("1.0.1", versionString);
            versionString = NetSparkle.Utilities.GetVersionString(new Version(0, 0, 0, 1));
            Assert.AreEqual("0.0.0.1", versionString);
        }

        [TestMethod]
        public void TestGetAbsoluteURL()
        {
            var abosluteURL = NetSparkle.Utilities.GetAbsoluteURL("https://example.com/program.exe", "https://example.com/appcast.xml");
            Assert.AreEqual("https://example.com/program.exe", abosluteURL.ToString());
            abosluteURL = NetSparkle.Utilities.GetAbsoluteURL("program.exe", "https://example.com/appcast.xml");
            Assert.AreEqual("https://example.com/program.exe", abosluteURL.ToString());
            abosluteURL = NetSparkle.Utilities.GetAbsoluteURL("program.exe", "https://example.com/subfolder/appcast.xml");
            Assert.AreEqual("https://example.com/subfolder/program.exe", abosluteURL.ToString());
            abosluteURL = NetSparkle.Utilities.GetAbsoluteURL("../program.exe", "https://example.com/subfolder/appcast.xml");
            Assert.AreEqual("https://example.com/program.exe", abosluteURL.ToString());
            abosluteURL = NetSparkle.Utilities.GetAbsoluteURL("./program.exe", "https://example.com/subfolder/appcast.xml");
            Assert.AreEqual("https://example.com/subfolder/program.exe", abosluteURL.ToString());
        }
    }
}
