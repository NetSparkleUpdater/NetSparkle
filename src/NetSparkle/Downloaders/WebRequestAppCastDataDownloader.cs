using NetSparkle.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace NetSparkle.Downloaders
{
    class WebRequestAppCastDataDownloader : IAppCastDataDownloader
    {
        private bool _trustEverySSLConnection;
        private string _extraJSONData;

        public WebRequestAppCastDataDownloader(bool trustEverySSLConnection, string extraJSONData)
        {
            _trustEverySSLConnection = trustEverySSLConnection;
            _extraJSONData = extraJSONData;
        }

        public string DownloadAndGetAppCastData(string url)
        {
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
            return null;
        }

        public Encoding GetAppCastEncoding()
        {
            return Encoding.UTF8;
        }

        /// <summary>
        /// Used by <see cref="XMLAppCast"/> to fetch the appcast and DSA signature.
        /// </summary>
        public WebResponse GetWebContentResponse(string url)
        {
            WebRequest request = WebRequest.Create(url);
            if (request != null)
            {
                if (request is FileWebRequest)
                {
                    FileWebRequest fileRequest = request as FileWebRequest;
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
                    if (_trustEverySSLConnection)
                    {
                        httpRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    }

                    // http://stackoverflow.com/a/10027534/3938401
                    if (_extraJSONData != null && _extraJSONData != "")
                    {
                        httpRequest.ContentType = "application/json";
                        httpRequest.Method = "POST";

                        using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                        {
                            streamWriter.Write(_extraJSONData);
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
    }
}
