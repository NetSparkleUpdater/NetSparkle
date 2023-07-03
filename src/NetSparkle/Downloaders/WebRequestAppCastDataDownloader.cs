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
        private string _appcastUrl = "";

        /// <summary>
        /// Default constructor for the app cast data downloader. Basically
        /// does nothing. :)
        /// </summary>
        public WebRequestAppCastDataDownloader()
        {
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
            _appcastUrl = url;
            // configure ssl cert link
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            // use HttpClient synchronously: https://stackoverflow.com/a/53529122/3938401
            var handler = new HttpClientHandler();
            if (RedirectHandler != null)
            {
                handler.AllowAutoRedirect = false;
            }
            if (TrustEverySSLConnection)
            {
#if NETCORE
                // ServerCertificateCustomValidationCallback not available on .NET 4.5.2
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
                    var postTask = Task.Run(() => httpClient.SendAsync(request));
                    postTask.Wait();
                    if (postTask.Result.IsSuccessStatusCode)
                    {
                        var postTaskStream = Task.Run(() => postTask.Result.Content.ReadAsStreamAsync());
                        postTask.Wait();
                        ServicePointManager.ServerCertificateValidationCallback -= ValidateRemoteCertificate;
                        using (StreamReader reader = new StreamReader(postTaskStream.Result, GetAppCastEncoding()))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    if (RedirectHandler != null)
                    {
                        var task = Task.Run(() => httpClient.GetAsync(url));
                        task.Wait();
                        var response = task.Result;
                        if ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399)
                        {
                            var redirectURI = response.Headers.Location;
                            if (RedirectHandler.Invoke(url, redirectURI.ToString(), response))
                            {
                                return DownloadAndGetAppCastData(redirectURI.ToString());
                            }
                        }
                        else if (response.IsSuccessStatusCode)
                        {
                            var readTask = Task.Run(() => response.Content.ReadAsStringAsync());
                            readTask.Wait();
                            return readTask.Result;
                        }
                    }
                    else 
                    {
                        var task = Task.Run(() => httpClient.GetStreamAsync(url));
                        var responseStream = task.Result;
                        ServicePointManager.ServerCertificateValidationCallback -= ValidateRemoteCertificate;
                        using (StreamReader reader = new StreamReader(responseStream, GetAppCastEncoding()))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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

        /// <summary>
        /// Download the app cast from the given URL.
        /// Performs a GET request by default. If <see cref="ExtraJsonData"/> is set,
        /// uses a POST request and sends the JSON data along with the
        /// request.
        /// </summary>
        /// <param name="url">the URL to download the app cast from</param>
        /// <returns>the response from the web server if creating the request
        /// succeeded; null otherwise. The response is not guaranteed to have
        /// succeeded!</returns>
#if NETCORE
        [Obsolete("GetWebContentResponse is deprecated, please use DownloadAndGetAppCastData instead. This method should never have been public. :)")]
#endif
        public WebResponse GetWebContentResponse(string url)
        {
            WebRequest request = WebRequest.Create(url);
            if (request != null)
            {
                if (request is FileWebRequest)
                {
                    var fileRequest = request as FileWebRequest;
                    if (fileRequest != null)
                    {
                        return request.GetResponse();
                    }
                }

                if (request is HttpWebRequest)
                {
                    HttpWebRequest httpRequest = request as HttpWebRequest;
                    httpRequest.UseDefaultCredentials = true;
                    httpRequest.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                    if (TrustEverySSLConnection)
                    {
                        httpRequest.ServerCertificateValidationCallback += AlwaysTrustRemoteCert;
                    }

                    // http://stackoverflow.com/a/10027534/3938401
                    if (!string.IsNullOrWhiteSpace(ExtraJsonData))
                    {
                        httpRequest.ContentType = "application/json";
                        httpRequest.Method = "POST";

                        using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                        {
                            streamWriter.Write(ExtraJsonData);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }
                    }

                    // request the cast and build the stream
                    if (TrustEverySSLConnection)
                    {
                        httpRequest.ServerCertificateValidationCallback -= AlwaysTrustRemoteCert;
                    }
                    return httpRequest.GetResponse();
                }
            }
            return null;
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
