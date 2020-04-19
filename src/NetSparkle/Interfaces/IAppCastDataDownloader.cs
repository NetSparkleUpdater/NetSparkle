using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetSparkleUpdater.Interfaces
{
    public interface IAppCastDataDownloader
    {
        /// <summary>
        /// Used for both downloading app cast and the app cast's .dsa file
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        string DownloadAndGetAppCastData(string url);

        /// <summary>
        /// Get the string encoding (e.g. UTF8 or ASCII) of the 
        /// app cast file so that it can be converted to bytes.
        /// (WebRequestAppCastDataDownloader defaults to UTF8.)
        /// </summary>
        /// <returns></returns>
        Encoding GetAppCastEncoding();
    }
}
