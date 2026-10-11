using System;

namespace NetSparkleUpdater.AppCastGenerator
{
    public class ValueKeysProvider : IKeysProvider
    {
        private string? _privateKey;
        private string? _publicKey;

        public void SetPublicKey(string value) => _publicKey = value;

        public void SetPrivateKey(string value) => _privateKey = value;

        public byte[]? GetPrivateKey() => GetKey(_privateKey);

        public byte[]? GetPublicKey() => GetKey(_publicKey);

        private static byte[]? GetKey(string? key)
        {
            return !string.IsNullOrWhiteSpace(key)
                ? Convert.FromBase64String(key)
                : null;
        }
    }
}
