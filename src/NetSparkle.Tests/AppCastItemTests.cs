using NetSparkleUpdater;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Xunit;

namespace NetSparkleUnitTests
{
    public class AppCastItemTests
    {
        [Theory]
        [InlineData(null, true, false, false)]
        [InlineData("win", true, false, false)]
        [InlineData("windows", true, false, false)]
        [InlineData("WINDOWS", true, false, false)]
        [InlineData("WIndOWS", true, false, false)]
        [InlineData("win-x64", true, false, false)]
        [InlineData("win-arm", true, false, false)]
        [InlineData("windows-arm", true, false, false)]
        [InlineData("windows-x86", true, false, false)]
        [InlineData("mac", false, true, false)]
        [InlineData("macos", false, true, false)]
        [InlineData("osx", false, true, false)]
        [InlineData("mac-x86", false, true, false)]
        [InlineData("macOS-x86", false, true, false)]
        [InlineData("macos-arm", false, true, false)]
        [InlineData("macos-x64", false, true, false)]
        [InlineData("osx-x86", false, true, false)]
        [InlineData("osx-arm", false, true, false)]
        [InlineData("osx-x64", false, true, false)]
        [InlineData("linux", false, false, true)]
        [InlineData("linux-x64", false, false, true)]
        [InlineData("linux-arm64", false, false, true)]
        [InlineData("linux-arm", false, false, true)]
        public void CanCheckOSProperly(string osString, bool isWindowsUpdate, bool isMacUpdate, bool isLinuxUpdate)
        {
            var item = new AppCastItem() { OperatingSystem = osString };
            Assert.Equal(item.IsWindowsUpdate, isWindowsUpdate);
            Assert.Equal(item.IsMacOSUpdate, isMacUpdate);
            Assert.Equal(item.IsLinuxUpdate, isLinuxUpdate);
        }

        [Theory]
        [InlineData("WINDOWS", true, false, false)]
        [InlineData("OSX", false, true, false)]
        [InlineData("LINUX", false, false, true)]
        public void OSCheckIsNotAffectedByCurrentCulture(string osString, bool isWindowsUpdate, bool isMacOSUpdate, bool isLinuxUpdate)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                // in tr-TR, "I".ToLower() is a dotless "ı", which broke OS string matching
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
                var item = new AppCastItem() { OperatingSystem = osString };
                Assert.Equal(isWindowsUpdate, item.IsWindowsUpdate);
                Assert.Equal(isMacOSUpdate, item.IsMacOSUpdate);
                Assert.Equal(isLinuxUpdate, item.IsLinuxUpdate);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }
    }
}
