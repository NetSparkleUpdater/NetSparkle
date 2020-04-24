using NetSparkleUpdater.Configurations;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <summary>
    /// An XML-based appcast document downloader and handler
    /// </summary>
    public class XMLAppCast : IAppCastHandler
    {
        private Configuration _config;
        private string _castUrl;

        private ISignatureVerifier _signatureVerifier;
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
        /// Sets the app cast handler up with its needed objects/parameters so that it can download
        /// and validate an appcast
        /// </summary>
        /// <param name="dataDownloader">object that will handle downloading the app cast file(s) from the internet</param>
        /// <param name="castUrl">the URL of the appcast file</param>
        /// <param name="config">the current configuration for checking which versions are installed, etc.</param>
        /// <param name="signatureVerifier">class to verify that DSA hashes are accurate</param>
        /// <param name="logWriter">object to write any log statements to</param>
        public void SetupAppCastHandler(IAppCastDataDownloader dataDownloader, string castUrl, Configuration config, ISignatureVerifier signatureVerifier, LogWriter logWriter = null)
        {
            _dataDownloader = dataDownloader;
            _config = config;
            _castUrl = castUrl;

            _signatureVerifier = signatureVerifier;
            _logWriter = logWriter ?? new LogWriter();
        }

        /// <summary>
        /// Download castUrl resource and parse it
        /// </summary>
        public bool DownloadAndParse()
        {
            try
            {
                _logWriter.PrintMessage("Downloading app cast data...");
                var appcast = _dataDownloader.DownloadAndGetAppCastData(_castUrl);
                var signatureNeeded = Utilities.IsSignatureNeeded(_signatureVerifier.SecurityMode, _signatureVerifier.HasValidKeyInformation());
                bool isValidAppcast = true;
                if (signatureNeeded)
                {
                    _logWriter.PrintMessage("Downloading app cast signature data...");
                    var signature = _dataDownloader.DownloadAndGetAppCastData(_castUrl + ".dsa");
                    isValidAppcast = VerifyAppCast(appcast, signature);
                }
                if (isValidAppcast)
                {
                    _logWriter.PrintMessage("Appcast is valid! Parsing...");
                    ParseAppCast(appcast);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logWriter.PrintMessage("Error reading app cast {0}: {1} ", _castUrl, e.Message);
            }
            _logWriter.PrintMessage("Appcast is not valid");
            return false;
        }

        private bool VerifyAppCast(string appcast, string signature)
        {
            if (appcast == null)
            {
                _logWriter.PrintMessage("Cannot read response from URL {0}", _castUrl);
                return false;
            }

            // checking signature
            var signatureNeeded = Utilities.IsSignatureNeeded(_signatureVerifier.SecurityMode, _signatureVerifier.HasValidKeyInformation());
            var appcastBytes = _dataDownloader.GetAppCastEncoding().GetBytes(appcast);
            if (signatureNeeded && _signatureVerifier.VerifySignature(signature, appcastBytes) == ValidationResult.Invalid)
            {
                _logWriter.PrintMessage("Signature check of appcast failed");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parse an XML memory stream build items list
        /// </summary>
        /// <param name="stream">The xml memory stream to parse</param>
        private void ParseAppCast(string appcast)
        {
            const string itemNode = "item";
            Items.Clear();

            XDocument doc = XDocument.Parse(appcast);
            var rss = doc?.Element("rss");
            var channel = rss?.Element("channel");

            Title = channel?.Element("title")?.Value ?? string.Empty;
            Language = channel?.Element("language")?.Value ?? "en";

            var items = doc.Descendants(itemNode);
            foreach (var item in items)
            {
                var currentItem = AppCastItem.Parse(_config.InstalledVersion, _config.ApplicationName, _castUrl, item, _logWriter);
                _logWriter.PrintMessage("Found an item in the app cast: version {0} ({1}) -- os = {2}", 
                    currentItem?.Version, currentItem?.ShortVersion, currentItem.OperatingSystemString);
                Items.Add(currentItem);
            }

            // sort versions in reverse order
            Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
        }

        /// <summary>
        /// Returns sorted list of updates between current and latest. Installed is not included.
        /// </summary>
        public virtual List<AppCastItem> GetAvailableUpdates()
        {
            Version installed = new Version(_config.InstalledVersion);
            var signatureNeeded = Utilities.IsSignatureNeeded(_signatureVerifier.SecurityMode, _signatureVerifier.HasValidKeyInformation());
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
