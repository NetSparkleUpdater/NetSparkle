using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
        private string _appcastUrl = "";

        /// <summary>
        /// Default constructor for the app cast data downloader. Basically
        /// does nothing. :)
        /// </summary>
        public LocalFileAppCastDownloader()
        {
        }

        /// <inheritdoc/>
        public string DownloadAndGetAppCastData(string url)
        {
            return File.ReadAllText(url);
        }

        /// <inheritdoc/>
        public Encoding GetAppCastEncoding()
        {
            return Encoding.UTF8;
        }
    }
}
