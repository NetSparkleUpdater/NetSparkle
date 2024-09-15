using System;
using System.Linq;
using System.IO;
using System.Reflection;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;
using Chaos.NaCl;

namespace NetSparkleUpdater.SignatureVerifiers
{
    /// <summary>
    /// Class to verify a Ed25519 signature
    /// </summary>
    public class Ed25519Checker : ISignatureVerifier
    {
        private Ed25519Signer? _signer;

        /// <summary>
        /// Create a Ed25519Checker object from the given parameters
        /// </summary>
        /// <param name="mode">The security mode of the validator. Controls what needs to be set in order to validate
        /// an app cast and its items.</param>
        /// <param name="publicKey">the base 64 public key as a string</param>
        /// <param name="publicKeyFile">the public key file</param>
        /// <param name="readFileBeingVerifiedInChunks">if true, reads the file this checker is verifying in chunks rather than all at once</param>
        /// <param name="chunkSize">if reading the file in chunks, size of chunks to read with. Defaults to 25 MB.</param>
        public Ed25519Checker(SecurityMode mode, string? publicKey = null, string? publicKeyFile = "NetSparkle_Ed25519.pub", 
            bool readFileBeingVerifiedInChunks = false, int chunkSize = 1024*1024*25)
        {
            SecurityMode = mode;
            ReadFileBeingVerifiedInChunks = readFileBeingVerifiedInChunks;
            ChunkSize = chunkSize > 0 ? chunkSize : 1024 * 1024 * 25;

            if (publicKeyFile != null && string.IsNullOrWhiteSpace(publicKey))
            {
                Stream? data = TryGetResourceStream(publicKeyFile);
                if (data == null)
                {
                    data = TryGetFileResource(publicKeyFile);
                }

                if (data != null)
                {
                    using (StreamReader reader = new StreamReader(data))
                    {
                        publicKey = reader.ReadToEnd();
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(publicKey))
            {
                try
                {
                    _signer = new Ed25519Signer();
                    _signer.Init(Convert.FromBase64String(publicKey), null);
                }
                catch
                {
                    _signer = null;
                }
            }
        }

        /// <summary>
        /// Determines if a public key exists
        /// </summary>
        /// <returns><c>bool</c></returns>
        public bool HasValidKeyInformation()
        {
            return _signer != null;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public SecurityMode SecurityMode { get; set; }

        /// <summary>
        /// When verifying files, whether to read the file in by chunks.
        /// This will save RAM when verifying files because
        /// it saves the file from being in both a byte[] and in the signature
        /// verifier internal byte storage.
        /// </summary>
        public bool ReadFileBeingVerifiedInChunks { get; set; }

        /// <summary>
        /// If reading file being verified in chunks, the size of the chunk.
        /// Defaults to 25 MB.
        /// </summary>
        public int ChunkSize { get; set; }

        private bool CheckSecurityMode(string signature, ref ValidationResult result)
        {
            switch (SecurityMode)
            {
                case SecurityMode.UseIfPossible:
                    // if we have a DSA key, we only accept non-null signatures
                    if (HasValidKeyInformation() && string.IsNullOrWhiteSpace(signature))
                    {
                        result = ValidationResult.Invalid;
                        return false;
                    }
                    // if we don't have an dsa key, we accept any signature
                    if (!HasValidKeyInformation())
                    {
                        result = ValidationResult.Unchecked;
                        return false;
                    }
                    break;

                case SecurityMode.Strict:
                    // only accept if we have both a public key and a non-null signature
                    if (!HasValidKeyInformation() || string.IsNullOrWhiteSpace(signature))
                    {
                        result = ValidationResult.Invalid;
                        return false;
                    }
                    break;
                
                case SecurityMode.Unsafe:
                    // always accept anything
                    result = ValidationResult.Unchecked;
                    return false;

                case SecurityMode.OnlyVerifySoftwareDownloads:
                    // If we don't have a signature, make sure to note this as "Unchecked" since we
                    // didn't end up checking anything due to a lack of public key/signature
                    if (!HasValidKeyInformation() || string.IsNullOrWhiteSpace(signature))
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
            if (_signer == null)
            {
                return res;
            }

            byte[] signatureBytes = Convert.FromBase64String(signature);
            _signer.AddToBuffer(dataToVerify, 0, dataToVerify.Length);
            return _signer.VerifySignature(signatureBytes) ? ValidationResult.Valid : ValidationResult.Invalid;
        }

        /// <inheritdoc/>
        public ValidationResult VerifySignatureOfFile(string signature, string binaryPath)
        {
            if (ReadFileBeingVerifiedInChunks)
            {
                ValidationResult res = ValidationResult.Invalid;
                if (!CheckSecurityMode(signature, ref res))
                {
                    return res;
                }
                if (_signer == null)
                {
                    return res;
                }

                // code for reading stream in chunks modified from https://stackoverflow.com/a/7542077/3938401
                byte[] bHash = Convert.FromBase64String(signature);
                var chunkSize = ChunkSize > 0 ? ChunkSize : 1024 * 1024 * 25;
                using (FileStream inputStream = File.OpenRead(binaryPath))
                {
                    // read file in chunks
                    byte[] buffer = new byte[chunkSize]; // read in chunks of ChunkSize
                    int bytesRead;
                    while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        _signer.AddToBuffer(buffer, 0, bytesRead);
                    }
                    return _signer.VerifySignature(bHash) ? ValidationResult.Valid : ValidationResult.Invalid;
                }
            }
            else
            {
                using (FileStream inputStream = File.OpenRead(binaryPath))
                {
                    return VerifySignature(signature, Utilities.ConvertStreamToByteArray(inputStream));
                }
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
        private static Stream? TryGetFileResource(string publicKey)
        {
            Stream? data = null;
            if (File.Exists(publicKey))
            {
                data = File.OpenRead(publicKey);
            }
            return data;
        }

        /// <summary>
        /// Get a resource stream based on the public key
        /// </summary>
        /// <param name="publicKeyResourceName">the public key resource name</param>
        /// <returns>a stream that contains the public key if found; null otherwise</returns>
        private static Stream? TryGetResourceStream(string publicKeyResourceName)
        {
            Stream? data = null;
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
                var resourceName = resources.FirstOrDefault(s => s.IndexOf(publicKeyResourceName, StringComparison.OrdinalIgnoreCase) > -1);
                if (!string.IsNullOrWhiteSpace(resourceName))
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
