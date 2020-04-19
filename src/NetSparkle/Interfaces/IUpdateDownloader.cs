using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater.Interfaces
{
    public interface IUpdateDownloader
    {
        /// <summary>
        /// Return true if the update downloader is currently downloading the update
        /// </summary>
        bool IsDownloading { get; }

        /// <summary>
        /// Event to call when some progress has been made on the download
        /// </summary>
        event DownloadProgressChangedEventHandler DownloadProgressChanged;

        /// <summary>
        /// Event to call when the download of the update file has been completed
        /// </summary>
        event AsyncCompletedEventHandler DownloadFileCompleted;

        /// <summary>
        /// Start the download of the file. The file download should be asynchronous!
        /// </summary>
        /// <param name="uri">URL for the download</param>
        /// <param name="downloadFilePath">Where to download the file</param>
        void StartFileDownload(Uri uri, string downloadFilePath);

        /// <summary>
        /// Cancel the download.
        /// </summary>
        void CancelDownload();

        /// <summary>
        /// Clean up and dispose of anything that has to be disposed of
        /// (cancel the download if needed, etc.)
        /// </summary>
        void Dispose();

        /// <summary>
        /// Retrieve the download file name of the app cast item from the server.
        /// This is useful if the server has any sort of redirects that take place
        /// when starting the download process. The client will use this file name
        /// when saving the file on disk.
        /// NetSparkle.CheckServerFileName = false can be set to avoid this call.
        /// </summary>
        /// <param name="item">The AppCastItem that will be downloaded</param>
        /// <returns>The file name of the file to download from the server 
        /// (including file extension). Null if not found/had error/not applicable.</returns>
        Task<string> RetrieveDestinationFileNameAsync(AppCastItem item);
    }
}
