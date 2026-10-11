using Chaos.NaCl;
using System;
using System.IO;
using System.Security.Cryptography;
using Console = Colorful.Console;

namespace NetSparkleUpdater.AppCastGenerator
{
    public class SignatureManager
    {
        public KeysProvider KeysProvider { get; }

        public SignatureManager(KeysProvider keysProvider)
        {
            KeysProvider = keysProvider;
        }

        public bool Generate(bool force = false)
        {
            if (KeysProvider.KeysExist() && !force)
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

            FileKeysProvider fileKeysProvider = KeysProvider.GetRequiredProvider<FileKeysProvider>();

            fileKeysProvider.SetPrivateKey(privKeyBase64);
            fileKeysProvider.SetPublicKey(pubKeyBase64);

            Console.WriteLine("Storing public/private keys to " + fileKeysProvider.GetStorageDirectory());
            return true;
        }

        public bool VerifySignature(string filePath, string signature)
        {
            return VerifySignature(new FileInfo(filePath), signature);
        }

        public bool VerifySignature(FileInfo file, string signature)
        {
            if (!KeysProvider.KeysExist())
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
            validator.InitWithNonExpandedPrivateKey(KeysProvider.GetPublicKey(), KeysProvider.GetPrivateKey());
            // read file in chunks
            byte[] buffer = new byte[chunkSize]; // read in chunks of 25 MB
            int bytesRead;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                validator.AddToBuffer(buffer, 0, bytesRead);
            }
            return validator.VerifySignature(Convert.FromBase64String(signature));
        }

        public string? GetSignatureForFile(string filePath)
        {
            return GetSignatureForFile(new FileInfo(filePath));
        }

        public string? GetSignatureForFile(FileInfo file)
        {
            if (!KeysProvider.KeysExist())
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
            validator.InitWithNonExpandedPrivateKey(KeysProvider.GetPublicKey(), KeysProvider.GetPrivateKey());
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
            signer.InitWithNonExpandedPrivateKey(KeysProvider.GetPublicKey(), KeysProvider.GetPrivateKey());
            signer.AddToBuffer(data, 0, data.Length);
            return Convert.ToBase64String(signer.GenerateSignature());
        }
    }
}
