using NetSparkle.Enums;
using NetSparkle.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace NetSparkle
{
    /// <summary>
    /// An XML-based appcast document downloader and handler
    /// </summary>
    public class XMLAppCast : IAppCastHandler
    {
        private Configuration _config;
        private string _castUrl;

        private DSAChecker _dsaChecker;
        private LogWriter _logWriter;

        private IAppCastDataDownloader _dataDownloader;

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
        public XMLAppCast()
        {
            Items = new List<AppCastItem>();
        }

        /// <summary>
        /// TODO: docs
        /// </summary>
        /// <param name="castUrl">the URL of the appcast file</param>
        /// <param name="trustEverySSLConnection">whether or not to trust every SSL connection</param>
        /// <param name="config">the current configuration</param>
        /// <param name="dsaChecker">class to verify that DSA hashes are accurate</param>
        /// <param name="logWriter">object to write any log statements to</param>
        /// <param name="extraJSON">string representation of JSON object to send along with the appcast request. nullable.</param>
        public void SetupAppCastHandler(IAppCastDataDownloader dataDownloader, string castUrl, Configuration config, DSAChecker dsaChecker, LogWriter logWriter = null)
        {
            _dataDownloader = dataDownloader;
            _config = config;
            _castUrl = castUrl;

            _dsaChecker = dsaChecker;
            _logWriter = logWriter ?? new LogWriter();
        }

        private string TryReadAppCastSignature()
        {
            try
            {
                var signaturestream = _dataDownloader.DownloadAndGetContentStream(_castUrl + ".dsa");
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
        /// Download castUrl resource and parse it
        /// </summary>
        public bool DownloadAndParse()
        {
            try
            {
                var inputstream = _dataDownloader.DownloadAndGetContentStream(_castUrl);
                var signature = TryReadAppCastSignature();
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
            if (signatureNeeded && _dsaChecker.VerifyDSASignature(signature, _dsaChecker.ConvertStreamToByteArray(memorystream)) == ValidationResult.Invalid)
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
        public virtual List<AppCastItem> GetNeededUpdates()
        {
            Version installed = new Version(_config.InstalledVersion);
            var signatureNeeded = _dsaChecker.SignatureNeeded();
            return Items.Where((item) =>
            {
#if NETFRAMEWORK
                // don't allow non-windows updates
                if (!item.IsWindowsUpdate)
                {
                    return false;
                }
#else
                // check operating system and filter out ones that don't match the current
                // operating system
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !item.IsWindowsUpdate)
                {
                    return false;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !item.IsMacOSUpdate)
                {
                    return false;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !item.IsLinuxUpdate)
                {
                    return false;
                }
#endif
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
