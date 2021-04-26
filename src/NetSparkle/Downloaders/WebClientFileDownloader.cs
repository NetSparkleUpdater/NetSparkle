using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater.Downloaders
{
    /// <summary>
    /// Class that downloads files from the internet and reports
    /// progress on those files being downloaded. Uses a <seealso cref="WebClient"/>
    /// object as its main method for downloading.
    /// </summary>
    public class WebClientFileDownloader : IUpdateDownloader, IDisposable
    {
        private WebClient _webClient;

        /// <summary>
        /// Default constructor for the web client file downloader.
        /// Uses default credentials and default proxy.
        /// </summary>
        public WebClientFileDownloader()
        {
            PrepareToDownloadFile();
        }

        /// <summary>
        /// Do preparation work necessary to download a file,
        /// aka set up the WebClient for use.
        /// </summary>
        public void PrepareToDownloadFile()
        {
            if (_webClient != null)
            {
                // can't re-use WebClient, so cancel old requests
                // and start a new request as needed
                if (_webClient.IsBusy)
                {
                    try 
                    {
                        _webClient.CancelAsync();
                    } catch {}
                }
            }
            _webClient = new WebClient
            {
                UseDefaultCredentials = true,
                Proxy = { Credentials = CredentialCache.DefaultNetworkCredentials },
            };
            _webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            _webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgressChanged?.Invoke(sender, 
                new Events.ItemDownloadProgressEventArgs(e.ProgressPercentage, e.UserState, e.BytesReceived, e.TotalBytesToReceive));
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DownloadFileCompleted?.Invoke(sender, e);
        }

        /// <inheritdoc/>
        public bool IsDownloading
        {
            get => _webClient.IsBusy;
        }

        /// <inheritdoc/>
        public event DownloadProgressEvent DownloadProgressChanged;
        /// <inheritdoc/>
        public event AsyncCompletedEventHandler DownloadFileCompleted;

        /// <inheritdoc/>
        public void Dispose()
        {
            _webClient.Dispose();
        }

        /// <inheritdoc/>
        public void StartFileDownload(Uri uri, string downloadFilePath)
        {
            _webClient.DownloadFileAsync(uri, downloadFilePath);
        }

        /// <inheritdoc/>
        public void CancelDownload()
        {
            try 
            {
                _webClient.CancelAsync();
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
                    }).ConfigureAwait(false))
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
