using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Chaos.NaCl;

namespace NetSparkleUpdater.AppCastGenerator
{
    public class SignatureManager
    {
        private string _storagePath;
        private string _privateKeyFilePath;
        private string _publicKeyFilePath;
        private string _privateKeyOverride;
        private string _publicKeyOverride;

        public const string PrivateKeyEnvironmentVariable = "SPARKLE_PRIVATE_KEY";
        public const string PublicKeyEnvironmentVariable = "SPARKLE_PUBLIC_KEY";

        public SignatureManager()
        {
            SetStorageDirectory(GetDefaultStorageDirectory());
            _privateKeyOverride = "";
            _publicKeyOverride = "";
        }

        public static string GetDefaultStorageDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "netsparkle");
        }

        public void SetStorageDirectory(string path)
        {
            _storagePath = path;

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }

            _privateKeyFilePath = Path.Combine(_storagePath, "NetSparkle_Ed25519.priv");
            _publicKeyFilePath = Path.Combine(_storagePath, "NetSparkle_Ed25519.pub");
        }

        public string GetStorageDirectory()
        {
            return _storagePath;
        }

        public void SetPublicKeyOverride(string overridePubKey)
        {
            _publicKeyOverride = overridePubKey;
        }

        public void SetPrivateKeyOverride(string overridePrivKey)
        {
            _privateKeyOverride = overridePrivKey;
        }

        public bool KeysExist()
        {
            if (GetPublicKey() != null && GetPrivateKey() != null)
            {
                return true;
            }

            return false;
        }

        public bool Generate(bool force = false)
        {

            if (KeysExist() && !force)
            {
                Console.WriteLine("Keys already exist, use --force to force regeneration");
                return false;
            }

            // start key generation
            Console.WriteLine("Generating key pair...");

            var seed = RandomNumberGenerator.GetBytes(32);
            //Ed25519.KeyPairFromSeed(out byte[] publicKey, out byte[] privateKey, seed);
            var publicKey = Ed25519.PublicKeyFromSeed(seed);

            var pubKeyBase64 = Convert.ToBase64String(publicKey);
            var privKeyBase64 = Convert.ToBase64String(seed);

            File.WriteAllText(_privateKeyFilePath, privKeyBase64);
            File.WriteAllText(_publicKeyFilePath, pubKeyBase64);

            Console.WriteLine("Storing public/private keys to " + _storagePath);
            return true;
        }

        public bool VerifySignature(string filePath, string signature)
        {
            return VerifySignature(new FileInfo(filePath), signature);
        }

        public bool VerifySignature(FileInfo file, string signature)
        {
            if (!KeysExist())
            {
                Console.WriteLine("Keys do not exist");
                return false;
            }
            if (signature == null)
            {
                Console.WriteLine("Signature at path {0} is null", file.FullName);
                return false;
            }

            // code for reading stream in chunks modified from https://stackoverflow.com/a/7542077/3938401
            byte[] bHash = Convert.FromBase64String(signature);
            const int chunkSize = 1024 * 1024 * 25;
            using FileStream inputStream = File.OpenRead(file.FullName);
            var validator = new Ed25519Signer();
            validator.InitWithNonExpandedPrivateKey(GetPublicKey(), GetPrivateKey());
            // read file in chunks
            byte[] buffer = new byte[chunkSize]; // read in chunks of 25 MB
            int bytesRead;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                validator.AddToBuffer(buffer, 0, bytesRead);
            }
            return validator.VerifySignature(Convert.FromBase64String(signature));
        }

        public string GetSignatureForFile(string filePath)
        {
            return GetSignatureForFile(new FileInfo(filePath));
        }

        public string GetSignatureForFile(FileInfo file)
        {
            if (!KeysExist())
            {
                Console.WriteLine("Keys do not exist");
                return null;
            }

            if (!file.Exists)
            {
                Console.Error.WriteLine("Target binary " + file.FullName + " does not exist");
                return null;
            }


            using FileStream inputStream = File.OpenRead(file.FullName);
            var validator = new Ed25519Signer();
            validator.InitWithNonExpandedPrivateKey(GetPublicKey(), GetPrivateKey());
            // read file in chunks
            const int chunkSize = 1024 * 1024 * 25;
            byte[] buffer = new byte[chunkSize]; // read in chunks of 25 MB
            int bytesRead;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                validator.AddToBuffer(buffer, 0, bytesRead);
            }
            return Convert.ToBase64String(validator.GenerateSignature());
        }

        public string GetSignatureForData(byte[] data)
        { 
            var signer = new Ed25519Signer();
            signer.InitWithNonExpandedPrivateKey(GetPublicKey(), GetPrivateKey());
            signer.AddToBuffer(data, 0, data.Length);
            return Convert.ToBase64String(signer.GenerateSignature());
        }

        public byte[] GetPrivateKey()
        {
            if (!string.IsNullOrWhiteSpace(_privateKeyOverride))
            {
                return Convert.FromBase64String(_privateKeyOverride);
            }
            return ResolveKeyLocation(PrivateKeyEnvironmentVariable, _privateKeyFilePath);
        }

        public byte[] GetPublicKey()
        {
            if (!string.IsNullOrWhiteSpace(_publicKeyOverride))
            {
                return Convert.FromBase64String(_publicKeyOverride);
            }
            return ResolveKeyLocation(PublicKeyEnvironmentVariable, _publicKeyFilePath);
        }

        private byte[] ResolveKeyLocation(string environmentVariableName, string fileLocation)
        {
            var key = Environment.GetEnvironmentVariable(environmentVariableName);

            if (key != null)
            {
                return Convert.FromBase64String(key);
            }

            if (!File.Exists(fileLocation))
            {
                return null;
            }

            return Convert.FromBase64String(File.ReadAllText(fileLocation));
        }

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
