using System;
using System.IO;

namespace NetSparkleUpdater.AppCastGenerator
{
    public class FileKeysProvider : IKeysProvider
    {
        public const string FolderName = "netsparkle";
        public const string PrivateKeyFileName = "NetSparkle_Ed25519.priv";
        public const string PublicKeyFileName = "NetSparkle_Ed25519.pub";

        private string _storagePath = string.Empty;
        private string _privateKeyFilePath = string.Empty;
        private string _publicKeyFilePath = string.Empty;

        public FileKeysProvider()
        {
            SetStorageDirectory(GetDefaultStorageDirectory());
        }

        public static string GetDefaultStorageDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FolderName);
        }

        public void SetStorageDirectory(string path)
        {
            _storagePath = path;

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }

            _privateKeyFilePath = Path.Combine(_storagePath, PrivateKeyFileName);
            _publicKeyFilePath = Path.Combine(_storagePath, PublicKeyFileName);
        }

        public string GetStorageDirectory() => _storagePath;

        public byte[]? GetPrivateKey() => GetKey(_privateKeyFilePath);

        public void SetPrivateKey(string value) => SetKey(_privateKeyFilePath, value);

        public byte[]? GetPublicKey() => GetKey(_publicKeyFilePath);

        public void SetPublicKey(string value) => SetKey(_publicKeyFilePath, value);

        private static byte[]? GetKey(string filePath)
        {
            if (File.Exists(filePath))
            {
                var key = File.ReadAllText(filePath);

                return Convert.FromBase64String(key);
            }

            return null;
        }

        private static void SetKey(string filePath, string value) => File.WriteAllText(filePath, value);

        /// <summary>
        /// Deletes existing keys off disk. You shouldn't call this if they aren't backed up.
        /// Useful for cleaning up unit tests.
        /// </summary>
        public void DeleteKeys()
        {
            if (File.Exists(_publicKeyFilePath))
            {
                File.Delete(_publicKeyFilePath);
            }

            if (File.Exists(_privateKeyFilePath))
            {
                File.Delete(_privateKeyFilePath);
            }
        }
    }
}
