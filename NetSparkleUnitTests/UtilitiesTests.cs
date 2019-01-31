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
    }
}
