using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// HttpClient implementation from:
// https://gist.github.com/xaecors/88e30c3810f8c626f6223ee48ce064bf

namespace NetSparkleUpdater.Downloaders
{
    /// <summary>
    /// Class that downloads files from the internet and reports
    /// progress on those files being downloaded. Uses a <seealso cref="WebClient"/>
    /// object as its main method for downloading.
    /// </summary>
    public class WebClientFileDownloader : IUpdateDownloader, IDisposable
    {
        private HttpClient _httpClient;
        private ILogger _logger;
        private CancellationTokenSource _cts;

        /// <summary>
        /// Default constructor for the web client file downloader.
        /// Uses default credentials and default proxy.
        /// </summary>
        public WebClientFileDownloader()
        {
            PrepareToDownloadFile();
        }

        /// <summary>
        /// Default constructor for the web client file downloader.
        /// Uses default credentials and default proxy.
        /// </summary>
        /// <param name="logger">ILogger to write logs to</param>
        public WebClientFileDownloader(ILogger logger)
        {
            _logger = logger;
            PrepareToDownloadFile();
        }

        /// <summary>
        /// ILogger to log data from WebClientFileDownloader
        /// </summary>
        public ILogger LogWriter
        {
            set { _logger = value; }
            get { return _logger; }
        }

        /// <summary>
        /// Do preparation work necessary to download a file,
        /// aka set up the WebClient for use.
        /// </summary>
        public void PrepareToDownloadFile()
        {
            _logger?.PrintMessage("IUpdateDownloader: Preparing to download file...");
            if (_httpClient != null)
            {
                _logger?.PrintMessage("IUpdateDownloader: HttpClient existed already. Canceling...");
                // can't re-use WebClient, so cancel old requests
                // and start a new request as needed
                if (IsDownloading)
                {
                    try 
                    {
                        _httpClient.CancelPendingRequests();
                    } catch {}
                }
            }
            _logger?.PrintMessage("IUpdateDownloader: Creating new HttpClient...");
            _cts = new CancellationTokenSource();
            _httpClient = new HttpClient();
        }

        /// <inheritdoc/>
        public bool IsDownloading { get; private set; }

        /// <inheritdoc/>
        public event DownloadProgressEvent DownloadProgressChanged;
        /// <inheritdoc/>
        public event AsyncCompletedEventHandler DownloadFileCompleted;

        /// <inheritdoc/>
        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _httpClient?.Dispose();
        }

        /// <inheritdoc/>
        public void StartFileDownload(Uri uri, string downloadFilePath)
        {
            _logger?.PrintMessage("IUpdateDownloader: Starting file download from {0} to {1}", uri, downloadFilePath);

            AsyncHelper.RunSync(async () =>
            {
                await StartFileDownloadAsync(uri, downloadFilePath);
            });
        }

        private async Task StartFileDownloadAsync(Uri uri, string downloadFilePath)
        {
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri))
                using (HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cts.Token))
                {
                    if (!response.IsSuccessStatusCode || !response.Content.Headers.ContentLength.HasValue)
                    {
                        return;
                    }
                    using (FileStream fileStream = new FileStream(downloadFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        long totalLength = response.Content.Headers.ContentLength ?? 0;
                        long totalRead = 0;
                        long readCount = 0;
                        byte[] buffer = new byte[8192]; // read 4 KB at a time
                        UpdateDownloadProgress(0, totalLength);
                        IsDownloading = true;

                        do
                        {
                            if (_cts.IsCancellationRequested)
                            {
                                DownloadFileCompleted?.Invoke(this, new AsyncCompletedEventArgs(null, true, null));
                            }

                            int bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cts.Token);
                            if (bytesRead == 0)
                            {
                                UpdateDownloadProgress(totalRead, totalLength);
                                break;
                            }
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;
                            readCount += 1;
                            UpdateDownloadProgress(totalRead, totalLength);
                        } while (IsDownloading);
                        IsDownloading = false;
                        fileStream.Close();
                        contentStream.Close();
                        UpdateDownloadProgress(totalRead, totalLength);
                        DownloadFileCompleted?.Invoke(this, new AsyncCompletedEventArgs(null, false, null));
                    }
                }
            }
            catch (Exception e)
            {
                LogWriter.PrintMessage("Error: {0}", e.Message);
                IsDownloading = false;
                DownloadFileCompleted?.Invoke(this, new AsyncCompletedEventArgs(e, true, null));
            }
        }

        private void UpdateDownloadProgress(long totalRead, long totalLength)
        {
            int percentage = Convert.ToInt32(Math.Round((double)totalRead / totalLength * 100, 0));

            DownloadProgressChanged?.Invoke(this, new ItemDownloadProgressEventArgs(percentage, null, totalRead, totalLength));
        }

        /// <inheritdoc/>
        public void CancelDownload()
        {
            _logger?.PrintMessage("IUpdateDownloader: Canceling download");
            try
            {
                _cts.Cancel();
                _httpClient.CancelPendingRequests();
            } catch {}
        }

        /// <inheritdoc/>
        public async Task<string> RetrieveDestinationFileNameAsync(AppCastItem item)
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            try
            {
                using (var response =
                    await httpClient.SendAsync(new HttpRequestMessage
                    {
                        Method = HttpMethod.Head,
                        RequestUri = new Uri(item.DownloadLink)
                    }, HttpCompletionOption.ResponseHeadersRead, _cts.Token).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        //var totalBytes = response.Content.Headers.ContentLength; // TODO: Use this value as well for a more accurate download %?
                        string destFilename = response.RequestMessage?.RequestUri?.LocalPath;

                        return Path.GetFileName(destFilename);
                    }
                    return null;
                }
            }
            catch
            {
            }
            return null;
        }
    }
}
