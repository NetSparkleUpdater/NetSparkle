using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace NetSparkle.Interfaces
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
    }
}
