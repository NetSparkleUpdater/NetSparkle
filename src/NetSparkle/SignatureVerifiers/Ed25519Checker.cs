using System;
using System.IO;
using System.Text;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;
using NSec.Cryptography;

namespace NetSparkleUpdater.SignatureVerifiers
{
    public class Ed25519Checker : ISignatureVerifier
    {
        private readonly Key _key;
        private readonly Ed25519 _algorithm = SignatureAlgorithm.Ed25519;

        public SecurityMode SecurityMode { get; set; }

        public Ed25519Checker(SecurityMode mode, string publicKey)
        {
            var blob = Encoding.ASCII.GetBytes(publicKey);

            SecurityMode = mode;

            _key = Key.Import(_algorithm, blob, KeyBlobFormat.PkixPublicKeyText);
        }

        public Ed25519Checker(SecurityMode mode, FileInfo publicKey)
        {
            if(!publicKey.Exists)
            {
                throw new FileNotFoundException(publicKey.FullName);
            }

            SecurityMode = mode;

            var blob = File.ReadAllBytes(publicKey.FullName);

            _key = Key.Import(_algorithm, blob, KeyBlobFormat.PkixPublicKeyText);
        }

        public bool HasValidKeyInformation()
        {
            return _key.HasPublicKey;
        }

        public ValidationResult VerifySignature(string signature, byte[] dataToVerify)
        {
            var signatureAsBytes = Convert.FromBase64String(signature);

            if (_algorithm.Verify(_key.PublicKey, dataToVerify, signatureAsBytes))
            {
                return ValidationResult.Valid;
            }

            return ValidationResult.Invalid;
        }

        public ValidationResult VerifySignatureOfFile(string signature, string binaryPath)
        {
            var data = File.ReadAllBytes(binaryPath);
            var signatureAsBytes = Convert.FromBase64String(signature);

            if (_algorithm.Verify(_key.PublicKey, data, signatureAsBytes))
            {
                return ValidationResult.Valid;
            }

            return ValidationResult.Invalid;
        }

        public ValidationResult VerifySignatureOfString(string signature, string data)
        {
            var signatureAsBytes = Convert.FromBase64String(signature);

            if (_algorithm.Verify(_key.PublicKey, signatureAsBytes, signatureAsBytes))
            {
                return ValidationResult.Valid;
            }

            return ValidationResult.Invalid;
        }
    }
}
