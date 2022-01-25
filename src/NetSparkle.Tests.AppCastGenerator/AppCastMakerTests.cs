using NetSparkleUpdater.AppCastGenerator;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using Xunit;

namespace NetSparkle.Tests.AppCastGenerator
{
    [Collection(SignatureManagerFixture.CollectionName)]
    public class AppCastMakerTests 
    {
        SignatureManagerFixture _fixture;

        public AppCastMakerTests(SignatureManagerFixture fixture)
        {
            _fixture = fixture;
        }

        private string GetCleanTempDir()
        {
            var tempPath = Path.GetTempPath();
            var tempDir = Path.Combine(tempPath, "netsparkle-unit-tests-13927");
            // remove any files set up in previous tests
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        private void CleanUpDir(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }
        }

        [Fact]
        public void CanGetVersionFromName()
        {
            // Version should always be pulled from the right-most version in the app name
            Assert.Null(AppCastMaker.GetVersionFromName("foo"));
            Assert.Null(AppCastMaker.GetVersionFromName("foo1."));
            Assert.Equal("1.0", AppCastMaker.GetVersionFromName("hello 1.0.txt"));
            Assert.Equal("0", AppCastMaker.GetVersionFromName("hello 1 .0.txt"));
            Assert.Equal("2.3", AppCastMaker.GetVersionFromName("hello a2.3.txt"));
            Assert.Equal("4.3.2", AppCastMaker.GetVersionFromName("My Favorite App 4.3.2.zip"));
            Assert.Equal("1.0", AppCastMaker.GetVersionFromName("foo1.0"));
            Assert.Equal("0.1", AppCastMaker.GetVersionFromName("foo0.1"));
            Assert.Equal("0.0.3.1", AppCastMaker.GetVersionFromName("foo0.0.3.1"));
            Assert.Equal("1.2.4", AppCastMaker.GetVersionFromName("foo1.2.4"));
            Assert.Equal("1.2.4.8", AppCastMaker.GetVersionFromName("foo1.2.4.8"));
            Assert.Equal("1.2.4.8", AppCastMaker.GetVersionFromName("1.0bar7.8foo 1.2.4.8"));
            Assert.Equal("2.0", AppCastMaker.GetVersionFromName("1.0bar7.8foo6.3 2.0"));
            // test that it limits version to 4 digits
            Assert.Equal("3.2.1.0", AppCastMaker.GetVersionFromName("My Favorite App 4.3.2.1.0.zip"));
        }

        [Fact]
        public void CanGetSearchExtensions()
        {
            var maker = new XMLAppCastMaker(_fixture.GetSignatureManager(), new Options());
            var extensions = maker.GetSearchExtensionsFromString("");
            Assert.Empty(extensions);
            extensions = maker.GetSearchExtensionsFromString("exe");
            Assert.Contains("*.exe", extensions);
            extensions = maker.GetSearchExtensionsFromString("exe,msi");
            Assert.Contains("*.exe", extensions);
            Assert.Contains("*.msi", extensions);
            // duplicate extensions should be ignored
            extensions = maker.GetSearchExtensionsFromString("exe,msi,msi,exe");
            Assert.Contains("*.exe", extensions);
            Assert.Contains("*.msi", extensions);
            Assert.Equal(2, extensions.Count());
        }

