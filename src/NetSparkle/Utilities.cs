using NetSparkleUpdater.Enums;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater
{
    /// <summary>
    /// Provides commonly used utility functions to users of NetSparkleUpdater
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// Removes trailing 0 components from the given version.<br/>
        /// "1.2.3.0" -> "1.2.3";<br/>
        /// "1.2.0.0" -> "1.2";<br/>
        /// "1.0.0.0" -> "1.0"
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
        /// Gets the signature of a file with the given DSA private key.
        /// </summary>
        /// <param name="fileToSignPath">Path to the file you want to sign</param>
        /// <param name="privateKeyFilePath">Path to the private key file</param>
        /// <returns>DSA signature as base64 string</returns>
        public static string? GetDSASignature(string fileToSignPath, string privateKeyFilePath)
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

                using (FileStream inputStream = File.OpenRead(fileToSignPath))
                {
                    byte[] hash = cryptoProvider.SignData(inputStream);
                    var dsaSignature = Convert.ToBase64String(hash);
                    return dsaSignature;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a <see cref="Uri"/> from a URL string. If the URL is relative, converts it 
        /// to an absolute URL based on the appcast URL.
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
        public static string ConvertNumBytesToUserReadableString(long numBytes)
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

        /// <summary>
        /// Get the full base (running) directory for this application including a trailing slash.
        /// From WalletWasabi:
        /// https://github.com/zkSNACKs/WalletWasabi/blob/8d42bce976605cca3326ea6c998b2294494900e6/WalletWasabi/Helpers/EnvironmentHelpers.cs
        /// </summary>
        /// <returns>the full running directory path including trailing slash for this application</returns>
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
            try
            {
                return Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            } 
            catch {}
            return System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
#endif
        }

        /// <summary>
        /// Convert a given <see cref="Stream"/> to a byte array
        /// </summary>
        /// <param name="stream">the <see cref="Stream"/> to convert</param>
        /// <returns>a byte[] array of the data in the given stream</returns>
        public static byte[] ConvertStreamToByteArray(Stream stream)
        {
            // read the data
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            return data;
        }

        /// <summary>
        /// Checks to see whether a signature is ncessary given the provided
        /// info on the <see cref="SecurityMode"/> and whether or not valid
        /// key information exists at the moment.
        /// </summary>
        /// <param name="securityMode">the <see cref="SecurityMode"/> for the signature check</param>
        /// <param name="doesKeyInfoExist">true if the application has appropriate key
        /// information in order to run signature checks; false otherwise</param>
        /// <param name="isCheckingSoftwareDownload">True if the caller is checking on the signature of a software
        /// download; false if the caller is checking on the signature of something else (e.g. release notes,
        /// app cast)</param>
        /// <returns>true if an item's signature needs to be checked; false otherwise</returns>
        public static bool IsSignatureNeeded(SecurityMode securityMode, bool doesKeyInfoExist, bool isCheckingSoftwareDownload = false)
        {
            switch (securityMode)
            {
                case SecurityMode.UseIfPossible:
                    // if we have a public key, we need a signature
                    return doesKeyInfoExist;
                case SecurityMode.Strict:
                    // we always need a signature
                    return true;
                case SecurityMode.Unsafe:
                    return false;
                case SecurityMode.OnlyVerifySoftwareDownloads:
                    return isCheckingSoftwareDownload;

            }
            return false;
        }

        /// <summary>
        /// Read all text from file asynchronously (this method is a fill-in for 
        /// .NET Framework and .NET Standard)
        /// From: https://stackoverflow.com/a/64860277/3938401
        /// </summary>
        /// <param name="path">path to file to read</param>
        /// <returns>file data</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<string> ReadAllTextAsync(string path)
        {
            switch (path)
            {
                case "": throw new ArgumentException("Empty path name is not legal", nameof(path));
                case null: throw new ArgumentNullException(nameof(path));
            }

            using var sourceStream = new FileStream(path, FileMode.Open, 
                FileAccess.Read, FileShare.Read, 
                bufferSize: 4096,
                useAsync: true);
            using var streamReader = new StreamReader(sourceStream, Encoding.UTF8, 
                detectEncodingFromByteOrderMarks: true); 
            // detectEncodingFromByteOrderMarks allows you to handle files with BOM correctly. 
            // Otherwise you may get chinese characters even when your text does not contain any

            return await streamReader.ReadToEndAsync();
        }

        /// <summary>
        /// Write text asynchronously (this method is a fill-in for 
        /// .NET Framework and .NET Standard)
        /// https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/using-async-for-file-access
        /// </summary>
        /// <param name="filePath">Location to write text</param>
        /// <param name="text">Text to write</param>
        /// <returns><seealso cref="Task"/> that you can await for the completion of this function</returns>
        public static async Task WriteTextAsync(string filePath, string text)
        {
            byte[] encodedText = Encoding.Unicode.GetBytes(text);

            using (var sourceStream =
                new FileStream(
                    filePath,
                    FileMode.Create, FileAccess.Write, FileShare.None,
                    bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            }
        }

        /// <summary>
        /// Create a <seealso cref="Stream"/> from a string
        /// https://stackoverflow.com/a/5238289/3938401
        /// </summary>
        /// <param name="str">String to turn into a stream</param>
        /// <param name="encoding"><seealso cref="Encoding"/> for stream</param>
        /// <returns></returns>
        public static MemoryStream GenerateStreamFromString(string str, Encoding encoding)
        {
            return new MemoryStream(encoding.GetBytes(str ?? ""));
        }
    }
}
