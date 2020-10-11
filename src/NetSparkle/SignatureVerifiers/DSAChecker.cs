using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;

namespace NetSparkleUpdater.SignatureVerifiers
{
    /// <summary>
    /// Class that allows you to verify a DSA signature of 
    /// some text, a file, or some other item based on a 
    /// DSA public key
    /// </summary>
    public class DSAChecker : ISignatureVerifier
    {
        private DSACryptoServiceProvider _cryptoProvider;

        /// <inheritdoc/>
        public bool HasValidKeyInformation()
        {
            return _cryptoProvider != null;
        }

        /// <summary>
        /// Create a DSAChecker object from the given parameters
        /// </summary>
        /// <param name="mode">The <see cref="SecurityMode"/> of the validator. Controls what needs to be set in order to validate
        /// an app cast and its items.</param>
        /// <param name="publicKey">the DSA public key as a string (will be used instead of the file if available, non-null, and not blank)</param>
        /// <param name="publicKeyFile">the public key file name (including extension)</param>
        public DSAChecker(SecurityMode mode, string publicKey = null, string publicKeyFile = "NetSparkle_DSA.pub")
        {
            SecurityMode = mode;

            string key = publicKey;

            if (string.IsNullOrEmpty(key))
            {
                Stream data = TryGetResourceStream(publicKeyFile);
                if (data == null)
                {
                    data = TryGetFileResource(publicKeyFile);
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

        /// <inheritdoc/>
        public SecurityMode SecurityMode { get; set; }

        private bool CheckSecurityMode(string signature, ref ValidationResult result)
        {
            var hasValidKeyInformation = HasValidKeyInformation();
            var isSignatureValid = !string.IsNullOrWhiteSpace(signature);
            switch (SecurityMode)
            {
                case SecurityMode.UseIfPossible:
                    // if we have a DSA key, we only accept non-null signatures
                    if (hasValidKeyInformation && !isSignatureValid)
                    {
                        result = ValidationResult.Invalid;
                        return false;
                    }
                    // if we don't have an dsa key, we accept any signature
                    if (!hasValidKeyInformation)
                    {
                        result = ValidationResult.Unchecked;
                        return false;
                    }
                    break;

                case SecurityMode.Strict:
                    // only accept if we have both a public key and a non-null signature
                    if (!hasValidKeyInformation || !isSignatureValid)
                    {
                        result = ValidationResult.Invalid;
                        return false;
                    }
                    break;
                
                case SecurityMode.Unsafe:
                    // always accept anything
                    // If we don't have a signature, make sure to note this as "Unchecked" since we
                    // didn't end up checking anything due to a lack of public key/signature
                    if (!hasValidKeyInformation || !isSignatureValid)
                    {
                        result = ValidationResult.Unchecked;
                        return false;
                    }
                    break;

                case SecurityMode.OnlyVerifySoftwareDownloads:
                    // If we don't have a signature, make sure to note this as "Unchecked" since we
                    // didn't end up checking anything due to a lack of public key/signature
                    if (!hasValidKeyInformation || !isSignatureValid)
                    {
                        result = ValidationResult.Unchecked;
                        return false;
                    }
                    break;
            }
            return true;
        }

        /// <inheritdoc/>
        public ValidationResult VerifySignature(string signature, byte[] dataToVerify)
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

        /// <inheritdoc/>
        public ValidationResult VerifySignatureOfFile(string signature, string binaryPath)
        {
            using (Stream inputStream = File.OpenRead(binaryPath))
            {
                return VerifySignature(signature, Utilities.ConvertStreamToByteArray(inputStream));
            }
        }

        /// <inheritdoc/>
        public ValidationResult VerifySignatureOfString(string signature, string data)
        {
            // creating stream from string
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(data);
                writer.Flush();
                stream.Position = 0;

                return VerifySignature(signature, Utilities.ConvertStreamToByteArray(stream));
            }
        }

        /// <summary>
        /// Gets a file resource based on a public key at a given path
        /// </summary>
        /// <param name="publicKey">the file name of the public key</param>
        /// <returns>the data stream of the file resource if the file exists; null otherwise</returns>
        private static Stream TryGetFileResource(string publicKey)
        {
            Stream data = null;
            if (File.Exists(publicKey))
            {
                data = File.OpenRead(publicKey);
            }
            return data;
        }

        /// <summary>
        /// Get a resource stream based on the public key
        /// </summary>
        /// <param name="publicKey">the public key resource name</param>
        /// <returns>a stream that contains the public key if found; null otherwise</returns>
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
