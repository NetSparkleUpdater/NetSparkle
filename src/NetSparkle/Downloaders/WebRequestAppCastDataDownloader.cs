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
    class WebRequestAppCastDataDownloader : IAppCastDataDownloader
    {
        private string _appcastUrl = "";

        public WebRequestAppCastDataDownloader()
        {
        }

        /// <summary>
        /// If true, don't check the validity of SSL certificates
        /// </summary>
        public bool TrustEverySSLConnection { get; set; } = false;

        /// <summary>
        /// If not "", sends extra JSON via POST to server with the web request for update information and for the DSA signature.
        /// </summary>
        public string ExtraJsonData { get; set; } = "";

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
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
                    {
                        return reader.ReadToEnd().Trim();
                    }
                }
                catch
                {

                }
            }
            ServicePointManager.ServerCertificateValidationCallback -= ValidateRemoteCertificate;
            return null;
        }

        public Encoding GetAppCastEncoding()
        {
            return Encoding.UTF8;
        }

        /// <summary>
        /// Download the app cast from the given URL.
        /// Performs a GET request by default. If ExtraJsonData is set,
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
                        httpRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
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
                    return httpRequest.GetResponse();
                }
            }
            return null;
        }

        /// <summary>
        /// Determine if the remote X509 certificate is valid
        /// </summary>
        /// <param name="sender">the web request</param>
        /// <param name="certificate">the certificate</param>
        /// <param name="chain">the chain</param>
        /// <param name="sslPolicyErrors">how to handle policy errors</param>
        /// <returns><c>true</c> if the cert is valid</returns>
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
