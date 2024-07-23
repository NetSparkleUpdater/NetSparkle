﻿using NetSparkleUpdater.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NetSparkleUpdater
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
            {
                return version.ToString();
            }
            if (version.Build != 0)
            {
                return version.ToString(3);
            }
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
            if (string.IsNullOrWhiteSpace(fileToSignPath) || !File.Exists(fileToSignPath))
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(privateKeyFilePath) || !File.Exists(privateKeyFilePath))
            {
                return null;
            }
            var privateKey = File.ReadAllText(privateKeyFilePath);
            if (!string.IsNullOrWhiteSpace(privateKey))
            {
                DSACryptoServiceProvider cryptoProvider = new DSACryptoServiceProvider();
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

        /// <summary>
        /// Creates a <see cref="Uri"/> from a URL string. If the URL is relative, converts it to an absolute URL based on the appcast URL.
        /// </summary>
        /// <param name="url">relative or absolute URL</param>
        /// <param name="appcastURL">URL to appcast</param>
        public static Uri GetAbsoluteURL(string url, string appcastURL)
        {
            return new Uri(new Uri(appcastURL), url);
        }

        /// <summary>
        /// Convert a number of bytes to a user-readable string
        /// </summary>
        /// <param name="numBytes">Number of bytes to convert</param>
        /// <returns>A string that represents the number of bytes in KB, MB, or GB if numBytes > 1024.
        /// If numBytes is less than 1024, returns numBytes.</returns>
        public static string NumBytesToUserReadableString(long numBytes)
        {
            if (numBytes > 1024)
            {
                double numBytesDecimal = numBytes;
                // Put in KB
                numBytesDecimal /= 1024;
                if (numBytesDecimal > 1024)
                {
                    // Put in MB
                    numBytesDecimal /= 1024;
                    if (numBytesDecimal > 1024)
                    {
                        // Put in GB
                        numBytesDecimal /= 1024;
                        return numBytesDecimal.ToString("F2") + " GB";
                    }
                    return numBytesDecimal.ToString("F2") + " MB";
                }
                return numBytesDecimal.ToString("F2") + " KB";
            }
            return numBytes.ToString();
        }

        // From WalletWasabi:
        // https://github.com/zkSNACKs/WalletWasabi/blob/8d42bce976605cca3326ea6c998b2294494900e6/WalletWasabi/Helpers/EnvironmentHelpers.cs
        public static string GetFullBaseDirectory()
        {
#if NETCORE
            var fullBaseDirectory = Path.GetFullPath(AppContext.BaseDirectory);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!fullBaseDirectory.StartsWith("/"))
                {
                    fullBaseDirectory = fullBaseDirectory.Insert(0, "/");
                }
            }

            return fullBaseDirectory;
#else
            // https://stackoverflow.com/a/837501/3938401
            return System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
#endif
        }

        public static byte[] ConvertStreamToByteArray(Stream stream)
        {
            // read the data
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            return data;
        }

        public static bool IsSignatureNeeded(SecurityMode securityMode, bool doesKeyInfoExist)
        {
            switch (securityMode)
            {
                case SecurityMode.UseIfPossible:
                    // if we have a dsa key, we need a signature
                    return doesKeyInfoExist;
                case SecurityMode.Strict:
                    // we always need a signature
                    return true;
                case SecurityMode.Unsafe:
                    return false;
            }
            return false;
        }
    }
}
