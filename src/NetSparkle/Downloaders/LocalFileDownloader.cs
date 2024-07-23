#nullable enable

using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace NetSparkleUpdater.Downloaders
{
    /// <summary>
    /// Class that downloads files from local sources and reports
    /// progress on those files being downloaded.
    /// </summary>
    public class LocalFileDownloader : IUpdateDownloader, IDisposable
    {
        private ILogger? _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private string _downloadFileLocation;

        /// <summary>
        /// Default constructor for the local file downloader.
        /// </summary>
        public LocalFileDownloader()
        {
            _logger = new LogWriter();
            _cancellationTokenSource = new CancellationTokenSource();
            _downloadFileLocation = "";
        }

        /// <summary>
        /// Default constructor for the local file downloader.
        /// Uses default credentials and default proxy.
        /// </summary>
        /// <param name="logger">ILogger to write logs to</param>
        public LocalFileDownloader(ILogger? logger)
        {
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
            _downloadFileLocation = "";
        }

        /// <summary>
        /// ILogger to log data from LocalFileDownloader
        /// </summary>
        public ILogger? LogWriter
        {
            set { _logger = value; }
            get { return _logger; }
        }

        /// <inheritdoc/>
        public bool IsDownloading { get; private set; }

        /// <summary>
        /// When handling the string url in StartFileDownload, 
        /// use Uri.LocalPath as the path to read text from.
        /// </summary>
        public bool UseLocalUriPath { get; set; } = false;

        /// <inheritdoc/>
        public event DownloadProgressEvent? DownloadProgressChanged;
        
        /// <inheritdoc/>
        public event AsyncCompletedEventHandler? DownloadFileCompleted;

        /// <inheritdoc/>
        public void CancelDownload()
        {
            _cancellationTokenSource.Cancel();
            DownloadFileCompleted?.Invoke(this, new AsyncCompletedEventArgs(null, true, null));
            _cancellationTokenSource = new CancellationTokenSource();
            IsDownloading = false;
            if (!string.IsNullOrWhiteSpace(_downloadFileLocation) && File.Exists(_downloadFileLocation))
            {
                try {
                    File.Delete(_downloadFileLocation);
                } catch {}
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        /// <inheritdoc/>
        public async Task<string> RetrieveDestinationFileNameAsync(AppCastItem item)
        {
            return await Task.Run(() => Path.GetFileName(item.DownloadLink));
        }

        /// <inheritdoc/>
        public async void StartFileDownload(Uri uri, string downloadFilePath)
        {
            var path = UseLocalUriPath ? uri.LocalPath : uri.AbsolutePath;
            await CopyFileAsync(path, downloadFilePath, _cancellationTokenSource.Token);
        }

        // https://stackoverflow.com/a/36925751/3938401 and
        // https://stackoverflow.com/questions/39742515/stream-copytoasync-with-progress-reporting-progress-is-reported-even-after-cop
        private async Task CopyFileAsync(string sourceFile, string destinationFile, CancellationToken cancellationToken)
        {
            var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            var bufferSize = 4096;
            var buffer = new byte[bufferSize];
            int bytesRead = 0;
            _downloadFileLocation = destinationFile;
            long totalRead = 0;
            try
            {
                var wasCanceled = false;
                using (var sourceStream =
                      new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions))
                using (var destinationStream =
                      new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.Read, bufferSize, fileOptions))
                {
                    IsDownloading = true;
                    long totalFileLength = sourceStream.Length;
                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            destinationStream.Close();
                            Cancel(destinationFile);
                            wasCanceled = true;
                            break;
                        }
                        totalRead += bytesRead;
                        UpdateDownloadProgress(totalRead, totalFileLength);
                    }
                    IsDownloading = false;
                }
                DownloadFileCompleted?.Invoke(this, new AsyncCompletedEventArgs(null, wasCanceled, null));
            }
            catch (Exception e)
            {
                LogWriter?.PrintMessage("Error: {0}", e.Message);
                Cancel(destinationFile);
                DownloadFileCompleted?.Invoke(this, new AsyncCompletedEventArgs(e, true, null));
                IsDownloading = false;
            }
        }

        private void Cancel(string destinationFile)
        {
            if (File.Exists(destinationFile))
            {
                try {
                    File.Delete(destinationFile);
                } catch {}
            }
            IsDownloading = false;
        }

        private void UpdateDownloadProgress(long totalRead, long totalLength)
        {
            if (totalLength == 0)
            {
                totalLength = 1; // ...just in case.
            }
            int percentage = Convert.ToInt32(Math.Round((double)totalRead / totalLength * 100, 0));
            DownloadProgressChanged?.Invoke(this, new ItemDownloadProgressEventArgs(percentage, this, totalRead, totalLength));
        }
    }
}
