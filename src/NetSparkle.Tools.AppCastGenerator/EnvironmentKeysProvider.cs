using System;

namespace NetSparkleUpdater.AppCastGenerator
{
    public class EnvironmentKeysProvider : IKeysProvider
    {
        public const string PrivateKeyEnvironmentVariable = "SPARKLE_PRIVATE_KEY";
        public const string PublicKeyEnvironmentVariable = "SPARKLE_PUBLIC_KEY";

        public byte[]? GetPrivateKey() => GetKey(PrivateKeyEnvironmentVariable);

        public byte[]? GetPublicKey() => GetKey(PublicKeyEnvironmentVariable);

        private static byte[]? GetKey(string environmentVariableName)
        {
            var key = Environment.GetEnvironmentVariable(environmentVariableName);

            if (key != null)
            {
                return Convert.FromBase64String(key);
            }

            return null;
        }
    }
}
