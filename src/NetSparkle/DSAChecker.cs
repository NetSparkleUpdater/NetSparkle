using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using NetSparkleUpdater.Enums;

namespace NetSparkleUpdater
{
    /// <summary>
    /// Class to verify a DSA signature
    /// </summary>
    public class DSAChecker
    {
        private SecurityMode _securityMode;
        private DSACryptoServiceProvider _cryptoProvider;

        /// <summary>
        /// Determines if a public key exists
        /// </summary>
        /// <returns><c>bool</c></returns>
        public bool DoesPublicKeyExist()
        {
            return _cryptoProvider != null;
        }

        /// <summary>
        /// Create a DSAChecker object from the given parameters
        /// </summary>
        /// <param name="mode">The security mode of the validator. Controls what needs to be set in order to validate
        /// an app cast and its items.</param>
        /// <param name="publicKey">the public key as string (will be preferred before the file)</param>
        /// <param name="publicKeyFile">the public key file</param>
        public DSAChecker(SecurityMode mode, string publicKey = null, string publicKeyFile = "NetSparkle_DSA.pub")
        {
            _securityMode = mode;

            string key = publicKey;

            if (string.IsNullOrEmpty(key))
            {
                Stream data = TryGetResourceStream(publicKeyFile);
                if (data == null)
                {
                    data = TryGetFileResource(publicKeyFile, data);
                }

                if (data != null)
                {
                    using (StreamReader reader = new StreamReader(data))
                    {
                        key = reader.ReadToEnd();
                    }
                }
            }

            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    _cryptoProvider = new DSACryptoServiceProvider();
                    _cryptoProvider.FromXmlString(key);
                }
                catch
                {
                    _cryptoProvider = null;
                }
            }
        }

        /// <summary>
        /// Returns if we need an signature
        /// </summary>
        /// <returns><c>bool</c></returns>
        public bool IsSignatureNeeded()
        {
            switch (_securityMode)
            {
                case SecurityMode.UseIfPossible:
                    // if we have a dsa key, we need a signature
                    return DoesPublicKeyExist();
                case SecurityMode.Strict:
                    // we always need a signature
                    return true;
                case SecurityMode.Unsafe:
                    return false;
            }
            return false;
        }

        private bool CheckSecurityMode(string signature, ref ValidationResult result)
        {
            switch (_securityMode)
            {
                case SecurityMode.UseIfPossible:
                    // if we have a DSA key, we only accept non-null signatures
                    if (DoesPublicKeyExist() && string.IsNullOrEmpty(signature))
                    {
                        result = ValidationResult.Invalid;
                        return false;
                    }
                    // if we don't have an dsa key, we accept any signature
                    if (!DoesPublicKeyExist())
                    {
                        result = ValidationResult.Unchecked;
                        return false;
                    }
                    break;

                case SecurityMode.Strict:
                    // only accept if we have both a public key and a non-null signature
                    if (!DoesPublicKeyExist() || string.IsNullOrEmpty(signature))
                    {
                        result = ValidationResult.Invalid;
                        return false;
                    }
                    break;
                
                case SecurityMode.Unsafe:
                    // always accept anything
                    // If we don't have a signature, make sure to note this as "Unchecked" since we
                    // didn't end up checking anything
                    if (!DoesPublicKeyExist() || string.IsNullOrEmpty(signature))
                    {
                        result = ValidationResult.Unchecked;
                        return false;
                    }
                    break;
            }
            return true;
        }

        /// <summary>
        /// Verifies the DSA signature
        /// </summary>
        /// <param name="signature">expected signature</param>
        /// <param name="stream">the stream of the binary</param>
        /// <returns>A <c>ValidationResult</c> that corresponds to the result of the DSA signature process</returns>
        public ValidationResult VerifyDSASignature(string signature, byte[] dataToVerify)
        {
            ValidationResult res = ValidationResult.Invalid;
            if (!CheckSecurityMode(signature, ref res))
            {
                return res;
            }

            // convert signature
            byte[] bHash = Convert.FromBase64String(signature);

            // verify
            return _cryptoProvider.VerifyData(dataToVerify, bHash) ? ValidationResult.Valid : ValidationResult.Invalid;
        }

        /// <summary>
        /// Verifies the DSA signature
        /// </summary>
        /// <param name="signature">expected signature</param>
        /// <param name="binaryPath">the path to the binary</param>
        /// <returns>A <c>ValidationResult</c> that corresponds to the result of the DSA signature process</returns>
        public ValidationResult VerifyDSASignatureFile(string signature, string binaryPath)
        {
            using (Stream inputStream = File.OpenRead(binaryPath))
            {
                return VerifyDSASignature(signature, ConvertStreamToByteArray(inputStream));
            }
        }

        public byte[] ConvertStreamToByteArray(Stream stream)
        {
            // read the data
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            return data;
        }

        /// <summary>
        /// Verifies the DSA signature of string data
        /// </summary>
        /// <param name="signature">expected signature</param>
        /// <param name="data">the data</param>
        /// <returns>A <c>ValidationResult</c> that corresponds to the result of the DSA signature process</returns>
        public ValidationResult VerifyDSASignatureOfString(string signature, string data)
        {
            // creating stream from string
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(data);
                writer.Flush();
                stream.Position = 0;

                return VerifyDSASignature(signature, ConvertStreamToByteArray(stream));
            }
        }

        /// <summary>
        /// Gets a file resource
        /// </summary>
        /// <param name="publicKey">the public key</param>
        /// <param name="data">the data stream</param>
        /// <returns>the data stream</returns>
        private static Stream TryGetFileResource(string publicKey, Stream data)
        {
            if (File.Exists(publicKey))
            {
                data = File.OpenRead(publicKey);
            }
            return data;
        }

        /// <summary>
        /// Get a resource stream
        /// </summary>
        /// <param name="publicKey">the public key</param>
        /// <returns>a stream</returns>
        private static Stream TryGetResourceStream(string publicKey)
        {
            Stream data = null;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                string[] resources;
                try
                {
                    resources = asm.GetManifestResourceNames();
                }
                catch (NotSupportedException)
                {
                    continue;
                }
                var resourceName = resources.FirstOrDefault(s => s.IndexOf(publicKey, StringComparison.OrdinalIgnoreCase) > -1);
                if (!string.IsNullOrEmpty(resourceName))
                {
                    data = asm.GetManifestResourceStream(resourceName);
                    if (data != null)
                    {
                        break;
                    }
                }
            }
            return data;
        }
    }
}
