using NetSparkleUpdater.AppCastGenerator;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace NetSparkle.Tests.AppCastGenerator
{
    public class SignatureManagerTests
    {
        [Fact]
        public void TestKeysExist()
        {
            var manager = new SignatureManager();
            manager.Generate(true);
            Assert.True(manager.KeysExist());
        }

        [Fact]
        public void CanGenerateKeys()
        {
            var manager = new SignatureManager();
            manager.Generate(true);

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
            var manager = new SignatureManager();
            manager.Generate(true);
            var signature = manager.GetSignatureForFile(path);
            // verify signature
            Assert.True(manager.VerifySignature(path, signature));
        }
    }
}
