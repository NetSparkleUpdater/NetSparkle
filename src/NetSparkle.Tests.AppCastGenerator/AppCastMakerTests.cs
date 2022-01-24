using NetSparkleUpdater.AppCastGenerator;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace NetSparkle.Tests.AppCastGenerator
{
    public class AppCastMakerTests
    {
        private SignatureManager GetSignatureManager()
        {
            var manager = new SignatureManager();
            // make sure we don't overwrite user's NetSparkle keys!!
            manager.SetStorageDirectory(Path.Combine(Path.GetTempPath(), "netsparkle-tests"));
            return manager;
        }

        [Fact]
        public void CanGetVersionFromName()
        {
            Assert.Null(AppCastMaker.GetVersionFromName("foo"));
            Assert.Null(AppCastMaker.GetVersionFromName("foo1."));
            Assert.Equal("1.0", AppCastMaker.GetVersionFromName("foo1.0"));
            Assert.Equal("0.1", AppCastMaker.GetVersionFromName("foo0.1"));
            Assert.Equal("0.0.3.1", AppCastMaker.GetVersionFromName("foo0.0.3.1"));
            Assert.Equal("1.2.4", AppCastMaker.GetVersionFromName("foo1.2.4"));
            Assert.Equal("1.2.4.8", AppCastMaker.GetVersionFromName("foo1.2.4.8"));
            Assert.Equal("1.2.4.8", AppCastMaker.GetVersionFromName("1.0bar7.8foo 1.2.4.8"));
            Assert.Equal("2.0", AppCastMaker.GetVersionFromName("1.0bar7.8foo6.3 2.0"));
        }

        [Fact]
        public void CanGetSearchExtensions()
        {
            var maker = new XMLAppCastMaker(GetSignatureManager(), new Options());
            var extensions = maker.GetSearchExtensionsFromString("");
            Assert.Empty(extensions);
            extensions = maker.GetSearchExtensionsFromString("exe");
            Assert.Contains("*.exe", extensions);
            extensions = maker.GetSearchExtensionsFromString("exe,msi");
            Assert.Contains("*.exe", extensions);
            Assert.Contains("*.msi", extensions);
            extensions = maker.GetSearchExtensionsFromString("exe,msi");
            Assert.Contains("*.exe", extensions);
            Assert.Contains("*.msi", extensions);
            Assert.Equal(2, extensions.Count());
        }
    }
}
