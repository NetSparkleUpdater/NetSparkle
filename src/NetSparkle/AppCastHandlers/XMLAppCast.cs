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
        /// Setup the app cast handler info for downloading and parsing app cast information
        /// </summary>
        /// <param name="dataDownloader">downloader that will manage the app cast download 
        /// (provided by <see cref="SparkleUpdater"/> via the 
        /// <see cref="SparkleUpdater.AppCastDataDownloader"/> property.</param>
        /// <param name="castUrl">full URL to the app cast file</param>
        /// <param name="config">configuration for handling update intervals/checks 
        /// (user skipped versions, etc.)</param>
        /// <param name="signatureVerifier">Object to check signatures of app cast information</param>
        /// <param name="logWriter">object that you can utilize to do any necessary logging</param>
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
                var signatureNeeded = Utilities.IsSignatureNeeded(_signatureVerifier.SecurityMode, _signatureVerifier.HasValidKeyInformation(), false);
                bool isValidAppcast = true;
                if (signatureNeeded)
                {
                    _logWriter.PrintMessage("Downloading app cast signature data...");
                    var signature = "";
                    try
                    {
                        signature = _dataDownloader.DownloadAndGetAppCastData(_castUrl + ".signature");
                    }
                    catch (Exception e)
                    {
                        _logWriter.PrintMessage("Error reading app cast {0}.signature: {1} ", _castUrl, e.Message);
                    }
                    if (string.IsNullOrWhiteSpace(signature))
                    {
                        // legacy: check for .dsa file
                        try
                        {
                            signature = _dataDownloader.DownloadAndGetAppCastData(_castUrl + ".dsa");
                        }
                        catch (Exception e)
                        {
                            _logWriter.PrintMessage("Error reading app cast {0}.dsa: {1} ", _castUrl, e.Message);
                        }
                    }
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
            var signatureNeeded = Utilities.IsSignatureNeeded(_signatureVerifier.SecurityMode, _signatureVerifier.HasValidKeyInformation(), false);
            var appcastBytes = _dataDownloader.GetAppCastEncoding().GetBytes(appcast);
            if (signatureNeeded && _signatureVerifier.VerifySignature(signature, appcastBytes) == ValidationResult.Invalid)
            {
                _logWriter.PrintMessage("Signature check of appcast failed");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parse the app cast XML string into a list of <see cref="AppCastItem"/> objects.
        /// When complete, the Items list should contain the parsed information
        /// as <see cref="AppCastItem"/> objects.
        /// </summary>
        /// <param name="appcast">the non-null string XML app cast</param>
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
            var signatureNeeded = Utilities.IsSignatureNeeded(_signatureVerifier.SecurityMode, _signatureVerifier.HasValidKeyInformation(), false);
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
                if (signatureNeeded && string.IsNullOrEmpty(item.DownloadSignature) && !string.IsNullOrEmpty(item.DownloadLink))
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
