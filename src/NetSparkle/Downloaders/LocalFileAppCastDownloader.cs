using NetSparkleUpdater.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetSparkleUpdater.Downloaders
{
    /// <summary>
    /// Class that takes care of downloading data for an app cast, including the 
    /// app cast itself as well as the app cast signature (if available). Allows
    /// you to send extra JSON with your request for the app cast information.
    /// </summary>
    public class LocalFileAppCastDownloader : IAppCastDataDownloader
    {
        /// <summary>
        /// Default constructor for the app cast data downloader. Basically
        /// does nothing. :)
        /// </summary>
        public LocalFileAppCastDownloader()
        {
        }

        /// <summary>
        /// When handling the string url in DownloadAndGetAppCastData, treat the 
        /// url as a Uri and use Uri.LocalPath as the path to read text from.
        /// </summary>
        public bool UseLocalUriPath { get; set; } = false;

        /// <inheritdoc/>
        public string DownloadAndGetAppCastData(string url)
        {
            if (UseLocalUriPath)
            {
                return File.ReadAllText(new Uri(url).LocalPath);
            }

            return File.ReadAllText(url);
        }
        
        /// <inheritdoc/>
        public async Task<string> DownloadAndGetAppCastDataAsync(string url, CancellationToken cancellationToken = default)
        {
            #if NETFRAMEWORK || NETSTANDARD
                return await Utilities.ReadAllTextAsync(UseLocalUriPath ? new Uri(url).LocalPath : url, cancellationToken);
            #else
                return await File.ReadAllTextAsync(UseLocalUriPath ? new Uri(url).LocalPath : url, cancellationToken);
            #endif
        }

        /// <inheritdoc/>
        public Encoding GetAppCastEncoding()
        {
            return Encoding.UTF8;
        }
    }
}
