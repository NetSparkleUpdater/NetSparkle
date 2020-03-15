using NetSparkle.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace NetSparkle.Downloaders
{
    class WebClientFileDownloader : IUpdateDownloader, IDisposable
    {
        private WebClient _webClient;

        public WebClientFileDownloader()
        {
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
            DownloadProgressChanged?.Invoke(sender, e);
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DownloadFileCompleted?.Invoke(sender, e);
        }

        public bool IsDownloading
        {
            get => _webClient.IsBusy;
        }

        public event DownloadProgressChangedEventHandler DownloadProgressChanged;
        public event AsyncCompletedEventHandler DownloadFileCompleted;

        public void Dispose()
        {
            _webClient.Dispose();
        }

        public void StartFileDownload(Uri uri, string downloadFilePath)
        {
            _webClient.DownloadFileAsync(uri, downloadFilePath);
        }

        public void CancelDownload()
        {
            _webClient.CancelAsync();
        }
    }
}
