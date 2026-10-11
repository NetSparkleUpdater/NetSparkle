using NetSparkleUpdater.AppCastGenerator;
using System;
using Xunit;

namespace NetSparkle.Tests.AppCastGenerator
{
    public class EnvironmentKeysProviderTests : IDisposable
    {
        private readonly EnvironmentKeysProvider _provider;
        private readonly string? _originalPrivateKey;
        private readonly string? _originalPublicKey;

        public EnvironmentKeysProviderTests()
        {
            _provider = new EnvironmentKeysProvider();

            // Save original environment variables to restore them after tests
            _originalPrivateKey = Environment.GetEnvironmentVariable(
                EnvironmentKeysProvider.PrivateKeyEnvironmentVariable);
            _originalPublicKey = Environment.GetEnvironmentVariable(
                EnvironmentKeysProvider.PublicKeyEnvironmentVariable);
        }

        [Fact]
        public void GetPrivateKey_ReturnsNullWhenEnvironmentVariableNotSet()
        {
            Environment.SetEnvironmentVariable(
                EnvironmentKeysProvider.PrivateKeyEnvironmentVariable,
                null);

            var key = _provider.GetPrivateKey();

            Assert.Null(key);
        }

        [Fact]
        public void GetPublicKey_ReturnsNullWhenEnvironmentVariableNotSet()
        {
            Environment.SetEnvironmentVariable(
                EnvironmentKeysProvider.PublicKeyEnvironmentVariable,
                null);

            var key = _provider.GetPublicKey();

            Assert.Null(key);
        }

        [Fact]
        public void GetPrivateKey_ReturnsDecodedKeyWhenEnvironmentVariableIsSet()
        {
            var keyContent = new byte[] { 1, 2, 3, 4, 5 };
            var base64Key = Convert.ToBase64String(keyContent);

            Environment.SetEnvironmentVariable(
                EnvironmentKeysProvider.PrivateKeyEnvironmentVariable,
                base64Key);

            var retrievedKey = _provider.GetPrivateKey();

            Assert.NotNull(retrievedKey);
            Assert.Equal(keyContent, retrievedKey);
        }

        [Fact]
        public void GetPublicKey_ReturnsDecodedKeyWhenEnvironmentVariableIsSet()
        {
            var keyContent = new byte[] { 10, 20, 30, 40, 50 };
            var base64Key = Convert.ToBase64String(keyContent);

            Environment.SetEnvironmentVariable(
                EnvironmentKeysProvider.PublicKeyEnvironmentVariable,
                base64Key);

            var retrievedKey = _provider.GetPublicKey();

            Assert.NotNull(retrievedKey);
            Assert.Equal(keyContent, retrievedKey);
        }

        [Fact]
        public void GetPrivateKey_ThrowsWhenEnvironmentVariableContainsInvalidBase64()
        {
            Environment.SetEnvironmentVariable(
                EnvironmentKeysProvider.PrivateKeyEnvironmentVariable,
                "not-a-valid-base64-string!");

            Assert.Throws<FormatException>(_provider.GetPrivateKey);
        }

        [Fact]
        public void GetPublicKey_ThrowsWhenEnvironmentVariableContainsInvalidBase64()
        {
            Environment.SetEnvironmentVariable(
                EnvironmentKeysProvider.PublicKeyEnvironmentVariable,
                "not-a-valid-base64-string!");

            Assert.Throws<FormatException>(_provider.GetPublicKey);
        }

        [Fact]
        public void BothKeys_CanBeSetAndRetrievedIndependently()
        {
            var privateKeyContent = new byte[] { 1, 2, 3 };
            var publicKeyContent = new byte[] { 4, 5, 6 };

            var privateBase64 = Convert.ToBase64String(privateKeyContent);
            var publicBase64 = Convert.ToBase64String(publicKeyContent);

            Environment.SetEnvironmentVariable(
                EnvironmentKeysProvider.PrivateKeyEnvironmentVariable,
                privateBase64);
            Environment.SetEnvironmentVariable(
                EnvironmentKeysProvider.PublicKeyEnvironmentVariable,
                publicBase64);

            var retrievedPrivate = _provider.GetPrivateKey();
            var retrievedPublic = _provider.GetPublicKey();

            Assert.NotNull(retrievedPrivate);
            Assert.Equal(privateKeyContent, retrievedPrivate);
            Assert.NotNull(retrievedPublic);
            Assert.Equal(publicKeyContent, retrievedPublic);
        }

        [Theory]
        [InlineData("SPARKLE_PRIVATE_KEY", EnvironmentKeysProvider.PrivateKeyEnvironmentVariable)]
        [InlineData("SPARKLE_PUBLIC_KEY", EnvironmentKeysProvider.PublicKeyEnvironmentVariable)]
        public void EnvironmentVariable_HasExpectedValue(string expectedValue, string actualValue)
        {
            Assert.Equal(expectedValue, actualValue);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Restore original environment variables
                if (_originalPrivateKey != null)
                {
                    Environment.SetEnvironmentVariable(
                        EnvironmentKeysProvider.PrivateKeyEnvironmentVariable,
                        _originalPrivateKey);
                }
                else
                {
                    Environment.SetEnvironmentVariable(
                        EnvironmentKeysProvider.PrivateKeyEnvironmentVariable,
                        null);
                }

                if (_originalPublicKey != null)
                {
                    Environment.SetEnvironmentVariable(
                        EnvironmentKeysProvider.PublicKeyEnvironmentVariable,
                        _originalPublicKey);
                }
                else
                {
                    Environment.SetEnvironmentVariable(
                        EnvironmentKeysProvider.PublicKeyEnvironmentVariable,
                        null);
                }
            }
        }
    }
}
