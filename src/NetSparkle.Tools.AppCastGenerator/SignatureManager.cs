using System;
using System.IO;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;

namespace NetSparkleUpdater.AppCastGenerator
{
    public class SignatureManager
    {
        private string _storage;
        private string _privateKey;
        private string _publicKey;

        private const string _privateKeyEnvironmentVariable = "SPARKLE_PRIVATE_KEY";
        private const string _publicKeyEnvironmentVariable = "SPARKLE_PUBLIC_KEY";

        public SignatureManager()
        {
            _storage = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "netsparkle");

            if (!Directory.Exists(_storage))
            {
                Directory.CreateDirectory(_storage);
            }

            _privateKey = Path.Combine(_storage, "NetSparkle_Ed25519.priv");
            _publicKey = Path.Combine(_storage, "NetSparkle_Ed25519.pub");
        }

        public bool KeysExist()
        {
            if (GetPublicKey() != null && GetPrivateKey() != null)
            {
                return true;
            }

            return false;
        }

        public void Generate(bool force = false)
        {

            if (KeysExist() && !force)
            {
                Console.WriteLine("Keys already exist, use --force to force regeneration");
                Environment.Exit(1);
            }

            // start key generation
            Console.WriteLine("Generating key pair...");


            var Random = new SecureRandom();

            Ed25519KeyPairGenerator kpg = new Ed25519KeyPairGenerator();
            kpg.Init(new Ed25519KeyGenerationParameters(Random));

            AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();
            Ed25519PrivateKeyParameters privateKey = (Ed25519PrivateKeyParameters)kp.Private;
            Ed25519PublicKeyParameters publicKey = (Ed25519PublicKeyParameters)kp.Public;

            var privKey = privateKey.GetEncoded();
            var pubKey = publicKey.GetEncoded();


            File.WriteAllBytes(_privateKey, privKey);
            File.WriteAllBytes(_publicKey, pubKey);

            Console.WriteLine("Storing public/private keys to " + _storage);
        }

        public bool VerifySignature(string file, string signature)
        {
            return VerifySignature(new FileInfo(file), signature);
        }

        public bool VerifySignature(FileInfo file, string signature)
        {
            if (!KeysExist())
            {
                Console.WriteLine("Keys do not exist");
                Environment.Exit(1);
            }

            
            var data = File.ReadAllBytes(file.FullName);

            var validator = new Ed25519Signer();
            validator.Init(false, new Ed25519PublicKeyParameters(GetPublicKey(), 0));
            validator.BlockUpdate(data, 0, data.Length);

            return validator.VerifySignature(Convert.FromBase64String(signature));
        }

        public string GetSignature(string file)
        {
            return GetSignature(new FileInfo(file));
        }

        public string GetSignature(FileInfo file)
        {
            if (!KeysExist())
            {
                Console.WriteLine("Keys do not exist");
                Environment.Exit(1);
            }

            if (!file.Exists)
            {
                Console.Error.WriteLine("Target binary " + file.FullName + " does not exists");
                Environment.Exit(1);
            }


            var data = File.ReadAllBytes(file.FullName);

            var signer = new Ed25519Signer();

            signer.Init(true, new Ed25519PrivateKeyParameters(GetPrivateKey(), 0));
            signer.BlockUpdate(data, 0, data.Length);

            return Convert.ToBase64String(signer.GenerateSignature());
        }

        public byte[] GetPrivateKey()
        {
            return ResolveKeyLocation(_privateKeyEnvironmentVariable, _privateKey);
        }

        public byte[] GetPublicKey()
        {
            return ResolveKeyLocation(_publicKeyEnvironmentVariable, _publicKey);
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

            return File.ReadAllBytes(fileLocation);
        }
    }
}
