using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;

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
            SetStorageDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "netsparkle"));
            _privateKeyOverride = "";
            _publicKeyOverride = "";
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


            var Random = new SecureRandom();

            Ed25519KeyPairGenerator kpg = new Ed25519KeyPairGenerator();
            kpg.Init(new Ed25519KeyGenerationParameters(Random));

            AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();
            Ed25519PrivateKeyParameters privateKey = (Ed25519PrivateKeyParameters)kp.Private;
            Ed25519PublicKeyParameters publicKey = (Ed25519PublicKeyParameters)kp.Public;

            var privKeyBase64 = Convert.ToBase64String(privateKey.GetEncoded());
            var pubKeyBase64 = Convert.ToBase64String(publicKey.GetEncoded());

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

            
            var data = File.ReadAllBytes(file.FullName);

            var validator = new Ed25519Signer();
            validator.Init(false, new Ed25519PublicKeyParameters(GetPublicKey(), 0));
            validator.BlockUpdate(data, 0, data.Length);

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
                Console.Error.WriteLine("Target binary " + file.FullName + " does not exists");
                return null;
            }


            var data = File.ReadAllBytes(file.FullName);

            var signer = new Ed25519Signer();

            signer.Init(true, new Ed25519PrivateKeyParameters(GetPrivateKey(), 0));
            signer.BlockUpdate(data, 0, data.Length);

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
    }
}
