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

        [Fact]
        public void CanFindBinaries()
        {
            // setup test dir
            var tempPath = Path.GetTempPath();
            var tempDir = Path.Combine(tempPath, "netsparkle-unit-tests-13927");
            // remove any files set up in previous tests
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempDir);
            // create dummy files
            File.WriteAllText(Path.Combine(tempDir, "hello.txt"), string.Empty);
            File.WriteAllText(Path.Combine(tempDir, "goodbye.txt"), string.Empty);
            File.WriteAllText(Path.Combine(tempDir, "batch.bat"), string.Empty);
            var tempSubDir = Path.Combine(tempDir, "Subdir");
            Directory.CreateDirectory(tempSubDir);
            File.WriteAllText(Path.Combine(tempSubDir, "good-day-sir.txt"), string.Empty);
            File.WriteAllText(Path.Combine(tempSubDir, "there-are-four-lights.txt"), string.Empty);
            File.WriteAllText(Path.Combine(tempSubDir, "please-understand.bat"), string.Empty);
            var maker = new XMLAppCastMaker(GetSignatureManager(), new Options());
            var binaryPaths = maker.FindBinaries(tempDir, maker.GetSearchExtensionsFromString("exe"), searchSubdirectories: false);
            Assert.Empty(binaryPaths);

            binaryPaths = maker.FindBinaries(tempDir, maker.GetSearchExtensionsFromString("txt"), searchSubdirectories: false);
            Assert.Equal(2, binaryPaths.Count());
            Assert.Contains(Path.Combine(tempDir, "hello.txt"), binaryPaths);
            Assert.Contains(Path.Combine(tempDir, "goodbye.txt"), binaryPaths);

            binaryPaths = maker.FindBinaries(tempDir, maker.GetSearchExtensionsFromString("txt,bat"), searchSubdirectories: false);
            Assert.Equal(3, binaryPaths.Count());
            Assert.Contains(Path.Combine(tempDir, "hello.txt"), binaryPaths);
            Assert.Contains(Path.Combine(tempDir, "goodbye.txt"), binaryPaths);
            Assert.Contains(Path.Combine(tempDir, "batch.bat"), binaryPaths);

            binaryPaths = maker.FindBinaries(tempDir, maker.GetSearchExtensionsFromString("txt,bat"), searchSubdirectories: true);
            Assert.Equal(6, binaryPaths.Count());
            Assert.Contains(Path.Combine(tempDir, "hello.txt"), binaryPaths);
            Assert.Contains(Path.Combine(tempDir, "goodbye.txt"), binaryPaths);
            Assert.Contains(Path.Combine(tempDir, "batch.bat"), binaryPaths);
            Assert.Contains(Path.Combine(tempSubDir, "good-day-sir.txt"), binaryPaths);
            Assert.Contains(Path.Combine(tempSubDir, "there-are-four-lights.txt"), binaryPaths);
            Assert.Contains(Path.Combine(tempSubDir, "please-understand.bat"), binaryPaths);
        }
    }
}
