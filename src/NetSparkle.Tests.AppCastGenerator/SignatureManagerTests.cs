using NetSparkleUpdater.AppCastGenerator;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace NetSparkle.Tests.AppCastGenerator
{
    [Collection(SignatureManagerFixture.CollectionName)]
    public class SignatureManagerTests
    {
        private SignatureManagerFixture _fixture;

        public SignatureManagerTests(SignatureManagerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestKeysExist()
        {
            var manager = _fixture.GetSignatureManager();
            Assert.True(manager.KeysExist());
        }

        [Fact]
        public void CanGenerateKeys()
        {
            var manager = _fixture.GetSignatureManager();

            var publicKey = manager.GetPublicKey();
            Assert.NotNull(publicKey);
            Assert.NotEmpty(publicKey);
            var privateKey = manager.GetPrivateKey();
            Assert.NotNull(privateKey);
            Assert.NotEmpty(privateKey);
        }

        // https://stackoverflow.com/a/1344242/3938401
        private static string RandomString(int length)
        {
            Random random = new SecureRandom();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [Fact]
        public void CanGetAndVerifySignature()
        {
            // create tmp file 
            var tempData = RandomString(1024);
            var path = Path.GetTempFileName();
            File.WriteAllText(path, tempData);
            Assert.True(File.Exists(path));
            Assert.Equal(tempData, File.ReadAllText(path));
            // get signature of file
            var manager = _fixture.GetSignatureManager();
            var signature = manager.GetSignatureForFile(path);
            // verify signature
            Assert.True(manager.VerifySignature(path, signature));
            // get rid of temp file
            File.Delete(path);
        }

        [Fact]
        public void CanGetAndVerifySignatureWithOverride()
        {
            // create tmp file 
            var tempData = RandomString(1024);
            var path = Path.GetTempFileName();
            File.WriteAllText(path, tempData);
            Assert.True(File.Exists(path));
            Assert.Equal(tempData, File.ReadAllText(path));
            // get signature of file
            var manager = _fixture.GetSignatureManager();
            var signature = manager.GetSignatureForFile(path);
            var realPublicKey = manager.GetPublicKey();
            var realPrivateKey = manager.GetPrivateKey();
            // intentionally mess up keys - by regenerating them in a new directory, we use a separate manager for this to preserve the keys
            SignatureManager newManager = _fixture.CreateSignatureManager("netsparkle-tests-wrong");
            try
            {
                // verify signature does not work
                Assert.False(newManager.VerifySignature(path, signature));
                // override and verify that it does work
                newManager.SetPublicKeyOverride(Convert.ToBase64String(realPublicKey));
                newManager.SetPrivateKeyOverride(Convert.ToBase64String(realPrivateKey));
                Assert.True(newManager.VerifySignature(path, signature));
            }
            finally
            {
                // clean up created signature manager even if test fails
                // get rid of temp file
                File.Delete(path);
                _fixture.CleanupSignatureManager(newManager);
            }
        }

        [Fact]
        public void CanGetAndVerifySignatureFromEnvironment()
        {
            // create tmp file 
            var tempData = RandomString(1024);
            var path = Path.GetTempFileName();
            File.WriteAllText(path, tempData);
            Assert.True(File.Exists(path));
            Assert.Equal(tempData, File.ReadAllText(path));

            // create keys
            var Random = new SecureRandom();

            Ed25519KeyPairGenerator kpg = new Ed25519KeyPairGenerator();
            kpg.Init(new Ed25519KeyGenerationParameters(Random));

            AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();
            Ed25519PrivateKeyParameters privateKey = (Ed25519PrivateKeyParameters)kp.Private;
            Ed25519PublicKeyParameters publicKey = (Ed25519PublicKeyParameters)kp.Public;

            var privKeyBase64 = Convert.ToBase64String(privateKey.GetEncoded());
            var pubKeyBase64 = Convert.ToBase64String(publicKey.GetEncoded());

            var manager = _fixture.GetSignatureManager();
            try
            {
                Environment.SetEnvironmentVariable(SignatureManager.PrivateKeyEnvironmentVariable, privKeyBase64);
                Environment.SetEnvironmentVariable(SignatureManager.PublicKeyEnvironmentVariable, pubKeyBase64);
                // get signature of file
                var signature = manager.GetSignatureForFile(path);
                manager.Generate(true); // force regeneration of keys to "prove" that we are using environment 
                // verify signature
                Assert.True(manager.VerifySignature(path, signature));
            }
            finally
            {
                // get rid of temp file
                File.Delete(path);
                // ensure environment is cleaned up, even if test fails
                Environment.SetEnvironmentVariable(SignatureManager.PrivateKeyEnvironmentVariable, null);
                Environment.SetEnvironmentVariable(SignatureManager.PublicKeyEnvironmentVariable, null);
            }
        }
    }
}
