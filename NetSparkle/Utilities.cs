using System;
using System.IO;
using System.Security.Cryptography;

namespace NetSparkle
{
    public class Utilities
    {
        public static string GetVersionString(Version version)
        {
            if (version.Revision != 0)
                return version.ToString();
            if (version.Build != 0)
                return version.ToString(3);
            return version.ToString(2);
        }

        public static string GetDSASignature(string fileToSignPath, string privateKeyFilePath)
        {
            DSACryptoServiceProvider cryptoProvider = null;
            var privateKey = File.ReadAllText(privateKeyFilePath);
            if (!string.IsNullOrEmpty(privateKey))
            {
                cryptoProvider = new DSACryptoServiceProvider();
                cryptoProvider.FromXmlString(privateKey);

                using (Stream inputStream = File.OpenRead(fileToSignPath))
                {
                    byte[] hash = null;
                    hash = cryptoProvider.SignData(inputStream);
                    var dsaSignature = Convert.ToBase64String(hash);
                    return dsaSignature;
                }
            }

            return null;
        }
    }
}
