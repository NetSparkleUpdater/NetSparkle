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
        private ILogger _logWriter;

        private IAppCastDataDownloader _dataDownloader;

        /// <summary>
        /// Sparkle XML namespace
        /// </summary>
        public static readonly XNamespace SparkleNamespace = "http://www.andymatuschak.org/xml-namespaces/sparkle";

        /// <summary>
        /// App cast title (usually the name of the application)
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// App cast language (e.g. "en")
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Extension (WITHOUT the "." at the start) for the signature
        /// file. Defaults to "signature".
        /// </summary>
        public string SignatureFileExtension { get; set; }

        /// <summary>
        /// List of <seealso cref="AppCastItem"/> that were parsed in the app cast
        /// </summary>
        public readonly List<AppCastItem> Items;

        /// <summary>
        /// Create a new object with an empty list of <seealso cref="AppCastItem"/> items
        /// </summary>
        public XMLAppCast()
        {
            Items = new List<AppCastItem>();
            SignatureFileExtension = "signature";
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
        public void SetupAppCastHandler(IAppCastDataDownloader dataDownloader, string castUrl, Configuration config, ISignatureVerifier signatureVerifier, ILogger logWriter = null)
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
                    var extension = SignatureFileExtension?.TrimStart('.') ?? "signature";
                    try
                    {
                        signature = _dataDownloader.DownloadAndGetAppCastData(_castUrl + "." + extension);
                    }
                    catch (Exception e)
                    {
                        _logWriter.PrintMessage("Error reading app cast {0}.{2}: {1} ", _castUrl, e.Message, extension);
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

        private bool VerifyAppCast(string appCast, string signature)
        {
            if (string.IsNullOrWhiteSpace(appCast))
            {
                _logWriter.PrintMessage("Cannot read response from URL {0}", _castUrl);
                return false;
            }

            // checking signature
            var signatureNeeded = Utilities.IsSignatureNeeded(_signatureVerifier.SecurityMode, _signatureVerifier.HasValidKeyInformation(), false);
            var appcastBytes = _dataDownloader.GetAppCastEncoding().GetBytes(appCast);
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
        /// <param name="appCast">the non-null string XML app cast</param>
        protected virtual void ParseAppCast(string appCast)
        {
            const string itemNode = "item";
            Items.Clear();

            XDocument doc = XDocument.Parse(appCast);
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
        /// Returns sorted list of updates between current installed version and latest version in <see cref="Items"/>. 
        /// Currently installed version is NOT included in the output.
        /// </summary>
        /// <returns>A list of <seealso cref="AppCastItem"/> updates that could be installed</returns>
        public virtual List<AppCastItem> GetAvailableUpdates()
        {
            Version installed = new Version(_config.InstalledVersion);
            var signatureNeeded = Utilities.IsSignatureNeeded(_signatureVerifier.SecurityMode, _signatureVerifier.HasValidKeyInformation(), false);
            _logWriter.PrintMessage("Looking for available updates; our installed version is {0}; do we need a signature? {1}", installed, signatureNeeded);
            return Items.Where((item) =>
            {
#if NETFRAMEWORK
                // don't allow non-windows updates
                if (!item.IsWindowsUpdate)
                {
                    _logWriter.PrintMessage("Rejecting update for {0} ({1}, {2}) because it isn't a Windows update and we're on Windows", item.Version, 
                        item.ShortVersion, item.Title);
                    return false;
                }
#else
                // check operating system and filter out ones that don't match the current
                // operating system
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !item.IsWindowsUpdate)
                {
                    _logWriter.PrintMessage("Rejecting update for {0} ({1}, {2}) because it isn't a Windows update and we're on Windows", item.Version, 
                        item.ShortVersion, item.Title);
                    return false;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !item.IsMacOSUpdate)
                {
                    _logWriter.PrintMessage("Rejecting update for {0} ({1}, {2}) because it isn't a macOS update and we're on macOS", item.Version,
                        item.ShortVersion, item.Title);
                    return false;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !item.IsLinuxUpdate)
                {
                    _logWriter.PrintMessage("Rejecting update for {0} ({1}, {2}) because it isn't a Linux update and we're on Linux", item.Version,
                        item.ShortVersion, item.Title);
                    return false;
                }
#endif
                // filter smaller versions
                if (new Version(item.Version).CompareTo(installed) <= 0)
                {
                    _logWriter.PrintMessage("Rejecting update for {0} ({1}, {2}) because it is older than our current version of {3}", item.Version,
                        item.ShortVersion, item.Title, installed);
                    return false;
                }
                // filter versions without signature if we need signatures. But accept version without downloads.
                if (signatureNeeded && string.IsNullOrEmpty(item.DownloadSignature) && !string.IsNullOrEmpty(item.DownloadLink))
                {
                    _logWriter.PrintMessage("Rejecting update for {0} ({1}, {2}) because it we needed a DSA/other signature and " +
                        "the item has no signature yet has a download link of {3}", item.Version,
                        item.ShortVersion, item.Title, item.DownloadLink);
                    return false;
                }
                // accept everything else
                _logWriter.PrintMessage("Item with version {0} ({1}) is a valid update! It can be downloaded at {2}", item.Version,
                    item.ShortVersion, item.DownloadLink);
                return true;
            }).ToList();
        }

        /// <summary>
        /// Create app cast XML document as an <seealso cref="XDocument"/> object
        /// </summary>
        /// <param name="items">The <seealso cref="AppCastItem"/> list to include in the output file</param>
        /// <param name="title">Application title/title for the app cast</param>
        /// <param name="link">Link to the where the app cast is going to be downloaded</param>
        /// <param name="description">Text that describes the app cast (e.g. what it provides)</param>
        /// <param name="language">Language of the app cast file</param>
        /// <returns>An <seealso cref="XDocument"/> xml document that describes the list of passed in update items</returns>
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
