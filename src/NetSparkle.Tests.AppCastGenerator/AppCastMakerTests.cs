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
using System.Runtime.InteropServices;
using System.Diagnostics;

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

        private string GetOperatingSystemForAppCastString()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "windows";
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "macos";
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "linux";
            }
            return "";
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
            Assert.Null(AppCastMaker.GetVersionFromName(""));
            Assert.Null(AppCastMaker.GetVersionFromName(null));
            Assert.Null(AppCastMaker.GetVersionFromName("foo"));
            Assert.Null(AppCastMaker.GetVersionFromName("foo1."));
            Assert.Equal("1.0", AppCastMaker.GetVersionFromName("hello 1.0.txt"));
            Assert.Equal("1.0", AppCastMaker.GetVersionFromName("hello 1.0            .txt")); // whitespace shouldn't matter
            Assert.Equal("0", AppCastMaker.GetVersionFromName("hello 1 .0.txt"));
            Assert.Equal("2.3", AppCastMaker.GetVersionFromName("hello a2.3.txt"));
            Assert.Equal("4.3.2", AppCastMaker.GetVersionFromName("My Favorite App 4.3.2.zip"));
            Assert.Equal("1.0", AppCastMaker.GetVersionFromName("foo1.0"));
            Assert.Equal("0.1", AppCastMaker.GetVersionFromName("foo0.1"));
            Assert.Equal("0.1", AppCastMaker.GetVersionFromName("0.1foo"));
            Assert.Equal("0.1", AppCastMaker.GetVersionFromName("0.1 My App"));
            Assert.Equal("0.0.3.1", AppCastMaker.GetVersionFromName("foo0.0.3.1"));
            Assert.Equal("1.2.4", AppCastMaker.GetVersionFromName("foo1.2.4"));
            Assert.Equal("1.2.4.8", AppCastMaker.GetVersionFromName("foo1.2.4.8"));
            Assert.Equal("1.2.4.8", AppCastMaker.GetVersionFromName("1.0bar7.8foo 1.2.4.8"));
            Assert.Equal("2.0", AppCastMaker.GetVersionFromName("1.0bar7.8foo6.3 2.0"));
            Assert.Equal("6.3.2.0", AppCastMaker.GetVersionFromName("1.0bar7.8foo6.3.2.0"));
            // test that it limits version to 4 digits
            Assert.Equal("3.2.1.0", AppCastMaker.GetVersionFromName("My Favorite App 4.3.2.1.0.zip"));
            // test with 0's and with .tar.gz (more than one "piece" to the extension)
            Assert.Null(AppCastMaker.GetVersionFromName(".tar.gz"));
            Assert.Equal("1.0", AppCastMaker.GetVersionFromName("hello 1.0.tar.gz"));
            Assert.Equal("4.3.2", AppCastMaker.GetVersionFromName("My Favorite App 4.3.2.tar.gz"));
            Assert.Equal("0.0.0", AppCastMaker.GetVersionFromName("My Favorite Tools (Linux-x64) 0.0.0.tar.gz"));
            // semantic
            //Assert.Equal("1.1.0", AppCastMaker.GetVersionFromName("1.1.0interruption.1.exe"));
        }

        [Fact]
        public void CanGetVersionFromFolderPath()
        {
            Assert.Equal("2.0.4", AppCastMaker.GetVersionFromName(Path.Combine("output", "2.0.4", "file.ext")));
            Assert.Equal("100.2.303", AppCastMaker.GetVersionFromName(Path.Combine("myapp", "bin", "100.2.303", "myapp.zip")));
            Assert.Equal("1.4.3.1", AppCastMaker.GetVersionFromName(Path.Combine("myapp", "1.4.3.1", "bin", "myapp.zip")));
            Assert.Equal("100.2.303", AppCastMaker.GetVersionFromName(Path.Combine("foo 100.2.303", "myapp.zip")));
            // takes first version that it finds
            Assert.Equal("3.1", AppCastMaker.GetVersionFromName(Path.Combine("myapp", "1.4", "3.1", "bin", "myapp.zip")));
            // only searches 4 folders up
            Assert.Null(AppCastMaker.GetVersionFromName(Path.Combine("boo", "moo", "1.0", "dir", "dir", "myapp", "bin", "myapp.zip")));
            Assert.Null(AppCastMaker.GetVersionFromName(Path.Combine("boo", "moo", "3.0", "1.0", "dir", "myapp", "bin", "myapp.zip")));
            Assert.Equal("1.0", AppCastMaker.GetVersionFromName(Path.Combine("boo", "moo", "3.0", "dir", "1.0", "myapp", "bin", "myapp.zip")));
        }

        [Fact]
        public void CanGetVersionFromFolderPathWithInitialBinaryDir()
        {
            Assert.Equal("2.0.4", AppCastMaker.GetVersionFromName(Path.Combine("output", "2.0.4", "file.ext"), ""));
            Assert.Equal("2.0.4", AppCastMaker.GetVersionFromName(Path.Combine("output", "2.0.4", "file.ext"), null));
            Assert.Equal("2.0.4", AppCastMaker.GetVersionFromName(Path.Combine("output", "2.0.4", "file.ext"), "output" + Path.DirectorySeparatorChar));
            Assert.Equal("2.0.4", AppCastMaker.GetVersionFromName(Path.Combine("output", "foo", "2.0.4", "file.ext"), Path.Combine("output", "foo")));
            Assert.Equal("2.0.4", AppCastMaker.GetVersionFromName(Path.Combine("output", "1.0", "foo", "2.0.4", "file.ext"), Path.Combine("output", "1.0", "foo")));
            Assert.Null(AppCastMaker.GetVersionFromName(Path.Combine("output", "1.0", "file.ext"), Path.Combine("output", "1.0")));
            Assert.Null(AppCastMaker.GetVersionFromName(Path.Combine("output", "1.0", "file.ext"), Path.Combine("output", "1.0", "file.ext")));
        }

        [Fact]
        public void CanGetVersionFromFullPathOnDisk()
        {
            // test a full file path by using the tmp dir
            var tempDir = GetCleanTempDir();
            var subFolder = "foo 100.0.302";
            var subFolderPath = Path.Combine(tempDir, subFolder);
            Directory.CreateDirectory(subFolderPath);
            // create dummy files
            var dummyFilePath = Path.Combine(subFolderPath, "hello.tar.gz");
            // path is now something like c:/tempDir/foo 1.0/hello.tar.gz
            const int fileSizeBytes = 57;
            var tempData = RandomString(fileSizeBytes);
            File.WriteAllText(dummyFilePath, tempData);
            var opts = new Options()
            {
                FileExtractVersion = true,
                SearchBinarySubDirectories = true,
                SourceBinaryDirectory = tempDir,
                Extensions = "tar.gz",
                OutputDirectory = tempDir,
                OperatingSystem = "windows",
                BaseUrl = "https://example.com/downloads",
                OverwriteOldItemsInAppcast = false,
                ReparseExistingAppCast = false,
            };
            try
            {
                var signatureManager = _fixture.GetSignatureManager();
                Assert.True(signatureManager.KeysExist());

                var maker = new XMLAppCastMaker(signatureManager, opts);
                var appCastFileName = maker.GetPathToAppCastOutput(opts.OutputDirectory, opts.SourceBinaryDirectory);
                var (items, productName) = maker.LoadAppCastItemsAndProductName(opts.SourceBinaryDirectory, opts.ReparseExistingAppCast, appCastFileName);
                Assert.Equal("100.0.302", items[0].Version);
            }
            finally
            {
                // make sure tempDir is always cleaned up
                CleanUpDir(tempDir);
            }
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
            // make sure .tar.gz works
            extensions = maker.GetSearchExtensionsFromString("tar.gz, tar, gz");
            Assert.Contains("*.tar.gz", extensions);
            Assert.Contains("*.tar", extensions);
            Assert.Contains("*.gz", extensions);
            Assert.Equal(3, extensions.Count());
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
                BaseUrl = "https://example.com/downloads",
                OverwriteOldItemsInAppcast = false,
                ReparseExistingAppCast = false,
            };

            try
            {
                var signatureManager = _fixture.GetSignatureManager();
                Assert.True(signatureManager.KeysExist());

                var maker = new XMLAppCastMaker(signatureManager, opts);
                var appCastFileName = maker.GetPathToAppCastOutput(opts.OutputDirectory, opts.SourceBinaryDirectory);
                var (items, productName) = maker.LoadAppCastItemsAndProductName(opts.SourceBinaryDirectory, opts.ReparseExistingAppCast, appCastFileName);
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

        [Fact]
        public void SingleDigitVersionDoesNotFail()
        {
            // setup test dir
            var tempDir = GetCleanTempDir();
            // create dummy files
            var dummyFilePath = Path.Combine(tempDir, "hello 1.txt");
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
                BaseUrl = "https://example.com/downloads",
                OverwriteOldItemsInAppcast = false,
                ReparseExistingAppCast = false,
            };

            try
            {
                var signatureManager = _fixture.GetSignatureManager();
                Assert.True(signatureManager.KeysExist());

                var maker = new XMLAppCastMaker(signatureManager, opts);
                var appCastFileName = maker.GetPathToAppCastOutput(opts.OutputDirectory, opts.SourceBinaryDirectory);
                var (items, productName) = maker.LoadAppCastItemsAndProductName(opts.SourceBinaryDirectory, opts.ReparseExistingAppCast, appCastFileName);
                if (items != null)
                {
                    maker.SerializeItemsToFile(items, productName, appCastFileName);
                    maker.CreateSignatureFile(appCastFileName, opts.SignatureFileExtension ?? "signature");
                }

                Assert.Single(items);
                Assert.Equal("1", items[0].Version);
                Assert.Equal("https://example.com/downloads/hello%201.txt", items[0].DownloadLink);
                Assert.True(items[0].DownloadSignature.Length > 0);
                Assert.True(items[0].IsWindowsUpdate);
                Assert.Equal(fileSizeBytes, items[0].UpdateSize);
            }
            finally
            {
                // make sure tempDir always cleaned up
                CleanUpDir(tempDir);
            }
        }

        [Fact]
        public void NoVersionCausesEmptyAppCast()
        {
            // setup test dir
            var tempDir = GetCleanTempDir();
            // create dummy files
            var dummyFilePath = Path.Combine(tempDir, "hello.txt");
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
                BaseUrl = "https://example.com/downloads",
                OverwriteOldItemsInAppcast = false,
                ReparseExistingAppCast = false,
            };

            try
            {
                var signatureManager = _fixture.GetSignatureManager();
                Assert.True(signatureManager.KeysExist());

                var maker = new XMLAppCastMaker(signatureManager, opts);
                var appCastFileName = maker.GetPathToAppCastOutput(opts.OutputDirectory, opts.SourceBinaryDirectory);
                var (items, productName) = maker.LoadAppCastItemsAndProductName(opts.SourceBinaryDirectory, opts.ReparseExistingAppCast, appCastFileName);
                // shouldn't have any items
                Assert.Empty(items);
            }
            finally
            {
                // make sure tempDir always cleaned up
                CleanUpDir(tempDir);
            }
        }

        // https://github.com/NetSparkleUpdater/NetSparkle/discussions/426
        // attempts to reproduce bug but they are not using the --file-extract-version param
        // so we went ahead and kept the test case but couldn't repro bug
        [Fact]
        public void CheckReleaseNotesLink()
        {
            // setup test dir
            var tempDir = GetCleanTempDir();
            var innerAppcastOutputPath = Path.Combine(tempDir, "www");
            var innerBuildPath = Path.Combine(tempDir, "www/builds");
            var innerChangelogPath = Path.Combine(tempDir, "www/changelogs");
            Directory.CreateDirectory(innerBuildPath);
            Directory.CreateDirectory(innerChangelogPath);
            // create dummy files
            var dummyInstallerFilePath = Path.Combine(innerBuildPath, "build_1.0.exe");
            const int fileSizeBytes = 57;
            var tempData = RandomString(fileSizeBytes);
            File.WriteAllText(dummyInstallerFilePath, tempData);
            var dummyChangelogFilePath = Path.Combine(innerChangelogPath, "1.0.md");
            tempData = RandomString(fileSizeBytes);
            File.WriteAllText(dummyChangelogFilePath, tempData);
            var opts = new Options()
            {
                FileExtractVersion = true,
                SearchBinarySubDirectories = true,
                SourceBinaryDirectory = innerBuildPath,
                ChangeLogPath = innerChangelogPath,
                Extensions = "exe",
                OutputDirectory = innerAppcastOutputPath,
                OperatingSystem = "windows",
                ProductName = "ProductName",
                BaseUrl = "https://example.com/downloads",
                ChangeLogUrl = "http://baseURL/appname/changelogs/",
                OverwriteOldItemsInAppcast = true,
                ReparseExistingAppCast = false,
                HumanReadableOutput = true
            };

            try
            {
                var signatureManager = _fixture.GetSignatureManager();
                Assert.True(signatureManager.KeysExist());

                var maker = new XMLAppCastMaker(signatureManager, opts);
                var appCastFileName = maker.GetPathToAppCastOutput(opts.OutputDirectory, opts.SourceBinaryDirectory);
                var (items, productName) = maker.LoadAppCastItemsAndProductName(opts.SourceBinaryDirectory, opts.ReparseExistingAppCast, appCastFileName);
                // shouldn't have any items
                if (items != null)
                {
                    maker.SerializeItemsToFile(items, productName, appCastFileName);
                    maker.CreateSignatureFile(appCastFileName, opts.SignatureFileExtension ?? "signature");
                }
                Console.Write(File.ReadAllText(Path.Combine(innerAppcastOutputPath, "appcast.xml")));
                Assert.Single(items);
                Assert.Equal("1.0", items[0].Version);
                Assert.Equal("http://baseURL/appname/changelogs/1.0.md", items[0].ReleaseNotesLink);
                Assert.True(items[0].DownloadSignature.Length > 0);
                Assert.True(items[0].IsWindowsUpdate);
                Assert.Equal(fileSizeBytes, items[0].UpdateSize);
            }
            finally
            {
                // make sure tempDir always cleaned up
                CleanUpDir(tempDir);
            }
        }

        [Fact]
        public void NetSparkleCanParseHumanReadableAppCast()
        {
            var tempDir = GetCleanTempDir();
            // create dummy file
            var dummyFilePath = Path.Combine(tempDir, "hello 2.0.exe");
            const int fileSizeBytes = 57;
            var tempData = RandomString(fileSizeBytes);
            File.WriteAllText(dummyFilePath, tempData);
            var opts = new Options()
            {
                FileExtractVersion = true,
                SearchBinarySubDirectories = true,
                SourceBinaryDirectory = tempDir,
                Extensions = "exe",
                ProductName = "My Application",
                OutputDirectory = tempDir,
                OperatingSystem = GetOperatingSystemForAppCastString(),
                BaseUrl = "https://example.com/downloads",
                OverwriteOldItemsInAppcast = false,
                ReparseExistingAppCast = false,
                HumanReadableOutput = true,
            };

            try
            {
                var signatureManager = _fixture.GetSignatureManager();
                Assert.True(signatureManager.KeysExist());
                var maker = new XMLAppCastMaker(signatureManager, opts);
                var appCastFileName = maker.GetPathToAppCastOutput(opts.OutputDirectory, opts.SourceBinaryDirectory);
                var (items, productName) = maker.LoadAppCastItemsAndProductName(opts.SourceBinaryDirectory, opts.ReparseExistingAppCast, appCastFileName);
                // should have one item
                Assert.Single(items);
                Assert.Equal("2.0", items[0].Version);
                Assert.Equal("https://example.com/downloads/hello%202.0.exe", items[0].DownloadLink);
                // write to file
                if (items != null)
                {
                    maker.SerializeItemsToFile(items, productName, appCastFileName);
                    maker.CreateSignatureFile(appCastFileName, opts.SignatureFileExtension);
                }
                // for debugging print out app cast
                // Console.WriteLine(File.ReadAllText(appCastFileName));
                // test NetSparkle reading file
                var appCastHandler = new NetSparkleUpdater.AppCastHandlers.XMLAppCast();
                var publicKey = signatureManager.GetPublicKey();
                var publicKeyString = Convert.ToBase64String(publicKey);
                appCastHandler.SetupAppCastHandler(
                        new NetSparkleUpdater.Downloaders.LocalFileAppCastDownloader(), 
                        appCastFileName,
                        new EmptyTestDataConfguration(
                            new FakeTestDataAssemblyAccessor() 
                            {
                                AssemblyVersion = "1.0"
                            }), 
                        new NetSparkleUpdater.SignatureVerifiers.Ed25519Checker(
                            NetSparkleUpdater.Enums.SecurityMode.Strict,
                            publicKeyString),
                        new NetSparkleUpdater.LogWriter(true));
                var didSucceed = appCastHandler.DownloadAndParse();
                Assert.True(didSucceed);
                var updates = appCastHandler.GetAvailableUpdates();
                Assert.Single(updates);
                Assert.Equal("2.0", updates[0].Version);
                Assert.Equal("https://example.com/downloads/hello%202.0.exe", updates[0].DownloadLink);
            }
            finally
            {
                // make sure tempDir always cleaned up
                CleanUpDir(tempDir);
            }
        }

        [Fact]
        public void CanSetVersionViaCLI()
        {
            // setup test dir
            var tempDir = GetCleanTempDir();
            // create dummy files
            var dummyFilePath = Path.Combine(tempDir, "hello.txt");
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
                BaseUrl = "https://example.com/downloads",
                OverwriteOldItemsInAppcast = false,
                ReparseExistingAppCast = false,
                FileVersion = "1.4.1"
            };

            try
            {
                var signatureManager = _fixture.GetSignatureManager();
                Assert.True(signatureManager.KeysExist());

                var maker = new XMLAppCastMaker(signatureManager, opts);
                var appCastFileName = maker.GetPathToAppCastOutput(opts.OutputDirectory, opts.SourceBinaryDirectory);
                var (items, productName) = maker.LoadAppCastItemsAndProductName(opts.SourceBinaryDirectory, opts.ReparseExistingAppCast, appCastFileName);
                // should have 1 item since we set the version via CLI
                Assert.Single(items);
                Assert.Equal("1.4.1", items[0].Version);
                Assert.Equal("https://example.com/downloads/hello.txt", items[0].DownloadLink);
            }
            finally
            {
                // make sure tempDir always cleaned up
                CleanUpDir(tempDir);
            }
        }

        [Fact]
        public void CannotSetVersionViaCLIWithTwoItemsHavingNoVersion()
        {
            // setup test dir
            var tempDir = GetCleanTempDir();
            // create dummy files
            var dummyFilePath = Path.Combine(tempDir, "hello.txt");
            var dummyFilePath2 = Path.Combine(tempDir, "goodbye.txt");
            const int fileSizeBytes = 57;
            var tempData = RandomString(fileSizeBytes);
            File.WriteAllText(dummyFilePath, tempData);
            tempData = RandomString(fileSizeBytes);
            File.WriteAllText(dummyFilePath2, tempData);
            var opts = new Options()
            {
                FileExtractVersion = true,
                SearchBinarySubDirectories = true,
                SourceBinaryDirectory = tempDir,
                Extensions = "txt",
                OutputDirectory = tempDir,
                OperatingSystem = "windows",
                BaseUrl = "https://example.com/downloads",
                OverwriteOldItemsInAppcast = false,
                ReparseExistingAppCast = false,
                FileVersion = "1.4.1"
            };

            try
            {
                var signatureManager = _fixture.GetSignatureManager();
                Assert.True(signatureManager.KeysExist());

                var maker = new XMLAppCastMaker(signatureManager, opts);
                var appCastFileName = maker.GetPathToAppCastOutput(opts.OutputDirectory, opts.SourceBinaryDirectory);
                var (items, productName) = maker.LoadAppCastItemsAndProductName(opts.SourceBinaryDirectory, opts.ReparseExistingAppCast, appCastFileName);
                // items should be null since this is a failure case
                Assert.Null(items);
            }
            finally
            {
                // make sure tempDir always cleaned up
                CleanUpDir(tempDir);
            }
        }

        [Fact]
        public void CanSetCriticalVersion()
        {
            // setup test dir
            var tempDir = GetCleanTempDir();
            // create dummy files
            var dummyFilePath = Path.Combine(tempDir, "hello myapp 1.3.txt");
            var dummyFilePath2 = Path.Combine(tempDir, "hello myapp 1.4.txt");
            const int fileSizeBytes = 57;
            var tempData = RandomString(fileSizeBytes);
            File.WriteAllText(dummyFilePath, tempData);
            tempData = RandomString(fileSizeBytes);
            File.WriteAllText(dummyFilePath2, tempData);
            var opts = new Options()
            {
                FileExtractVersion = true,
                SearchBinarySubDirectories = true,
                SourceBinaryDirectory = tempDir,
                Extensions = "txt",
                OutputDirectory = tempDir,
                OperatingSystem = GetOperatingSystemForAppCastString(),
                BaseUrl = "https://example.com/downloads",
                OverwriteOldItemsInAppcast = false,
                ReparseExistingAppCast = false,
                CriticalVersions = "1.3",
            };

            try
            {
                var signatureManager = _fixture.GetSignatureManager();
                Assert.True(signatureManager.KeysExist());

                var maker = new XMLAppCastMaker(signatureManager, opts);
                var appCastFileName = maker.GetPathToAppCastOutput(opts.OutputDirectory, opts.SourceBinaryDirectory);
                var (items, productName) = maker.LoadAppCastItemsAndProductName(opts.SourceBinaryDirectory, opts.ReparseExistingAppCast, appCastFileName);
                Assert.Equal(2, items.Count());
                // 1.4 should not be marked critical; 1.3 should be
                Assert.Equal("1.4", items[0].Version);
                Assert.False(items[0].IsCriticalUpdate);
                Assert.Equal("1.3", items[1].Version);
                Assert.True(items[1].IsCriticalUpdate);
                // make sure data ends up in file, too
                if (items != null)
                {
                    maker.SerializeItemsToFile(items, productName, appCastFileName);
                    maker.CreateSignatureFile(appCastFileName, opts.SignatureFileExtension ?? "signature");
                }
                // DEBUG: Console.WriteLine(File.ReadAllText(Path.Combine(tempDir, "appcast.xml")));
                // test NetSparkle reading file
                var appCastHandler = new NetSparkleUpdater.AppCastHandlers.XMLAppCast();
                var publicKey = signatureManager.GetPublicKey();
                var publicKeyString = Convert.ToBase64String(publicKey);
                appCastHandler.SetupAppCastHandler(
                        new NetSparkleUpdater.Downloaders.LocalFileAppCastDownloader(), 
                        appCastFileName,
                        new EmptyTestDataConfguration(
                            new FakeTestDataAssemblyAccessor() 
                            {
                                AssemblyVersion = "1.0"
                            }), 
                        new NetSparkleUpdater.SignatureVerifiers.Ed25519Checker(
                            NetSparkleUpdater.Enums.SecurityMode.Strict,
                            publicKeyString),
                        new NetSparkleUpdater.LogWriter(true));
                var didSucceed = appCastHandler.DownloadAndParse();
                Assert.True(didSucceed);
                var updates = appCastHandler.GetAvailableUpdates();
                Assert.Equal(2, updates.Count());
                // 1.4 should not be marked critical; 1.3 should be
                Assert.Equal("1.4", updates[0].Version);
                Assert.False(updates[0].IsCriticalUpdate);
                Assert.Equal("1.3", updates[1].Version);
                Assert.True(updates[1].IsCriticalUpdate);
            }
            finally
            {
                // make sure tempDir always cleaned up
                CleanUpDir(tempDir);
            }
        }

        [Fact]
        public void ChangelogNameInAppcastMatchesFilesystem()
        {
            // setup test dir
            var tempDir = GetCleanTempDir();
            // create dummy files
            var dummyFilePath = Path.Combine(tempDir, "hello 1.0.txt");
            const int fileSizeBytes = 57;
            var tempData = RandomString(fileSizeBytes);
            File.WriteAllText(dummyFilePath, tempData);

            var dummyChangelogFilePath = Path.Combine(tempDir, "change_log_1.0.md");
            tempData = RandomString(fileSizeBytes);
            File.WriteAllText(dummyChangelogFilePath, tempData);
            var opts = new Options()
            {
                FileExtractVersion = true,
                SearchBinarySubDirectories = true,
                SourceBinaryDirectory = tempDir,
                ChangeLogPath = tempDir,
                Extensions = "txt",
                OutputDirectory = tempDir,
                OperatingSystem = "windows",
                ProductName = "ProductName",
                BaseUrl = "https://example.com/downloads",
                ChangeLogUrl = "http://baseURL/appname/changelogs/",
                ChangeLogFileNamePrefix = "change_log_",
                OverwriteOldItemsInAppcast = true,
                ReparseExistingAppCast = false,
                HumanReadableOutput = true
            };

            try
            {
                var signatureManager = _fixture.GetSignatureManager();
                Assert.True(signatureManager.KeysExist());

                var maker = new XMLAppCastMaker(signatureManager, opts);
                var appCastFileName = maker.GetPathToAppCastOutput(opts.OutputDirectory, opts.SourceBinaryDirectory);
                var (items, productName) = maker.LoadAppCastItemsAndProductName(opts.SourceBinaryDirectory, opts.ReparseExistingAppCast, appCastFileName);
                Assert.Single(items);
                Assert.EndsWith("change_log_1.0.md", items[0].ReleaseNotesLink);
            }
            finally
            {
                // make sure tempDir always cleaned up
                CleanUpDir(tempDir);
            }
        }

        [Fact]
        public void CanGetSemVerLikeVersionsFromExistingAppCast()
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
            <title>Version 2.0 Beta 1</title>
            <sparkle:releaseNotesLink>
            https://netsparkleupdater.github.io/NetSparkle/files/sample-app/2.0-release-notes.md
            </sparkle:releaseNotesLink>
            <pubDate>Fri, 28 Oct 2016 10:30:00 +0000</pubDate>
            <enclosure url=""https://netsparkleupdater.github.io/NetSparkle/files/sample-app/NetSparkleUpdate.exe""
                       sparkle:version=""2.0-beta1""
                       sparkle:shortVersionString=""2.0""
                       sparkle:os=""windows""
                       length=""1337""
                       type=""application/octet-stream""
                       sparkle:signature=""foo"" />
        </item>
        <item>
            <title>Version 2.0 Alpha 1</title>
            <sparkle:releaseNotesLink>
            https://netsparkleupdater.github.io/NetSparkle/files/sample-app/2.0-release-notes.md
            </sparkle:releaseNotesLink>
            <pubDate>Fri, 28 Oct 2016 10:30:00 +0000</pubDate>
            <enclosure url=""https://netsparkleupdater.github.io/NetSparkle/files/sample-app/NetSparkleUpdate.exe""
                       sparkle:version=""2.0-alpha.1""
                       sparkle:shortVersionString=""2.0""
                       sparkle:os=""windows""
                       length=""2337""
                       type=""application/octet-stream""
                       sparkle:signature=""bar"" />
        </item>
    </channel>
