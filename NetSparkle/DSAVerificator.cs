using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace NetSparkle
{
    /// <summary>
    /// Controls the Mode in which situations which files has to be signed with the DSA private key.
    /// If an DSA public key and an signature is preset they allways has to be valid.
    /// </summary>
    public enum SecurityMode
    {
        /// <summary>
        /// All files (with or without signature) will be accepted. This was the default mode before.
        /// I strongly don't recommend this mode. It can cause critical security issues.
        /// </summary>
        Unsafe = 1,

        /// <summary>
        /// If there is an DSA public key all files has to be signed. If there isn't any DSA public key
        /// also files without an signature will be accepted. It's an mix between Unsafe and Strict and
        /// can have some security issues if the DSA public key gets lost in the application.
        /// </summary>
        UseIfPossible = 2,

        /// <summary>
        /// Every file has to be signed. This means the DSA public key must exist. I recommend this mode
        /// to enforce the use of secure update informations. This is the default mode.
        /// </summary>
        Strict = 3,
    }

    /// <summary>
    /// Return value of the DSA verification check functions.
    /// </summary>
    public enum ValidationResult
    {
        /// <summary>
        /// The DSA public key and signature exists and they are valid.
        /// </summary>
        Valid = 1,

        /// <summary>
        /// Depending on the SecirityMode at least one of DSA public key or the signature dosn't exist or
        /// they exists but they are not valid. In this case the file will be rejected.
        /// </summary>
        Invalid = 2,

        /// <summary>
        /// There wasn't any DSA public key or signature and SecurityMode said this is okay.
        /// </summary>
        Unchecked = 3,
    }

    /// <summary>
    /// Class to verify a DSA signature
    /// </summary>
    public class DSAChecker
    {
        private SecurityMode _securityMode;
        private DSACryptoServiceProvider _provider;

        /// <summary>
        /// Determines if a public key exists
        /// </summary>
        /// <returns><c>bool</c>  </returns>
        public bool PublicKeyExists()
        {
            if (_provider == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mode">The security mode of the validator. Control what parts has to be exist</param>
        /// <param name="publicKey">the public key as string (will be prefered before the file)</param>
        /// <param name="publicKeyFile">the public key file</param>
        public DSAChecker(SecurityMode mode, string publicKey = null, string publicKeyFile = "NetSparkle_DSA.pub")
        {
            _securityMode = mode;

            string key = publicKey;

            if (string.IsNullOrEmpty(key))
            {
                // TODO: Loading Ressources don't work
                Stream data = TryGetResourceStream(publicKeyFile);
                if (data == null)
                    data = TryGetFileResource(publicKeyFile, data);


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
                    _provider = new DSACryptoServiceProvider();
                    _provider.FromXmlString(key);
                }
                catch
                {
                    _provider = null;
                }
            }
        }

        /// <summary>
        /// Returns if we need an signature
        /// </summary>
        /// <returns><c>bool</c>  </returns>
        public bool SignatureNeeded()
        {
            switch (_securityMode)
            {
                case SecurityMode.UseIfPossible:
                    // if we have an dsa key we need an signature
                    return PublicKeyExists();

                case SecurityMode.Strict:
                    // we always need an signature
                    return true;

                case SecurityMode.Unsafe:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool CheckSecurityMode(string signature, ref ValidationResult result)
        {
            switch (_securityMode)
            {
                case SecurityMode.UseIfPossible:
                    // if we have an dsa key we accept only signatures
                    if (PublicKeyExists() && string.IsNullOrEmpty(signature))
                    {
                        result = ValidationResult.Invalid;
                        return false;
                    }
                    // if we don't have an dsa key we accept all.
                    if (!PublicKeyExists())
                    {
                        result = ValidationResult.Unchecked;
                        return false;
                    }
                    break;

                case SecurityMode.Strict:
                    // only accept if we have booth
                    if (!PublicKeyExists() || string.IsNullOrEmpty(signature))
                    {
                        result = ValidationResult.Invalid;
                        return false;
                    }
                    break;
                
                case SecurityMode.Unsafe:
                    // allways accept anything.
                    // but exit with unchecked if we have an signature
                    if (!PublicKeyExists() || string.IsNullOrEmpty(signature))
                    {
                        result = ValidationResult.Unchecked;
                        return false;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }

        /// <summary>
        /// Verifies the DSA signature
        /// </summary>
        /// <param name="signature">expected signature</param>
        /// <param name="stream">the stream of the binary</param>
        /// <returns><c>true</c> if the signature matches the expected signature.</returns>
        public ValidationResult VerifyDSASignature(string signature, Stream stream)
        {
            ValidationResult res = ValidationResult.Invalid;
            if (!CheckSecurityMode(signature, ref res))
                return res;

            // convert signature
            byte[] bHash = Convert.FromBase64String(signature);

            // read the data
            byte[] bData = null;
            bData = new Byte[stream.Length];
            stream.Read(bData, 0, bData.Length);

            // verify
            return _provider.VerifyData(bData, bHash) ? ValidationResult.Valid : ValidationResult.Invalid;
        }

        /// <summary>
        /// Verifies the DSA signature
        /// </summary>
        /// <param name="signature">expected signature</param>
        /// <param name="binaryPath">the path to the binary</param>
        /// <returns><c>true</c> if the signature matches the expected signature.</returns>
        public ValidationResult VerifyDSASignatureFile(string signature, string binaryPath)
        {
            var data = string.Empty;
            using (Stream inputStream = File.OpenRead(binaryPath))
            {
                return VerifyDSASignature(signature, inputStream);
            }
        }

        /// <summary>
        /// Verifies the DSA signature of string data
        /// </summary>
        /// <param name="signature">expected signature</param>
        /// <param name="data">the data</param>
        /// <returns><c>true</c> if the signature matches the expected signature.</returns>
        public ValidationResult VerifyDSASignatureOfString(string signature, string data)
        {
            // creating stream from string
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(data);
                writer.Flush();
                stream.Position = 0;

                return VerifyDSASignature(signature, stream);
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
                    Debug.WriteLine("Skipped assembly {0}", asm.FullName);
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
