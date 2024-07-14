using NetSparkleUpdater.SignatureVerifiers;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace NetSparkleUnitTests
{
    public class Ed25519Tests
    {
        private static readonly SecureRandom Random = new SecureRandom();

        [Fact]
        public void CanValidateBouncyCastleSignatureMadeViaKeysFromBouncyCastle()
        {
            Ed25519KeyPairGenerator kpg = new Ed25519KeyPairGenerator();
            kpg.Init(new Ed25519KeyGenerationParameters(Random));

            AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();
            Ed25519PrivateKeyParameters privateKey = (Ed25519PrivateKeyParameters)kp.Private;
            Ed25519PublicKeyParameters publicKey = (Ed25519PublicKeyParameters)kp.Public;
            var pubKeyBase64 = Convert.ToBase64String(publicKey.GetEncoded());
            // create signature for item
            byte[] msg = new byte[Random.NextInt() & 255];
            Random.NextBytes(msg);
            var signer = new Org.BouncyCastle.Crypto.Signers.Ed25519Signer();
            signer.Init(true, privateKey);
            signer.BlockUpdate(msg, 0, msg.Length);
            byte[] signature = signer.GenerateSignature();
            var signatureForAppCast = Convert.ToBase64String(signature);

            // verify signature
            var checker = new Ed25519Checker(NetSparkleUpdater.Enums.SecurityMode.Strict, pubKeyBase64);
            Assert.True(checker.VerifySignature(signatureForAppCast, msg) == NetSparkleUpdater.Enums.ValidationResult.Valid);
        }

        [Fact]
        public void CanValidateSignatureMadeViaKeysFromChaosNaCl()
        {
            var seed = new byte[32];
            Random.NextBytes(seed);
            Chaos.NaCl.Ed25519.KeyPairFromSeed(out byte[] publicKey, out byte[] privateKey, seed);

            var pubKeyBase64 = Convert.ToBase64String(publicKey);
            // create signature for item
            byte[] msg = new byte[Random.NextInt() & 255];
            Random.NextBytes(msg);
            var signer = new Chaos.NaCl.Ed25519Signer();
            signer.Init(null, privateKey);
            signer.AddToBuffer(msg, 0, msg.Length);
            byte[] signature = signer.GenerateSignature();
            var signatureForAppCast = Convert.ToBase64String(signature);

            // verify signature
            var checker = new Ed25519Checker(NetSparkleUpdater.Enums.SecurityMode.Strict, pubKeyBase64);
            Assert.True(checker.VerifySignature(signatureForAppCast, msg) == NetSparkleUpdater.Enums.ValidationResult.Valid);
        }

        [Fact]
        public void CanValidateSignatureFromChaosNaClFromBouncyCastleKeys()
        {
            Ed25519KeyPairGenerator kpg = new Ed25519KeyPairGenerator();
            kpg.Init(new Ed25519KeyGenerationParameters(Random));

            AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();
            Ed25519PrivateKeyParameters privateKey = (Ed25519PrivateKeyParameters)kp.Private;
            Ed25519PublicKeyParameters publicKey = (Ed25519PublicKeyParameters)kp.Public;
            var pubKeyBase64 = Convert.ToBase64String(publicKey.GetEncoded());
            // create signature for item
            byte[] msg = new byte[Random.NextInt() & 255];
            Random.NextBytes(msg);
            // internally, BouncyCastle is using the seed as its private key. This matches the Chaos.NaCl
            // documentation that the 32 bit private key is "A 32 byte seeds which allow arbitrary values.
            // This is the form that should be generated and stored." and the expanded one is
            // "A 64 byte expanded form. This form is used internally to improve performance"
            // So BouncyCastle's private key that is saved on disk is "just" the seed.
            // (See https://github.com/bcgit/bc-csharp/blob/master/crypto/src/crypto/generators/Ed25519KeyPairGenerator.cs)
            // See also notes on https://github.com/orlp/ed25519 and https://github.com/johndbritton/teleport/pull/84
            // ("In newer implementations of the algorithm, the seed is saved and the keys are derived from the seed each time they are needed.")
            // The expanded key basically is the seed and the public key, it looks like, based on crypto_sign2 and
            // the reference implementation of https://github.com/orlp/ed25519/blob/master/src/sign.c (the latter takes a public key
            // in its signing algorithm and the code in Chaos.NaCl is similar if not identical, therefore the public key is also
            // used in Chaos.NaCl. ...yes, this isn't scientific necessarily, but it saves us from reading the implementation algorithm itself.
            var expandedPrivatekey = Chaos.NaCl.Ed25519.ExpandedPrivateKeyFromSeed(privateKey.GetEncoded());
            var signer = new Chaos.NaCl.Ed25519Signer();
            signer.Init(null, expandedPrivatekey);
            signer.AddToBuffer(msg, 0, msg.Length);
            byte[] signature = signer.GenerateSignature();
            var signatureForAppCast = Convert.ToBase64String(signature);

            // verify signature
            var checker = new Ed25519Checker(NetSparkleUpdater.Enums.SecurityMode.Strict, pubKeyBase64);
            Assert.True(checker.VerifySignature(signatureForAppCast, msg) == NetSparkleUpdater.Enums.ValidationResult.Valid);
        }

        [Fact]
        public void ValidateFileSignature()
        {
            Ed25519KeyPairGenerator kpg = new Ed25519KeyPairGenerator();
            kpg.Init(new Ed25519KeyGenerationParameters(Random));

            AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();
            Ed25519PrivateKeyParameters privateKey = (Ed25519PrivateKeyParameters)kp.Private;
            Ed25519PublicKeyParameters publicKey = (Ed25519PublicKeyParameters)kp.Public;
            var pubKeyBase64 = Convert.ToBase64String(publicKey.GetEncoded());
            // create signature for item
            byte[] msg = new byte[1024*1024*50];
            Random.NextBytes(msg);
            var signer = new Org.BouncyCastle.Crypto.Signers.Ed25519Signer();
            signer.Init(true, privateKey);
            signer.BlockUpdate(msg, 0, msg.Length);
            byte[] signature = signer.GenerateSignature();
            var signatureForAppCast = Convert.ToBase64String(signature);
            // actually check signature
            var tmpFile = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllBytes(tmpFile, msg);
            // verify signature if not reading in chunks
            var checker = new Ed25519Checker(NetSparkleUpdater.Enums.SecurityMode.Strict, pubKeyBase64);
            Assert.True(checker.VerifySignatureOfFile(signatureForAppCast, tmpFile) == NetSparkleUpdater.Enums.ValidationResult.Valid);
            // verify signature if reading in chunks
            checker = new Ed25519Checker(NetSparkleUpdater.Enums.SecurityMode.Strict, pubKeyBase64, readFileBeingVerifiedInChunks: true, chunkSize: 1024 * 1024 * 10);
            Assert.True(checker.VerifySignatureOfFile(signatureForAppCast, tmpFile) == NetSparkleUpdater.Enums.ValidationResult.Valid);
        }
    }
}
