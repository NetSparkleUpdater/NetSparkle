using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NetSparkleUpdater.Enums;
using NSec.Cryptography;

namespace NetSparkleUpdater.AppCastGenerator
{
    public class SignatureManager
    {
        private string _storage;
        private string _privateKey;
        private string _publicKey;

        private KeyBlobFormat _privateKeyFormat = KeyBlobFormat.PkixPrivateKeyText;
        private KeyBlobFormat _publicKeyFormat = KeyBlobFormat.PkixPublicKeyText;
        private SignatureAlgorithm _algorithm = SignatureAlgorithm.Ed25519;

        public SignatureManager()
        {
            _storage = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "netsparkle");

            if(!Directory.Exists(_storage))
            {
                Directory.CreateDirectory(_storage);
            }

            _privateKey = Path.Combine(_storage, "NetSparkle_DSA.priv");
            _publicKey = Path.Combine(_storage, "NetSparkle_DSA.pub");
        }

        public bool KeysExist()
        {
            if (File.Exists(_privateKey) || File.Exists(_publicKey))
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

            var algorithm = SignatureAlgorithm.Ed25519;

            using var key = Key.Create(algorithm, new KeyCreationParameters
            {
                ExportPolicy = KeyExportPolicies.AllowPlaintextArchiving
            });


            File.WriteAllBytes(_privateKey, key.Export(_privateKeyFormat));
            File.WriteAllBytes(_publicKey, key.PublicKey.Export(_publicKeyFormat));

            Console.WriteLine("Storing public/private keys to " + _storage);

            /*
             * 
            // DSACryptoServiceProvider is a thin windows only wrapper. Can't generate keys on linux / mac
            DSACryptoServiceProvider prv = new DSACryptoServiceProvider();
            File.WriteAllText(_privateKey, prv.ToXmlString(true));
            File.WriteAllText(_publicKey, prv.ToXmlString(false));
            */
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

            var key = ImportKey();
            var data = File.ReadAllBytes(file.FullName);
            var signatureAsBytes = Convert.FromBase64String(signature);

            // verify the data using the signature and the public key
            if (_algorithm.Verify(key.PublicKey, data, signatureAsBytes))
            {
                return true;
            }

            return false;
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

            var key = ImportKey();
            var data = File.ReadAllBytes(file.FullName);
            var signature = _algorithm.Sign(key, data);

            return Convert.ToBase64String(signature);
        }

        private Key ImportKey()
        {
            var blob = File.ReadAllBytes(_privateKey);
            var key = Key.Import(_algorithm, blob, _privateKeyFormat);
            return key;
        }
    }
}
