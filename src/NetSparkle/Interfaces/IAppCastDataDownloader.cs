using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetSparkle.Interfaces
{
    public interface IAppCastDataDownloader
    {
        /// <summary>
        /// Used for both downloading app cast and the app cast's .dsa file
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        string DownloadAndGetAppCastData(string url);

        Encoding GetAppCastEncoding();
    }
}
