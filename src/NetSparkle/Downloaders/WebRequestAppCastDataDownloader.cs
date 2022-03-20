using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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

        /// <inheritdoc/>
        public string DownloadAndGetAppCastData(string url)
        {
            _appcastUrl = url;
            // configure ssl cert link
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            var response = GetWebContentResponse(url);
            if (response != null)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), GetAppCastEncoding()))
                    {
                        var appCastData = reader.ReadToEnd();
                        ServicePointManager.ServerCertificateValidationCallback -= ValidateRemoteCertificate;
                        return appCastData;
                    }
                }
                catch
                {

                }
            }
            ServicePointManager.ServerCertificateValidationCallback -= ValidateRemoteCertificate;
            return null;
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