        [Fact]
        public void CanFindBinaries()
        {
            // setup test dir
            var tempDir = GetCleanTempDir();
            // create dummy files
            File.WriteAllText(Path.Combine(tempDir, "hello.txt"), string.Empty);
            File.WriteAllText(Path.Combine(tempDir, "goodbye.txt"), string.Empty);
            File.WriteAllText(Path.Combine(tempDir, "batch.bat"), string.Empty);
            var tempSubDir = Path.Combine(tempDir, "Subdir");
            Directory.CreateDirectory(tempSubDir);
            File.WriteAllText(Path.Combine(tempSubDir, "good-day-sir.txt"), string.Empty);
            File.WriteAllText(Path.Combine(tempSubDir, "there-are-four-lights.txt"), string.Empty);
            File.WriteAllText(Path.Combine(tempSubDir, "please-understand.bat"), string.Empty);
            var maker = new XMLAppCastMaker(_fixture.GetSignatureManager(), new Options());
            var binaryPaths = maker.FindBinaries(tempDir, maker.GetSearchExtensionsFromString("exe"), searchSubdirectories: false);

            try
            {
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
            finally
            {
                // make sure tempDir is always cleaned up
                CleanUpDir(tempDir);
            }
        }

        [Fact]
        public void XMLAppCastHasProperExtension()
        {
            var maker = new XMLAppCastMaker(_fixture.GetSignatureManager(), new Options());
            Assert.Equal("xml", maker.GetAppCastExtension());
        }

        [Fact]
        public void CanGetItemsAndProductNameFromExistingAppCast()
        {
            var maker = new XMLAppCastMaker(_fixture.GetSignatureManager(), new Options());
            // create fake app cast file
            var appCastData = @"";
            var fakeAppCastFilePath = Path.GetTempFileName();
            File.WriteAllText(fakeAppCastFilePath, appCastData);
            var (items, productName) = maker.GetItemsAndProductNameFromExistingAppCast(fakeAppCastFilePath, false);
            Assert.Empty(items);
            Assert.Null(productName);
            // now create something with some actual data!
            appCastData = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<rss xmlns:dc=""http://purl.org/dc/elements/1.1/"" xmlns:sparkle=""http://www.andymatuschak.org/xml-namespaces/sparkle"" version=""2.0"">
    <channel>
        <title>NetSparkle Test App</title>
        <link>https://netsparkleupdater.github.io/NetSparkle/files/sample-app/appcast.xml</link>
        <description>Most recent changes with links to updates.</description>
        <language>en</language>
        <item>
            <title>Version 2.0</title>
            <sparkle:releaseNotesLink>
            https://netsparkleupdater.github.io/NetSparkle/files/sample-app/2.0-release-notes.md
            </sparkle:releaseNotesLink>
            <pubDate>Fri, 28 Oct 2016 10:30:00 +0000</pubDate>
            <enclosure url=""https://netsparkleupdater.github.io/NetSparkle/files/sample-app/NetSparkleUpdate.exe""
                       sparkle:version=""2.0""
                       sparkle:os=""windows""
                       length=""12288""
                       type=""application/octet-stream""
                       sparkle:signature=""foo"" />
        </item>
        <item>
            <title>Version 1.3</title>
            <sparkle:releaseNotesLink>
            https://netsparkleupdater.github.io/NetSparkle/files/sample-app/1.3-release-notes.md
            </sparkle:releaseNotesLink>
            <pubDate>Thu, 27 Oct 2016 10:30:00 +0000</pubDate>
            <enclosure url=""https://netsparkleupdater.github.io/NetSparkle/files/sample-app/NetSparkleUpdate13.exe""
                       sparkle:version=""1.3""
                       sparkle:os=""linux""
                       length=""11555""
                       type=""application/octet-stream""
                       sparkle:signature=""bar"" />
        </item>
    </channel>
</rss>
".Trim();
            fakeAppCastFilePath = Path.GetTempFileName();
            File.WriteAllText(fakeAppCastFilePath, appCastData);
            (items, productName) = maker.GetItemsAndProductNameFromExistingAppCast(fakeAppCastFilePath, false);
            Assert.Equal("NetSparkle Test App", productName);
            Assert.Equal(2, items.Count);
            Assert.Equal("Version 2.0", items[0].Title);
            Assert.Equal("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/2.0-release-notes.md", items[0].ReleaseNotesLink);
            Assert.Equal(28, items[0].PublicationDate.Day);
            Assert.Equal("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/NetSparkleUpdate.exe", items[0].DownloadLink);
            Assert.Equal("windows", items[0].OperatingSystemString);
            Assert.Equal("2.0", items[0].Version);
            Assert.Equal(12288, items[0].UpdateSize);
            Assert.Equal("foo", items[0].DownloadSignature);

            Assert.Equal("Version 1.3", items[1].Title);
            Assert.Equal("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/1.3-release-notes.md", items[1].ReleaseNotesLink);
            Assert.Equal(27, items[1].PublicationDate.Day);
            Assert.Equal(30, items[1].PublicationDate.Minute);
            Assert.Equal("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/NetSparkleUpdate13.exe", items[1].DownloadLink);
            Assert.Equal("linux", items[1].OperatingSystemString);
            Assert.Equal("1.3", items[1].Version);
            Assert.Equal(11555, items[1].UpdateSize);
            Assert.Equal("bar", items[1].DownloadSignature);

            // test duplicate items
            appCastData = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<rss xmlns:dc=""http://purl.org/dc/elements/1.1/"" xmlns:sparkle=""http://www.andymatuschak.org/xml-namespaces/sparkle"" version=""2.0"">
    <channel>
        <title>NetSparkle Test App</title>
        <link>https://netsparkleupdater.github.io/NetSparkle/files/sample-app/appcast.xml</link>
        <description>Most recent changes with links to updates.</description>
        <language>en</language>
        <item>
            <title>Version 2.0</title>
            <sparkle:releaseNotesLink>
            https://netsparkleupdater.github.io/NetSparkle/files/sample-app/2.0-release-notes.md
            </sparkle:releaseNotesLink>
            <pubDate>Fri, 28 Oct 2016 10:30:00 +0000</pubDate>
            <enclosure url=""https://netsparkleupdater.github.io/NetSparkle/files/sample-app/NetSparkleUpdate.exe""
                       sparkle:version=""2.0""
                       sparkle:os=""windows""
                       length=""12288""
                       type=""application/octet-stream""
                       sparkle:signature=""foo"" />
        </item>
        <item>
            <title>Version 1.3</title>
            <sparkle:releaseNotesLink>
            https://netsparkleupdater.github.io/NetSparkle/files/sample-app/1.3-release-notes.md
            </sparkle:releaseNotesLink>
            <pubDate>Thu, 27 Oct 2016 10:30:00 +0000</pubDate>
            <enclosure url=""https://netsparkleupdater.github.io/NetSparkle/files/sample-app/NetSparkleUpdate13.exe""
                       sparkle:version=""1.3""
                       sparkle:os=""linux""
                       length=""11555""
                       type=""application/octet-stream""
                       sparkle:signature=""bar"" />
        </item>
        <item>
            <title>Version 1.3 - The Real Deal</title>
            <sparkle:releaseNotesLink>
            https://netsparkleupdater.github.io/NetSparkle/files/sample-app/1.3-real-release-notes.md
            </sparkle:releaseNotesLink>
            <pubDate>Thu, 27 Oct 2016 12:44:00 +0000</pubDate>
            <enclosure url=""https://netsparkleupdater.github.io/NetSparkle/files/sample-app/NetSparkleUpdate13-real.exe""
                       sparkle:version=""1.3""
                       sparkle:os=""macOS""
                       length=""22222""
                       type=""application/octet-stream""
                       sparkle:signature=""moo"" />
        </item>
    </channel>
</rss>
".Trim();
            fakeAppCastFilePath = Path.GetTempFileName();
            File.WriteAllText(fakeAppCastFilePath, appCastData);
            (items, productName) = maker.GetItemsAndProductNameFromExistingAppCast(fakeAppCastFilePath, true);
            Assert.Equal("NetSparkle Test App", productName);
            Assert.Equal(2, items.Count);
            Assert.Equal("Version 2.0", items[0].Title);
            Assert.Equal("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/2.0-release-notes.md", items[0].ReleaseNotesLink);
            Assert.Equal(28, items[0].PublicationDate.Day);
            Assert.Equal("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/NetSparkleUpdate.exe", items[0].DownloadLink);
            Assert.Equal("windows", items[0].OperatingSystemString);
            Assert.Equal("2.0", items[0].Version);
            Assert.Equal(12288, items[0].UpdateSize);
            Assert.Equal("foo", items[0].DownloadSignature);

            Assert.Equal("Version 1.3 - The Real Deal", items[1].Title);
            Assert.Equal("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/1.3-real-release-notes.md", items[1].ReleaseNotesLink);
            Assert.Equal(27, items[1].PublicationDate.Day);
            Assert.Equal(44, items[1].PublicationDate.Minute);
            Assert.Equal("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/NetSparkleUpdate13-real.exe", items[1].DownloadLink);
            Assert.Equal("macOS", items[1].OperatingSystemString);
            Assert.Equal("1.3", items[1].Version);
            Assert.Equal(22222, items[1].UpdateSize);
            Assert.Equal("moo", items[1].DownloadSignature);
        }

        // https://stackoverflow.com/a/1344242/3938401
        private static string RandomString(int length)
        {
            Random random = new SecureRandom();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [Fact]
        public void CanCreateSimpleAppCast()
        {
            // setup test dir
            var tempDir = GetCleanTempDir();
            // create dummy files
            var dummyFilePath = Path.Combine(tempDir, "hello 1.0.txt");
            const int fileSizeBytes = 57;
            var tempData = RandomString(fileSizeBytes);
            File.WriteAllText(dummyFilePath, tempData);
            var opts = new Options()
            {
                FileExtractVersion = true,
                SearchBinarySubDirectories = true,
                SourceBinaryDirectory = tempDir,
                Extensions = "txt",
                OutputDirectory = tempDir,
                OperatingSystem = "windows",
                BaseUrl = new Uri("https://example.com/downloads")
            };

            try
            {
                var signatureManager = _fixture.GetSignatureManager();
                Assert.True(signatureManager.KeysExist());

                var maker = new XMLAppCastMaker(signatureManager, opts);
                var appCastFileName = maker.GetPathToAppCastOutput(opts.OutputDirectory, opts.SourceBinaryDirectory);
                var (items, productName) = maker.LoadAppCastItemsAndProductName(opts.SourceBinaryDirectory, opts.OverwriteOldItemsInAppcast, appCastFileName);
                if (items != null)
                {
                    maker.SerializeItemsToFile(items, productName, appCastFileName);
                    maker.CreateSignatureFile(appCastFileName, opts.SignatureFileExtension ?? "signature");
                }

                Assert.Single(items);
                Assert.Equal("1.0", items[0].Version);
                Assert.Equal("https://example.com/downloads/hello%201.0.txt", items[0].DownloadLink);
                Assert.True(items[0].DownloadSignature.Length > 0);
                Assert.True(items[0].IsWindowsUpdate);
                Assert.Equal(fileSizeBytes, items[0].UpdateSize);

                // reading from the file, and using the data directly - should result in the same ed25519 signature.
                // see also https://github.com/RubyCrypto/ed25519/blob/main/README.md
                var sigFromFile = signatureManager.GetSignatureForFile(dummyFilePath);
                var sigFromBinaryData = signatureManager.GetSignatureForData(File.ReadAllBytes(dummyFilePath));
                Assert.Equal(sigFromFile, sigFromBinaryData);

                // the sig embedded into the item should also be the same
                Assert.Equal(items[0].DownloadSignature, sigFromFile);
                Assert.True(signatureManager.VerifySignature(dummyFilePath, items[0].DownloadSignature));
                Assert.True(signatureManager.VerifySignature(
                    appCastFileName,
                    File.ReadAllText(appCastFileName + "." + (opts.SignatureFileExtension ?? "signature"))));
                // read back and make sure things are the same
                (items, productName) = maker.GetItemsAndProductNameFromExistingAppCast(appCastFileName, true);
                Assert.Single(items);
                Assert.Equal("1.0", items[0].Version);
                Assert.Equal("https://example.com/downloads/hello%201.0.txt", items[0].DownloadLink);
                Assert.True(items[0].DownloadSignature.Length > 0);
                Assert.True(items[0].IsWindowsUpdate);
                Assert.Equal(fileSizeBytes, items[0].UpdateSize);
                Assert.True(signatureManager.VerifySignature(new FileInfo(dummyFilePath), items[0].DownloadSignature));
            }
            finally
            {
                // make sure tempDir always cleaned up
                CleanUpDir(tempDir);
            }
        }
    }
}
