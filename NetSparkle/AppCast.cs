using NetSparkle.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace NetSparkle
{
    /// <summary>
    /// An app-cast 
    /// </summary>
    public class AppCast
    {
        private const string itemNode = "item";
        private const string enclosureNode = "enclosure";
        private const string releaseNotesLinkNode = "sparkle:releaseNotesLink";
        private const string descriptionNode = "description";
        private const string versionAttribute = "sparkle:version";
        private const string dsaSignature = "sparkle:dsaSignature";
        private const string criticalAttribute = "sparkle:criticalUpdate";
        private const string lengthAttribute = "length";
        private const string urlAttribute = "url";
        private const string pubDateNode = "pubDate";

        private readonly Configuration _config;
        private readonly string _castUrl;
        private readonly List<AppCastItem> _items;

        private readonly bool _trustEverySSLConnection;
        private readonly string _extraJSON;
        private DSAChecker _dsaChecker;
        private LogWriter _logWriter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="castUrl">the URL of the appcast file</param>
        /// <param name="trustEverySSLConnection">whether or not to trust every SSL connection</param>
        /// <param name="config">the current configuration</param>
        /// <param name="dsaChecker">class to verify that DSA hashes are accurate</param>
        /// <param name="logWriter">object to write any log statements to</param>
        /// <param name="extraJSON">string representation of JSON object to send along with the appcast request. nullable.</param>
        public AppCast(string castUrl, bool trustEverySSLConnection, Configuration config, DSAChecker dsaChecker, LogWriter logWriter, string extraJSON = null)
        {
            _config = config;
            _castUrl = castUrl;

            _items = new List<AppCastItem>();

            _trustEverySSLConnection = trustEverySSLConnection;
            _dsaChecker = dsaChecker;
            _logWriter = logWriter;
            _extraJSON = extraJSON;
        }

        private string TryReadSignature()
        {
            try
            {
                var signaturestream = GetWebContentStream(_castUrl + ".dsa");
                var signature = string.Empty;
                using (StreamReader reader = new StreamReader(signaturestream, Encoding.ASCII))
                {
                    return reader.ReadToEnd().Trim();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Used by <see cref="AppCast"/> to fetch the appcast and DSA signature as a <see cref="Stream"/>.
        /// </summary>
        public Stream GetWebContentStream(string url)
        {
            var response = GetWebContentResponse(url);
            if (response != null)
            {
                var ms = new MemoryStream();
                response.GetResponseStream().CopyTo(ms);
                response.Close();
                ms.Position = 0;
                return ms;
            }
            return null;
        }

        /// <summary>
        /// Used by <see cref="AppCast"/> to fetch the appcast and DSA signature.
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
                        httpRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                    // http://stackoverflow.com/a/10027534/3938401
                    if (_extraJSON != null && _extraJSON != "")
                    {
                        httpRequest.ContentType = "application/json";
                        httpRequest.Method = "POST";

                        using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                        {
                            streamWriter.Write(_extraJSON);
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
        /// Download castUrl resource and parse it
        /// </summary>
        public bool Read()
        {
            try
            {
                var inputstream = GetWebContentStream(_castUrl);
                var signature = TryReadSignature();
                return ReadStream(inputstream, signature);
            }
            catch (Exception e)
            {
                _logWriter.PrintMessage("error reading app cast {0}: {1} ", _castUrl, e.Message);
                return false;
            }
        }

        private bool ReadStream(Stream inputstream, string signature)
        {
            if (inputstream == null)
            {
                _logWriter.PrintMessage("Cannot read response from URL {0}", _castUrl);
                return false;
            }

            // inputstream needs to be copied. WebResponse can't be positioned back
            var memorystream = new MemoryStream();
            inputstream.CopyTo(memorystream);
            memorystream.Position = 0;

            // checking signature
            var signatureNeeded = _dsaChecker.SignatureNeeded();
            if (signatureNeeded && _dsaChecker.VerifyDSASignature(signature, memorystream) == ValidationResult.Invalid)
            {
                _logWriter.PrintMessage("Signature check of appcast failed");
                return false;
            }
            memorystream.Position = 0;

            // parse xml
            using (XmlTextReader reader = new XmlTextReader(memorystream))
            {
                Parse(reader);
                return true;
            }
        }

        private void Parse(XmlReader reader)
        {
            AppCastItem currentItem = null;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case itemNode:
                            currentItem = new AppCastItem()
                            {
                                AppVersionInstalled = _config.InstalledVersion,
                                AppName = _config.ApplicationName,
                                UpdateSize = 0,
                                IsCriticalUpdate = false
                            };
                            break;
                        case releaseNotesLinkNode:
                            if (currentItem != null)
                            {
                                currentItem.ReleaseNotesDSASignature = reader.GetAttribute(dsaSignature);
                                currentItem.ReleaseNotesLink = reader.ReadString().Trim();
                            }
                            break;
                        case descriptionNode:
                            if (currentItem != null)
                            {
                                currentItem.Description = reader.ReadString().Trim();
                            }
                            break;
                        case enclosureNode:
                            if (currentItem != null)
                            {
                                currentItem.Version = reader.GetAttribute(versionAttribute);
                                currentItem.DownloadLink = reader.GetAttribute(urlAttribute);
                                currentItem.DownloadDSASignature = reader.GetAttribute(dsaSignature);
                                string length = reader.GetAttribute(lengthAttribute);
                                if (length != null)
                                {
                                    int size = 0;
                                    if (int.TryParse(length, out size))
                                    {
                                        currentItem.UpdateSize = size;
                                    }
                                    else
                                    {
                                        currentItem.UpdateSize = 0;
                                    }
                                }
                                bool isCritical = false;
                                string critical = reader.GetAttribute(criticalAttribute);
                                if (critical != null && critical == "true" || critical == "1")
                                {
                                    isCritical = true;
                                }
                                currentItem.IsCriticalUpdate = isCritical;
                            }
                            break;
                        case pubDateNode:
                            if (currentItem != null)
                            {
                                string dt = reader.ReadString().Trim();
                                try
                                {
                                    currentItem.PublicationDate = DateTime.ParseExact(dt, "ddd, dd MMM yyyy HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (FormatException ex)
                                {
                                    _logWriter.PrintMessage("Cannot parse item datetime {0} with message {1}", dt, ex.Message);
                                }
                            }
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    switch (reader.Name)
                    {
                        case itemNode:
                            _items.Add(currentItem);
                            break;
                    }
                }
            }

            // sort versions in reverse order
            _items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
        }

        /// <summary>
        /// Returns sorted list of updates between current and latest. Installed is not included.
        /// </summary>
        public AppCastItem[] GetUpdates()
        {
            Version installed = new Version(_config.InstalledVersion);
            var signatureNeeded = _dsaChecker.SignatureNeeded();

            return _items.Where((item) => {
                // filter smaller versions
                if (new Version(item.Version).CompareTo(installed) <= 0)
                    return false;
                // filter versions without signature if we need signatures. But accept version without downloads.
                if (signatureNeeded && string.IsNullOrEmpty(item.DownloadDSASignature) && !string.IsNullOrEmpty(item.DownloadLink))
                    return false;
                // accept everthing else
                return true;
            }).ToArray();
        }
    }
}
