using NetSparkleUpdater.SignatureVerifiers;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NetSparkleUnitTests
{
    public class Ed25519Tests
    {
        private static readonly SecureRandom Random = new SecureRandom();

        [Fact]
        public void CanValidateSignature()
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
            var signer = new Ed25519Signer();
            signer.Init(true, privateKey);
            signer.BlockUpdate(msg, 0, msg.Length);
            byte[] signature = signer.GenerateSignature();
            var signatureForAppCast = Convert.ToBase64String(signature);

            // verify signature
            var checker = new Ed25519Checker(NetSparkleUpdater.Enums.SecurityMode.Strict, pubKeyBase64);
            Assert.True(checker.VerifySignature(signatureForAppCast, msg) == NetSparkleUpdater.Enums.ValidationResult.Valid);
        }
    }
}
