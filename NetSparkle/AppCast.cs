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

        //private void Parse(XmlReader reader)
        //{
        //    const string itemNode = "item";
        //    const string enclosureNode = "enclosure";
        //    const string sparkleEnclosureNode = "sparkle:enclosure";
        //    const string releaseNotesLinkNode = "sparkle:releaseNotesLink";
        //    const string descriptionNode = "description";
        //    const string versionAttribute = "sparkle:version";
        //    const string dsaSignature = "sparkle:dsaSignature";
        //    const string criticalAttribute = "sparkle:criticalUpdate";
        //    const string operatingSystemAttribute = "sparkle:os";
        //    const string lengthAttribute = "length";
        //    const string typeAttribute = "type";
        //    const string urlAttribute = "url";
        //    const string pubDateNode = "pubDate";

        //    AppCastItem currentItem = null;

        //    while (reader.Read())
        //    {
        //        if (reader.NodeType == XmlNodeType.Element)
        //        {
        //            switch (reader.Name)
        //            {
        //                case itemNode:
        //                    currentItem = new AppCastItem()
        //                    {
        //                        AppVersionInstalled = _config.InstalledVersion,
        //                        AppName = _config.ApplicationName,
        //                        UpdateSize = 0,
        //                        IsCriticalUpdate = false,
        //                        OperatingSystemString = "windows",
        //                        MIMEType = "application/octet-stream"
        //                    };
        //                    break;
        //                case releaseNotesLinkNode:
        //                    if (currentItem != null)
        //                    {
        //                        currentItem.ReleaseNotesDSASignature = reader.GetAttribute(dsaSignature);
        //                        currentItem.ReleaseNotesLink = reader.ReadString().Trim();
        //                    }
        //                    break;
        //                case descriptionNode:
        //                    if (currentItem != null)
        //                    {
        //                        currentItem.Description = reader.ReadString().Trim();
        //                    }
        //                    break;
        //                case enclosureNode:
        //                case sparkleEnclosureNode:
        //                    if (currentItem != null)
        //                    {
        //                        currentItem.Version = reader.GetAttribute(versionAttribute);
        //                        currentItem.DownloadLink = reader.GetAttribute(urlAttribute);
        //                        if (!string.IsNullOrEmpty(currentItem.DownloadLink) && !currentItem.DownloadLink.Contains("/"))
        //                        {
        //                            // Download link contains only the filename -> complete with _castUrl
        //                            currentItem.DownloadLink = _castUrl.Substring(0, _castUrl.LastIndexOf('/') + 1) + currentItem.DownloadLink;
        //                        }

        //                        currentItem.DownloadDSASignature = reader.GetAttribute(dsaSignature);
        //                        string length = reader.GetAttribute(lengthAttribute);
        //                        if (length != null)
        //                        {
        //                            int size = 0;
        //                            if (int.TryParse(length, out size))
        //                            {
        //                                currentItem.UpdateSize = size;
        //                            }
        //                            else
        //                            {
        //                                currentItem.UpdateSize = 0;
        //                            }
        //                        }
        //                        bool isCritical = false;
        //                        string critical = reader.GetAttribute(criticalAttribute);
        //                        if (critical != null && critical == "true" || critical == "1")
        //                        {
        //                            isCritical = true;
        //                        }
        //                        currentItem.IsCriticalUpdate = isCritical;

        //                        string operatingSystem = reader.GetAttribute(operatingSystemAttribute);
        //                        if (operatingSystem != null && operatingSystem != "")
        //                        {
        //                            currentItem.OperatingSystemString = operatingSystem;
        //                        }

        //                        string mimeType = reader.GetAttribute(typeAttribute);
        //                        if (mimeType != null && mimeType != "")
        //                        {
        //                            currentItem.MIMEType = mimeType;
        //                        }
        //                    }
        //                    break;
        //                case pubDateNode:
        //                    if (currentItem != null)
        //                    {
        //                        // "ddd, dd MMM yyyy HH:mm:ss zzz" => Standard date format
        //                        //      e.g. "Sat, 26 Oct 2019 22:05:11 -05:00"
        //                        // "ddd, dd MMM yyyy HH:mm:ss Z" => Check for MS AppCenter Sparkle date format which ends with GMT
        //                        //      e.g. "Sat, 26 Oct 2019 22:05:11 GMT"
        //                        // "ddd, dd MMM yyyy HH:mm:ss" => Standard date format with no timezone (fallback)
        //                        //      e.g. "Sat, 26 Oct 2019 22:05:11"
        //                        string[] formats = { "ddd, dd MMM yyyy HH:mm:ss zzz", "ddd, dd MMM yyyy HH:mm:ss Z", "ddd, dd MMM yyyy HH:mm:ss" };
        //                        string dt = reader.ReadString().Trim();
        //                        if (DateTime.TryParseExact(dt, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
        //                        {
        //                            _logWriter.PrintMessage("Converted '{0}' to {1}.", dt, dateValue);
        //                            currentItem.PublicationDate = dateValue;
        //                        }
        //                        else
        //                        {
        //                            _logWriter.PrintMessage("Cannot parse item datetime {0}", dt);
        //                        }
        //                    }
        //                    break;
        //            }
        //        }
        //        else if (reader.NodeType == XmlNodeType.EndElement)
        //        {
        //            switch (reader.Name)
        //            {
        //                case itemNode:
        //                    Items.Add(currentItem);
        //                    break;
        //            }
        //        }
        //    }

        //    // sort versions in reverse order
        //    Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
        //}

        /// <summary>
        /// Returns sorted list of updates between current and latest. Installed is not included.
        /// </summary>
        public AppCastItem[] GetUpdates()
        {
            Version installed = new Version(_config.InstalledVersion);
            var signatureNeeded = _dsaChecker.SignatureNeeded();

            return Items.Where((item) =>
            {
                // don't allow non-windows updates
                if (!item.IsWindowsUpdate)
                    return false;
                // filter smaller versions
                if (new Version(item.Version).CompareTo(installed) <= 0)
                    return false;
                // filter versions without signature if we need signatures. But accept version without downloads.
                if (signatureNeeded && string.IsNullOrEmpty(item.DownloadDSASignature) && !string.IsNullOrEmpty(item.DownloadLink))
                    return false;
                // accept everything else
                return true;
            }).ToArray();
        }

        public static XDocument GenerateAppCastXml(List<AppCastItem> items, string title, string link = "", string description = "", string language = "en")
        {
            var channel = new XElement("channel");
            channel.Add(new XElement("title", title));

            if (!string.IsNullOrEmpty(link))
                channel.Add(new XElement("link", link));

            if (!string.IsNullOrEmpty(description))
                channel.Add(new XElement("description", description));

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
