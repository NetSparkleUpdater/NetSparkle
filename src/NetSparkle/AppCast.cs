using NetSparkle.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace NetSparkle
{
    /// <summary>
    /// An app-cast 
    /// </summary>
    public class AppCast
    {
        private readonly Configuration _config;
        private readonly string _castUrl;

        private readonly bool _trustEverySSLConnection;
        private readonly string _extraJSON;
        private readonly DSAChecker _dsaChecker;
        private readonly LogWriter _logWriter;

        /// <summary>
        /// Sparkle XML namespace
        /// </summary>
        public static readonly XNamespace SparkleNamespace = "http://www.andymatuschak.org/xml-namespaces/sparkle";

        /// <summary>
        /// AppCast Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// AppCast Language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// AppCastItems from the appcast
        /// </summary>
        public readonly List<AppCastItem> Items;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="castUrl">the URL of the appcast file</param>
        /// <param name="trustEverySSLConnection">whether or not to trust every SSL connection</param>
        /// <param name="config">the current configuration</param>
        /// <param name="dsaChecker">class to verify that DSA hashes are accurate</param>
        /// <param name="logWriter">object to write any log statements to</param>
        /// <param name="extraJSON">string representation of JSON object to send along with the appcast request. nullable.</param>
        public AppCast(string castUrl, bool trustEverySSLConnection, Configuration config, DSAChecker dsaChecker, LogWriter logWriter = null, string extraJSON = null)
        {
            _config = config;
            _castUrl = castUrl;

            Items = new List<AppCastItem>();

            _trustEverySSLConnection = trustEverySSLConnection;
            _dsaChecker = dsaChecker;
            _logWriter = logWriter ?? new LogWriter();
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
                    {
                        httpRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    }

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
            Parse(memorystream);
            return true;
        }

        /// <summary>
        /// Parse an XML memory stream build items list
        /// </summary>
        /// <param name="stream">The xml memory stream to parse</param>
        private void Parse(MemoryStream stream)
        {
            const string itemNode = "item";

            XDocument doc = XDocument.Load(stream);
            var rss = doc?.Element("rss");
            var channel = rss?.Element("channel");

            Title = channel?.Element("title")?.Value ?? string.Empty;
            Language = channel?.Element("language")?.Value ?? "en";

            var items = doc.Descendants(itemNode);
            foreach (var item in items)
            {
                var currentItem = AppCastItem.Parse(_config.InstalledVersion, _config.ApplicationName, _castUrl, item, _logWriter);
                Items.Add(currentItem);
            }

            // sort versions in reverse order
            Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
        }

        /// <summary>
        /// Returns sorted list of updates between current and latest. Installed is not included.
        /// </summary>
        public List<AppCastItem> GetUpdates()
        {
            Version installed = new Version(_config.InstalledVersion);
            var signatureNeeded = _dsaChecker.SignatureNeeded();

            return Items.Where((item) =>
            {
                // don't allow non-windows updates
                if (!item.IsWindowsUpdate)
                {
                    return false;
                }
                // filter smaller versions
                if (new Version(item.Version).CompareTo(installed) <= 0)
                {
                    return false;
                }
                // filter versions without signature if we need signatures. But accept version without downloads.
                if (signatureNeeded && string.IsNullOrEmpty(item.DownloadDSASignature) && !string.IsNullOrEmpty(item.DownloadLink))
                {
                    return false;
                }
                // accept everything else
                return true;
            }).ToList();
        }

        /// <summary>
        /// Create AppCast XML
        /// </summary>
        /// <param name="items">The AppCastItems to include in the AppCast</param>
        /// <param name="title">AppCast application title</param>
        /// <param name="link">AppCast link</param>
        /// <param name="description">AppCast description</param>
        /// <param name="language">AppCast language</param>
        /// <returns>AppCast xml document</returns>
        public static XDocument GenerateAppCastXml(List<AppCastItem> items, string title, string link = "", string description = "", string language = "en")
        {
            var channel = new XElement("channel");
            channel.Add(new XElement("title", title));

            if (!string.IsNullOrEmpty(link))
            {
                channel.Add(new XElement("link", link));
            }

            if (!string.IsNullOrEmpty(description))
            {
                channel.Add(new XElement("description", description));
            }

            channel.Add(new XElement("language", language));

            foreach (var item in items)
            {
                channel.Add(item.GetXElement());
            }

            var document = new XDocument(
                new XElement("rss", new XAttribute("version", "2.0"), new XAttribute(XNamespace.Xmlns + "sparkle", SparkleNamespace),
                    channel)
            );

            return document;
        }
    }
}