</rss>
".Trim();
            fakeAppCastFilePath = Path.GetTempFileName();
            File.WriteAllText(fakeAppCastFilePath, appCastData);
            (items, productName) = maker.GetItemsAndProductNameFromExistingAppCast(fakeAppCastFilePath, false);
            Assert.Equal(2, items.Count);
            Assert.Equal("Version 2.0 Beta 1", items[0].Title);
            Assert.Equal("2.0-beta1", items[0].Version);
            Assert.Equal("2.0", items[0].ShortVersion);
            Assert.Equal("-beta1", items[0].SemVerLikeVersion.AllSuffixes);
            Assert.Equal("2.0", items[0].SemVerLikeVersion.Version);
            Assert.Equal(1337, items[0].UpdateSize);
            Assert.Equal("foo", items[0].DownloadSignature);

            Assert.Equal("Version 2.0 Alpha 1", items[1].Title);
            Assert.Equal("2.0-alpha.1", items[1].Version);
            Assert.Equal("2.0", items[1].ShortVersion);
            Assert.Equal("-alpha.1", items[1].SemVerLikeVersion.AllSuffixes);
            Assert.Equal("2.0", items[1].SemVerLikeVersion.Version);
            Assert.Equal(2337, items[1].UpdateSize);
            Assert.Equal("bar", items[1].DownloadSignature);
            // make sure things stay in order when sorted (sort with latest first)
            items.Sort((a, b) => b.SemVerLikeVersion.CompareTo(a.SemVerLikeVersion));
            Assert.Equal("2.0-beta1", items[0].Version);
            Assert.Equal("2.0-alpha.1", items[1].Version);
        }

        [Fact]
        public void CanMakeAppCastWithAssemblyData()
        {
            var envVersion = Environment.Version;
            var dotnetVersion = "net" + envVersion.Major + ".0";
            var csproj = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>" + dotnetVersion + @"</TargetFramework>
    <RootNamespace>csharp_testing</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <Version>2.0.1-beta-1</Version>
    <AssemblyVersion>2.0.1</AssemblyVersion>
  </PropertyGroup>
</Project>".Trim();
            var program = @"Console.WriteLine(""Hello, World!"");";
            var tempDir = GetCleanTempDir();
            var csprojPath = Path.Combine(tempDir, "proj.csproj");
            var programPath = Path.Combine(tempDir, "Program.cs");
            var innerFolder = RandomString(10);
            var buildPath = Directory.CreateDirectory(Path.Combine(tempDir, innerFolder)).FullName;
            try
            {
                
                File.WriteAllText(csprojPath, csproj);
                File.WriteAllText(programPath, program);
                // compile it
                var p = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = "dotnet",
                        WorkingDirectory = tempDir,
                        Arguments = $"build --framework " + dotnetVersion + " --output \"" + buildPath + "\""
                    }
                };
                p.OutputDataReceived += (o, e) => 
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        Console.WriteLine("OUTPUT: " + e.Data);
                    }
                };
                p.ErrorDataReceived += (o, e) => 
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        Console.WriteLine("ERROR: " + e.Data);
                    }
                };
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();     
                p.WaitForExit();
                // ok now that it has built, read the assembly
                var dllPath = Path.Combine(buildPath, "proj.dll");
                if (File.Exists(dllPath))
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(dllPath).ProductVersion?.Trim();
                    Assert.Equal("2.0.1-beta-1", versionInfo);
                    // ok, now use this for the app cast
                    var dummyChangelogFilePath = Path.Combine(buildPath, "2.0.1-beta-1.md");
                    const int fileSizeBytes = 123;
                    var tempData = RandomString(fileSizeBytes);
                    File.WriteAllText(dummyChangelogFilePath, tempData);
                    var opts = new Options()
                    {
                        FileExtractVersion = false,
                        SearchBinarySubDirectories = true,
                        SourceBinaryDirectory = buildPath,
                        ChangeLogPath = buildPath,
                        Extensions = "dll",
                        OutputDirectory = buildPath,
                        OperatingSystem = "windows",
                        ProductName = "ProductName",
                        BaseUrl = "https://example.com/downloads",
                        ChangeLogUrl = "http://baseURL/appname/changelogs/",
                        ChangeLogFileNamePrefix = "change_log_",
                        OverwriteOldItemsInAppcast = true,
                        ReparseExistingAppCast = false,
                        HumanReadableOutput = true
                    };
                    var signatureManager = _fixture.GetSignatureManager();
                    Assert.True(signatureManager.KeysExist());

                    var maker = new XMLAppCastMaker(signatureManager, opts);
                    var appCastFileName = maker.GetPathToAppCastOutput(opts.OutputDirectory, opts.SourceBinaryDirectory);
                    var (items, productName) = maker.LoadAppCastItemsAndProductName(opts.SourceBinaryDirectory, opts.ReparseExistingAppCast, appCastFileName);
                    if (items != null)
                    {
                        maker.SerializeItemsToFile(items, productName, appCastFileName);
                        maker.CreateSignatureFile(appCastFileName, opts.SignatureFileExtension ?? "signature");
                    }
                    Assert.Single(items);
                    Assert.Equal("2.0.1-beta-1", items[0].Version);
                    Assert.Equal("2.0.1", items[0].ShortVersion);
                    Assert.Equal("http://baseURL/appname/changelogs/2.0.1-beta-1.md", items[0].ReleaseNotesLink);
                    Assert.True(items[0].DownloadSignature.Length > 0);
                    Assert.True(items[0].IsWindowsUpdate);
                    Assert.Equal(new FileInfo(dllPath).Length, items[0].UpdateSize);
                }
                else
                {
                    Assert.True(false, "Failed to build assembly");
                }
            }
            finally
            {
                // make sure tempDir always cleaned up
                CleanUpDir(tempDir);
            }
        }
    }
}
