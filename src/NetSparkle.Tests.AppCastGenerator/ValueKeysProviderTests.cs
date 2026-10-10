using NetSparkleUpdater.AppCastGenerator;
using System;
using Xunit;

namespace NetSparkle.Tests.AppCastGenerator
{
    public class ValueKeysProviderTests
    {
        [Fact]
        public void GetPrivateKey_ReturnsNullWhenNotSet()
        {
            var provider = new ValueKeysProvider();

            var key = provider.GetPrivateKey();

            Assert.Null(key);
        }

        [Fact]
        public void GetPublicKey_ReturnsNullWhenNotSet()
        {
            var provider = new ValueKeysProvider();

            var key = provider.GetPublicKey();

            Assert.Null(key);
        }

        [Fact]
        public void SetPrivateKey_ThenGetPrivateKey_ReturnsDecodedValue()
        {
            var provider = new ValueKeysProvider();
            var keyContent = new byte[] { 1, 2, 3, 4, 5 };
            var base64Key = Convert.ToBase64String(keyContent);

            provider.SetPrivateKey(base64Key);
            var retrievedKey = provider.GetPrivateKey();

            Assert.NotNull(retrievedKey);
            Assert.Equal(keyContent, retrievedKey);
        }

        [Fact]
        public void SetPublicKey_ThenGetPublicKey_ReturnsDecodedValue()
        {
            var provider = new ValueKeysProvider();
            var keyContent = new byte[] { 10, 20, 30, 40, 50 };
            var base64Key = Convert.ToBase64String(keyContent);

            provider.SetPublicKey(base64Key);
            var retrievedKey = provider.GetPublicKey();

            Assert.NotNull(retrievedKey);
            Assert.Equal(keyContent, retrievedKey);
        }

        [Fact]
        public void SetPrivateKey_WithInvalidBase64_ThrowsWhenDecoding()
        {
            var provider = new ValueKeysProvider();
            provider.SetPrivateKey("not-a-valid-base64-string!");

            Assert.Throws<FormatException>(provider.GetPrivateKey);
        }

        [Fact]
        public void SetPublicKey_WithInvalidBase64_ThrowsWhenDecoding()
        {
            var provider = new ValueKeysProvider();
            provider.SetPublicKey("not-a-valid-base64-string!");

            Assert.Throws<FormatException>(provider.GetPublicKey);
        }

        [Fact]
        public void BothKeys_CanBeSetAndRetrievedIndependently()
        {
            var provider = new ValueKeysProvider();
            var privateKeyContent = new byte[] { 1, 2, 3 };
            var publicKeyContent = new byte[] { 4, 5, 6 };

            provider.SetPrivateKey(Convert.ToBase64String(privateKeyContent));
            provider.SetPublicKey(Convert.ToBase64String(publicKeyContent));

            var retrievedPrivate = provider.GetPrivateKey();
            var retrievedPublic = provider.GetPublicKey();

            Assert.Equal(privateKeyContent, retrievedPrivate);
            Assert.Equal(publicKeyContent, retrievedPublic);
        }
    }
}
