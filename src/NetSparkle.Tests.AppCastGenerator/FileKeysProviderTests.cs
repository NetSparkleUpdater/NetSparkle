using NetSparkleUpdater.AppCastGenerator;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace NetSparkle.Tests.AppCastGenerator
{
    public class FileKeysProviderTests : IDisposable
    {
        private string _testDirectory;

        private FileKeysProvider _provider;

        public FileKeysProviderTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), $"FileKeysProvider-Tests-{Guid.NewGuid()}");

            _provider = new FileKeysProvider();
            _provider.SetStorageDirectory(_testDirectory);
        }

        [Fact]
        public void Constructor_CreatesDefaultStorageDirectory()
        {
            var provider = new FileKeysProvider();
            var defaultDirectory = FileKeysProvider.GetDefaultStorageDirectory();

            Assert.NotNull(provider.GetStorageDirectory());
            Assert.NotEmpty(provider.GetStorageDirectory());
        }

        [Fact]
        public void GetDefaultStorageDirectory_ReturnsLocalApplicationDataPath()
        {
            var defaultPath = FileKeysProvider.GetDefaultStorageDirectory();
            var expectedPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                FileKeysProvider.FolderName);

            Assert.Equal(expectedPath, defaultPath);
        }

        [Fact]
        public void SetStorageDirectory_CreatesDirectoryIfNotExists()
        {
            var newPath = Path.Combine(Path.GetTempPath(), $"new-storage-{Guid.NewGuid()}");

            Assert.False(Directory.Exists(newPath));

            _provider.SetStorageDirectory(newPath);

            Assert.True(Directory.Exists(newPath));

            Directory.Delete(newPath, true);
        }

        [Fact]
        public void SetStorageDirectory_UpdatesStoragePath()
        {
            var newPath = Path.Combine(Path.GetTempPath(), $"storage-update-{Guid.NewGuid()}");

            _provider.SetStorageDirectory(newPath);

            Assert.Equal(newPath, _provider.GetStorageDirectory());

            if (Directory.Exists(newPath))
            {
                Directory.Delete(newPath, true);
            }
        }

        [Fact]
        public void GetStorageDirectory_ReturnsPreviouslySetPath()
        {
            var storagePath = _provider.GetStorageDirectory();

            Assert.Equal(_testDirectory, storagePath);
        }

        [Fact]
        public void GetPrivateKey_ReturnsNullWhenFileDoesNotExist()
        {
            var privateKey = _provider.GetPrivateKey();

            Assert.Null(privateKey);
        }

        [Fact]
        public void GetPublicKey_ReturnsNullWhenFileDoesNotExist()
        {
            var publicKey = _provider.GetPublicKey();

            Assert.Null(publicKey);
        }

        [Fact]
        public void SetPrivateKey_CreatesKeyFile()
        {
            var keyContent = Convert.ToBase64String(Encoding.UTF8.GetBytes("private-key-content"));
            var expectedKeyPath = Path.Combine(_testDirectory, FileKeysProvider.PrivateKeyFileName);

            _provider.SetPrivateKey(keyContent);

            Assert.True(File.Exists(expectedKeyPath));
        }

        [Fact]
        public void SetPublicKey_CreatesKeyFile()
        {
            var keyContent = Convert.ToBase64String(Encoding.UTF8.GetBytes("public-key-content"));
            var expectedKeyPath = Path.Combine(_testDirectory, FileKeysProvider.PublicKeyFileName);

            _provider.SetPublicKey(keyContent);

            Assert.True(File.Exists(expectedKeyPath));
        }

        [Fact]
        public void SetPrivateKey_WriteToCorrectFile()
        {
            var keyContent = Convert.ToBase64String(Encoding.UTF8.GetBytes("test-private-key"));

            _provider.SetPrivateKey(keyContent);
            var retrievedKey = _provider.GetPrivateKey();

            Assert.NotNull(retrievedKey);
            Assert.Equal(Encoding.UTF8.GetBytes("test-private-key"), retrievedKey);
        }

        [Fact]
        public void SetPublicKey_WriteToCorrectFile()
        {
            var keyContent = Convert.ToBase64String(Encoding.UTF8.GetBytes("test-public-key"));

            _provider.SetPublicKey(keyContent);
            var retrievedKey = _provider.GetPublicKey();

            Assert.NotNull(retrievedKey);
            Assert.Equal(Encoding.UTF8.GetBytes("test-public-key"), retrievedKey);
        }

        [Fact]
        public void GetPrivateKey_ReturnsDecodedBase64Content()
        {
            var originalContent = new byte[] { 1, 2, 3, 4, 5 };
            var base64Content = Convert.ToBase64String(originalContent);

            _provider.SetPrivateKey(base64Content);
            var retrievedKey = _provider.GetPrivateKey();

            Assert.NotNull(retrievedKey);
            Assert.Equal(originalContent, retrievedKey);
        }

        [Fact]
        public void GetPublicKey_ReturnsDecodedBase64Content()
        {
            var originalContent = new byte[] { 10, 20, 30, 40, 50 };
            var base64Content = Convert.ToBase64String(originalContent);

            _provider.SetPublicKey(base64Content);
            var retrievedKey = _provider.GetPublicKey();

            Assert.NotNull(retrievedKey);
            Assert.Equal(originalContent, retrievedKey);
        }

        [Fact]
        public void DeleteKeys_RemovesPrivateKeyFile()
        {
            var keyContent = Convert.ToBase64String(Encoding.UTF8.GetBytes("private-key"));
            _provider.SetPrivateKey(keyContent);

            var keyPath = Path.Combine(_testDirectory, FileKeysProvider.PrivateKeyFileName);
            Assert.True(File.Exists(keyPath));

            _provider.DeleteKeys();

            Assert.False(File.Exists(keyPath));
        }

        [Fact]
        public void DeleteKeys_RemovesPublicKeyFile()
        {
            var keyContent = Convert.ToBase64String(Encoding.UTF8.GetBytes("public-key"));
            _provider.SetPublicKey(keyContent);

            var keyPath = Path.Combine(_testDirectory, FileKeysProvider.PublicKeyFileName);
            Assert.True(File.Exists(keyPath));

            _provider.DeleteKeys();

            Assert.False(File.Exists(keyPath));
        }

        [Fact]
        public void DeleteKeys_RemovesBothKeyFiles()
        {
            var privateKeyContent = Convert.ToBase64String(Encoding.UTF8.GetBytes("private"));
            var publicKeyContent = Convert.ToBase64String(Encoding.UTF8.GetBytes("public"));

            _provider.SetPrivateKey(privateKeyContent);
            _provider.SetPublicKey(publicKeyContent);

            var privateKeyPath = Path.Combine(_testDirectory, FileKeysProvider.PrivateKeyFileName);
            var publicKeyPath = Path.Combine(_testDirectory, FileKeysProvider.PublicKeyFileName);

            Assert.True(File.Exists(privateKeyPath));
            Assert.True(File.Exists(publicKeyPath));

            _provider.DeleteKeys();

            Assert.False(File.Exists(privateKeyPath));
            Assert.False(File.Exists(publicKeyPath));
        }

        [Theory]
        [InlineData("NetSparkle_Ed25519.priv", FileKeysProvider.PrivateKeyFileName)]
        [InlineData("NetSparkle_Ed25519.pub", FileKeysProvider.PublicKeyFileName)]
        [InlineData("netsparkle", FileKeysProvider.FolderName)]
        public void Constants_HasExpectedValue(string expectedValue, string actualValue)
        {
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void SetStorageDirectory_WithExistingDirectory_DoesNotThrow()
        {
            var existingPath = Path.Combine(Path.GetTempPath(), $"existing-{Guid.NewGuid()}");

            Directory.CreateDirectory(existingPath);

            try
            {
                _provider.SetStorageDirectory(existingPath);

                Assert.Equal(existingPath, _provider.GetStorageDirectory());
            }
            finally
            {
                if (Directory.Exists(existingPath))
                {
                    Directory.Delete(existingPath, true);
                }
            }
        }

        [Fact]
        public void MultipleProviders_UseSeparateStorageDirectories()
        {
            var provider1 = new FileKeysProvider();
            var provider2 = new FileKeysProvider();

            var path1 = Path.Combine(Path.GetTempPath(), $"provider1-{Guid.NewGuid()}");
            var path2 = Path.Combine(Path.GetTempPath(), $"provider2-{Guid.NewGuid()}");

            provider1.SetStorageDirectory(path1);
            provider2.SetStorageDirectory(path2);

            var keyContent = Convert.ToBase64String(Encoding.UTF8.GetBytes("key"));
            provider1.SetPrivateKey(keyContent);
            provider2.SetPublicKey(keyContent);

            var key1Private = provider1.GetPrivateKey();
            var key1Public = provider1.GetPublicKey();
            var key2Private = provider2.GetPrivateKey();
            var key2Public = provider2.GetPublicKey();

            Assert.NotNull(key1Private);
            Assert.Null(key1Public);
            Assert.Null(key2Private);
            Assert.NotNull(key2Public);

            provider1.DeleteKeys();
            provider2.DeleteKeys();

            if (Directory.Exists(path1)) Directory.Delete(path1, true);
            if (Directory.Exists(path2)) Directory.Delete(path2, true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _provider.DeleteKeys();

                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
        }
    }
}
