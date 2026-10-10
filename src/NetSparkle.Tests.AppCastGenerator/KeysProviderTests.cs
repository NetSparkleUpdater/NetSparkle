using NetSparkleUpdater.AppCastGenerator;
using Moq;
using System;
using Xunit;

namespace NetSparkle.Tests.AppCastGenerator
{
    public class KeysProviderTests
    {
        [Fact]
        public void GetRequiredProvider_ReturnsProviderOfRequestedType()
        {
            IKeysProvider provider1 = new ValueKeysProvider();
            IKeysProvider provider2 = new EnvironmentKeysProvider();
            var keysProvider = new KeysProvider([provider1, provider2]);

            var retrieved = keysProvider.GetRequiredProvider<ValueKeysProvider>();

            Assert.Same(provider1, retrieved);
        }

        [Fact]
        public void GetRequiredProvider_ThrowsWhenProviderNotFound()
        {
            IKeysProvider provider1 = new ValueKeysProvider();
            var keysProvider = new KeysProvider([provider1]);

            Assert.Throws<InvalidOperationException>(keysProvider.GetRequiredProvider<EnvironmentKeysProvider>);
        }

        [Fact]
        public void KeysExist_ReturnsTrueWhenBothKeysPresent()
        {
            var privateKey = new byte[] { 1, 2, 3 };
            var publicKey = new byte[] { 4, 5, 6 };

            var mockProvider = new Mock<IKeysProvider>();
            mockProvider.Setup(p => p.GetPrivateKey()).Returns(privateKey);
            mockProvider.Setup(p => p.GetPublicKey()).Returns(publicKey);

            var keysProvider = new KeysProvider([mockProvider.Object]);
            Assert.True(keysProvider.KeysExist());
        }

        [Fact]
        public void KeysExist_ReturnsFalseWhenPrivateKeyMissing()
        {
            var publicKey = new byte[] { 4, 5, 6 };

            var mockProvider = new Mock<IKeysProvider>();
            mockProvider.Setup(p => p.GetPrivateKey()).Returns((byte[])null);
            mockProvider.Setup(p => p.GetPublicKey()).Returns(publicKey);

            var keysProvider = new KeysProvider([mockProvider.Object]);
            Assert.False(keysProvider.KeysExist());
        }

        [Fact]
        public void KeysExist_ReturnsFalseWhenPublicKeyMissing()
        {
            var privateKey = new byte[] { 1, 2, 3 };

            var mockProvider = new Mock<IKeysProvider>();
            mockProvider.Setup(p => p.GetPrivateKey()).Returns(privateKey);
            mockProvider.Setup(p => p.GetPublicKey()).Returns((byte[])null);

            var keysProvider = new KeysProvider([mockProvider.Object]);
            Assert.False(keysProvider.KeysExist());
        }

        [Fact]
        public void GetPrivateKey_ReturnsFirstAvailablePrivateKey()
        {
            var key1 = new byte[] { 1, 2, 3 };
            var key2 = new byte[] { 4, 5, 6 };

            var mockProvider1 = new Mock<IKeysProvider>();
            mockProvider1.Setup(p => p.GetPrivateKey()).Returns((byte[])null);

            var mockProvider2 = new Mock<IKeysProvider>();
            mockProvider2.Setup(p => p.GetPrivateKey()).Returns(key1);

            var mockProvider3 = new Mock<IKeysProvider>();
            mockProvider3.Setup(p => p.GetPrivateKey()).Returns(key2);

            var keysProvider = new KeysProvider([mockProvider1.Object, mockProvider2.Object, mockProvider3.Object]);
            var retrieved = keysProvider.GetPrivateKey();

            Assert.Equal(key1, retrieved);
        }

        [Fact]
        public void GetPublicKey_ReturnsFirstAvailablePublicKey()
        {
            var key1 = new byte[] { 1, 2, 3 };
            var key2 = new byte[] { 4, 5, 6 };

            var mockProvider1 = new Mock<IKeysProvider>();
            mockProvider1.Setup(p => p.GetPublicKey()).Returns((byte[])null);

            var mockProvider2 = new Mock<IKeysProvider>();
            mockProvider2.Setup(p => p.GetPublicKey()).Returns(key1);

            var mockProvider3 = new Mock<IKeysProvider>();
            mockProvider3.Setup(p => p.GetPublicKey()).Returns(key2);

            var keysProvider = new KeysProvider([mockProvider1.Object, mockProvider2.Object, mockProvider3.Object]);
            var retrieved = keysProvider.GetPublicKey();

            Assert.Equal(key1, retrieved);
        }

        [Fact]
        public void GetKey_ReturnsNullWhenNoProviderHasKey()
        {
            var mockProvider1 = new Mock<IKeysProvider>();
            mockProvider1.Setup(p => p.GetPrivateKey()).Returns((byte[])null);

            var mockProvider2 = new Mock<IKeysProvider>();
            mockProvider2.Setup(p => p.GetPrivateKey()).Returns((byte[])null);

            var keysProvider = new KeysProvider([mockProvider1.Object, mockProvider2.Object]);
            var retrieved = keysProvider.GetKey(p => p.GetPrivateKey());

            Assert.Null(retrieved);
        }

        [Fact]
        public void GetKey_UsesProvidedFunctionToRetrieveKey()
        {
            var expectedKey = new byte[] { 1, 2, 3 };

            var mockProvider = new Mock<IKeysProvider>();
            mockProvider.Setup(p => p.GetPublicKey()).Returns(expectedKey);

            var keysProvider = new KeysProvider([mockProvider.Object]);
            var retrieved = keysProvider.GetKey(p => p.GetPublicKey());

            Assert.Equal(expectedKey, retrieved);
        }
    }
}
