#nullable enable

using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// HttpClient implementation from:
// https://gist.github.com/xaecors/88e30c3810f8c626f6223ee48ce064bf

namespace NetSparkleUpdater.Downloaders
{
    /// <summary>
    /// Class that downloads files from the internet and reports
    /// progress on those files being downloaded. Uses a <seealso cref="HttpClient"/>
    /// object as its main method for downloading.
    /// </summary>
    public class WebFileDownloader : IUpdateDownloader, IDisposable
    {
        private HttpClient? _httpClient;
        private ILogger? _logger;
        private CancellationTokenSource? _cts;
        private string? _downloadFileLocation;

        /// <summary>
        /// Default constructor for the web client file downloader.
        /// </summary>
        public WebFileDownloader()
        {
        }

        /// <summary>
        /// Constructor for the web file downloader that takes an <seealso cref="ILogger"/> instance.
        /// </summary>
        /// <param name="logger">ILogger to write logs to</param>
        public WebFileDownloader(ILogger? logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// ILogger to log data from WebFileDownloader
        /// </summary>
        public ILogger? LogWriter
        {
            set { _logger = value; }
            get { return _logger; }
        }

        /// <summary>
        /// Set this to handle redirects that manually, e.g. redirects that go from HTTPS to HTTP (which are not allowed
        /// by default)
        /// </summary>
        public RedirectHandler? RedirectHandler { get; set; }

        /// <summary>
        /// Do preparation work necessary to download a file,
        /// aka set up the HttpClient for use.
        /// </summary>
        public virtual void PrepareToDownloadFile()
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
                        _cts?.Cancel();
                        _httpClient.CancelPendingRequests();
                    } catch {}
                }
            }
            _logger?.PrintMessage("IUpdateDownloader: Creating new HttpClient...");
            _cts = new CancellationTokenSource();
            _httpClient = CreateHttpClient();
        }

        /// <summary>
        /// Create the HttpClient used for file downloads
        /// </summary>
        /// <returns>The client used for file downloads</returns>
        protected virtual HttpClient CreateHttpClient()
        {
            return CreateHttpClient(null);
        }

        /// <summary>
        /// Create the HttpClient used for file downloads (with nullable handler)
        /// </summary>
        /// <param name="handler">HttpClientHandler for messages</param>
        /// <returns>The client used for file downloads</returns>
        protected virtual HttpClient CreateHttpClient(HttpClientHandler? handler)
        {
            return handler != null ? new HttpClient(handler) : new HttpClient();
        }

        /// <inheritdoc/>
        public bool IsDownloading { get; private set; }

        /// <inheritdoc/>
        public event DownloadProgressEvent? DownloadProgressChanged;
        
        /// <inheritdoc/>
        public event AsyncCompletedEventHandler? DownloadFileCompleted;

        /// <inheritdoc/>
        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _httpClient?.Dispose();
        }

        /// <inheritdoc/>
        public async void StartFileDownload(Uri? uri, string downloadFilePath)
        {
            if (uri == null)
            {
                _logger?.PrintMessage("StartFileDownloadAsync had a null Uri; not going to download anything");
                return;
            }
            else 
            {
                _logger?.PrintMessage("IUpdateDownloader: Starting file download from {0} to {1}", uri, downloadFilePath);
                await StartFileDownloadAsync(uri, downloadFilePath);
            }
        }

        private async Task StartFileDownloadAsync(Uri? uri, string downloadFilePath)
        {
            try
            {
                if (uri == null)
                {
                    _logger?.PrintMessage("StartFileDownloadAsync had a null Uri; not going to download anything");
                    return;
                }
                if (_httpClient == null)
                {
                    _httpClient = CreateHttpClient();
                }
                if (_cts == null)
                {
                    _cts = new CancellationTokenSource();
                }
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri))
                using (HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cts.Token))
                {
                    if (!response.IsSuccessStatusCode || !response.Content.Headers.ContentLength.HasValue)
                    {
                        if ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399 && RedirectHandler != null)
                        {
                            var redirectURI = response.Headers.Location;
                            if (RedirectHandler.Invoke(uri.ToString(), redirectURI?.ToString() ?? "", response))
                            {
                                await StartFileDownloadAsync(redirectURI, downloadFilePath);
                            }
                        }
                        else
                        {
                            throw new NetSparkleException(string.Format("Cannot download file (status code: {1}). {2}", uri.ToString(), response.StatusCode,
                                !response.Content.Headers.ContentLength.HasValue ? "No content length header sent." : ""));
                        }
                        return;
                    }
                    long totalLength = 0;
                    long totalRead = 0;
                    long readCount = 0;
                    _downloadFileLocation = downloadFilePath;
                    const int bufferSize = 32*1024; // read 32 KB at a time -- increased on 9/27/2022 from 4 KB
                    using (FileStream fileStream = new FileStream(downloadFilePath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize, true))
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        totalLength = response.Content.Headers.ContentLength ?? 0;
                        totalRead = 0;
                        readCount = 0;
                        byte[] buffer = new byte[bufferSize]; 
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
                            //await Task.Delay(1000); // for TESTING ONLY ("throttling" the download)
                            UpdateDownloadProgress(totalRead, totalLength);
                        } while (IsDownloading);
                        IsDownloading = false;
                    }
                    UpdateDownloadProgress(totalRead, totalLength);
                    DownloadFileCompleted?.Invoke(this, new AsyncCompletedEventArgs(null, false, null));
                }
            }
            catch (Exception e)
            {
                _logger?.PrintMessage("Error: {0}", e.Message);
                IsDownloading = false;
                DownloadFileCompleted?.Invoke(this, new AsyncCompletedEventArgs(e, false, null));
            }
        }

        private void UpdateDownloadProgress(long totalRead, long totalLength)
        {
            if (totalLength == 0)
            {
                totalLength = 1; // just in case
            }
            int percentage = Convert.ToInt32(Math.Round((double)totalRead / totalLength * 100, 0));
            DownloadProgressChanged?.Invoke(this, new ItemDownloadProgressEventArgs(percentage, this, totalRead, totalLength));
        }

        /// <inheritdoc/>
        public void CancelDownload()
        {
            _logger?.PrintMessage("IUpdateDownloader: Canceling download");
            try
            {
                _cts?.Cancel();
                _httpClient?.CancelPendingRequests();
            } catch {}
            if (File.Exists(_downloadFileLocation))
            {
                try {
                    File.Delete(_downloadFileLocation);
                } catch {}
            }
            IsDownloading = false;
            _cts = new CancellationTokenSource();
        }

        private async Task<string?> RetrieveDestinationFileNameAsyncForUri(Uri? uri)
        {
            if (uri == null)
            {
                _logger?.PrintMessage("StartFileDownloadAsync had a null Uri; not going to download anything");
                return "";
            }
            var httpClient = CreateHttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            try
            {
                if (_cts == null)
                {
                    _cts = new CancellationTokenSource();
                }
                using (var response =
                    await httpClient.SendAsync(new HttpRequestMessage
                    {
                        Method = HttpMethod.Head,
                        RequestUri = uri
                    }, HttpCompletionOption.ResponseHeadersRead, _cts.Token).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        //var totalBytes = response.Content.Headers.ContentLength; // TODO: Use this value as well for a more accurate download %?
                        string destFilename = response.RequestMessage?.RequestUri?.LocalPath ?? "";
                        return Path.GetFileName(destFilename);
                    } else if ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399 && RedirectHandler != null)
                    {
                        var redirectURI = response.Headers.Location;
                        if (RedirectHandler.Invoke(uri.ToString(), redirectURI?.ToString() ?? "", response))
                        {
                            return await RetrieveDestinationFileNameAsyncForUri(redirectURI);
                        }
                    }
                    return null;
                }
            }
            catch (Exception e)
            {
                _logger?.PrintMessage("Error retrieving destination file name: {0}", e.Message);
            }
            return null;
        }

        /// <inheritdoc/>
        public async Task<string?> RetrieveDestinationFileNameAsync(AppCastItem item)
        {
            return await RetrieveDestinationFileNameAsyncForUri(new Uri(item.DownloadLink));
        }
    }
}
