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
    public class WebRequestAppCastDataDownloader : IAppCastDataDownloader
    {
        private ILogger _logger;
        private string _appcastUrl = "";

        /// <summary>
        /// Default constructor for the app cast data downloader.
        /// </summary>
        public WebRequestAppCastDataDownloader() : this(new LogWriter())
        {
        }

        /// <summary>
        /// Default constructor for the app cast data downloader.
        /// </summary>
        /// <param name="logger">ILogger to write logs to</param>
        public WebRequestAppCastDataDownloader(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            _logger = logger;
        }

        /// <summary>
        /// ILogger to log data from WebRequestAppCastDataDownloader
        /// </summary>
        public ILogger LogWriter
        {
            set { _logger = value; }
            get { return _logger; }
        }

        /// <summary>
        /// If true, don't check the validity of SSL certificates. Defaults to false.
        /// </summary>
        public bool TrustEverySSLConnection { get; set; } = false;

        /// <summary>
        /// If not "", sends extra JSON via POST to server with the web request for update information and for the app cast signature.
        /// </summary>
        public string ExtraJsonData { get; set; } = "";

        /// <summary>
        /// Set this to handle redirects that manually, e.g. redirects that go from HTTPS to HTTP (which are not allowed
        /// by default)
        /// </summary>
        public RedirectHandler RedirectHandler { get; set; }

        /// <inheritdoc/>
        public string DownloadAndGetAppCastData(string url)
        {
            return DownloadAndGetAppCastDataAsync(url).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Used for both async downloading app cast and the app cast's .signature file.
        /// Note that you must handle your own exceptions if they occur. 
        /// Otherwise, <see cref="SparkleUpdater"></see> will act as though the appcast 
        /// failed to download.
        /// </summary>
        /// <param name="url">string URL for the place where the app cast can be downloaded</param>
        /// <returns>The Task to retrieve the app cast data encoded as a string</returns>
        public async Task<string> DownloadAndGetAppCastDataAsync(string url)
        {
            _appcastUrl = url;
            // configure ssl cert link
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;

            var handler = new HttpClientHandler();
            if (RedirectHandler != null)
            {
                handler.AllowAutoRedirect = false;
            }

            if (TrustEverySSLConnection)
            {
#if NETCORE
                // ServerCertificateCustomValidationCallback not available on .NET 4.6.2 (first available in 4.7.1)
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };
#endif
            }

            var httpClient = CreateHttpClient(handler);
            try
            {
                if (!string.IsNullOrWhiteSpace(ExtraJsonData))
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Content = new StringContent(ExtraJsonData, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        ServicePointManager.ServerCertificateValidationCallback -= ValidateRemoteCertificate;
                        using (StreamReader reader = new StreamReader(responseStream, GetAppCastEncoding()))
                        {
                            return await reader.ReadToEndAsync().ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    if (RedirectHandler != null)
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(url).ConfigureAwait(false);
                        if ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399)
                        {
                            var redirectURI = response.Headers.Location;
                            if (RedirectHandler.Invoke(url, redirectURI.ToString(), response))
                            {
                                return await DownloadAndGetAppCastDataAsync(redirectURI.ToString()).ConfigureAwait(false);
                            }
                        }
                        else if (response.IsSuccessStatusCode)
                        {
                            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        Stream responseStream = await httpClient.GetStreamAsync(url).ConfigureAwait(false);
                        ServicePointManager.ServerCertificateValidationCallback -= ValidateRemoteCertificate;
                        using (StreamReader reader = new StreamReader(responseStream, GetAppCastEncoding()))
                        {
                            return await reader.ReadToEndAsync().ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogWriter.PrintMessage("Error: {0}", e.Message);
            }
            return "";
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
        protected virtual HttpClient CreateHttpClient(HttpClientHandler handler)
        {
            if (handler != null)
            {
                return new HttpClient(handler);
            }
            else
            {
                return new HttpClient();
            }
        }

        /// <inheritdoc/>
        public Encoding GetAppCastEncoding()
        {
            return Encoding.UTF8;
        }

        private bool AlwaysTrustRemoteCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Determine if the remote X509 certificate is valid
        /// </summary>
        /// <param name="sender">the web request that is being made</param>
        /// <param name="certificate">the certificate</param>
        /// <param name="chain">the chain</param>
        /// <param name="sslPolicyErrors">any SSL policy errors that have occurred</param>
        /// <returns><c>true</c> if the cert is valid; false otherwise</returns>
        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (TrustEverySSLConnection)
            {
                // verify if we talk about our app cast dll 
                if (sender is HttpWebRequest req && req.RequestUri.Equals(new Uri(_appcastUrl)))
                {
                    return true;
                }
            }

            // check our cert                 
            return sslPolicyErrors == SslPolicyErrors.None && certificate is X509Certificate2 cert2 && cert2.Verify();
        }
    }
}
