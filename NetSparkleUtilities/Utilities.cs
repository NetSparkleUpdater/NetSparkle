using System;
using System.IO;
using System.Security.Cryptography;

namespace NetSparkleUtilities
{
    /// <summary>
    /// Provides commonly used utility functions.
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// Removes trailing 0 components from the given version.
        /// </summary>
        /// <param name="version">Version object</param>
        /// <returns>Version string</returns>
        public static string GetVersionString(Version version)
        {
            if (version.Revision != 0)
                return version.ToString();
            if (version.Build != 0)
                return version.ToString(3);
            return version.ToString(2);
        }
        
        /// <summary>
        /// Signs a file with the given private key.
        /// </summary>
        /// <param name="fileToSignPath">Path to the file you want to sign</param>
        /// <param name="privateKeyFilePath">Path to the private key file</param>
        /// <returns>DSA signature as base64 string</returns>
        public static string GetDSASignature(string fileToSignPath, string privateKeyFilePath)
        {
            if (string.IsNullOrEmpty(fileToSignPath) || !File.Exists(fileToSignPath))
            {
                return null;
            }
            if (string.IsNullOrEmpty(privateKeyFilePath) || !File.Exists(privateKeyFilePath))
            {
                return null;
            }
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
