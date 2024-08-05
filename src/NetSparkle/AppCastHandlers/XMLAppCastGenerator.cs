using NetSparkleUpdater;
using NetSparkleUpdater.Configurations;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;
using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <summary>
    /// An XML-based app cast document downloader and handler
    /// </summary>
    public class XMLAppCastGenerator : IAppCastGenerator
    {
        private ILogger? _logWriter;

        /// <summary>
        /// Sparkle XML namespace
        /// </summary>
        public static readonly XNamespace SparkleNamespace = "http://www.andymatuschak.org/xml-namespaces/sparkle";
        
        /// <summary>
        /// Default XML attribute name for ed25519 signatures
        /// </summary>
        public static readonly string Ed25519SignatureAttribute = "edSignature";
        
        /// <summary>
        /// Default XML attribute name for a signature for an unspecified algorithm
        /// </summary>
        public static readonly string SignatureAttribute = "signature";

        /// <summary>
        /// An app cast generator that reads/writes XML
        /// </summary>
        /// <param name="logger">Optional <seealso cref="ILogger"/> for logging data</param>
        public XMLAppCastGenerator(ILogger? logger = null)
        {
            _logWriter = logger;
            HumanReadableOutput = true;
            OutputSignatureAttribute = SignatureAttribute;
        }

        /// <summary>
        /// Set to true to make serialized output human readable (newlines, indents) when written to a file.
        /// Set to false to make this output not necessarily human readable.
        /// Defaults to true.
        /// </summary>
        public bool HumanReadableOutput { get; set; }

        /// <summary>
        /// Output attribute title for signatures. Defaults to
        /// <seealso cref="SignatureAttribute"/>.
        /// </summary>
        public string OutputSignatureAttribute { get; set; }

        /// <inheritdoc/>
        public AppCast DeserializeAppCast(string appCastString)
        {
            var appCast = new AppCast();
            if (!string.IsNullOrWhiteSpace(appCastString))
            {
                XDocument doc = XDocument.Parse(appCastString);
                var rss = doc?.Element("rss");
                var channel = rss?.Element("channel");

                appCast.Title = channel?.Element("title")?.Value ?? string.Empty;
                appCast.Language = channel?.Element("language")?.Value ?? "en";
                appCast.Link = channel?.Element("link")?.Value ?? "";
                appCast.Description = channel?.Element("description")?.Value ?? "Most recent changes with links to updates";

                var docItems = doc?.Descendants("item");
                if (docItems != null)
                {
                    foreach (var item in docItems)
                    {
                        var currentItem = ReadAppCastItem(item);
                        _logWriter?.PrintMessage("Found an item in the app cast: version {0} ({1}) -- os = {2}", 
                            currentItem.Version ?? "[Unknown version]", currentItem.ShortVersion ?? "[Unknown short version]", currentItem.OperatingSystem ?? "[Unknown operating system]");
                        appCast.Items.Add(currentItem);
                    }
                }
                // sort versions in reverse order
                appCast.Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
            }
            return appCast;
        }

        /// <inheritdoc/>
        public async Task<AppCast> DeserializeAppCastAsync(string appCastString)
        {
            return await Task.Run(() => DeserializeAppCast(appCastString));
        }

        /// <inheritdoc/>
        public AppCast DeserializeAppCastFromFile(string filePath)
        {
            return DeserializeAppCast(File.ReadAllText(filePath));
        }

        /// <inheritdoc/>
        public async Task<AppCast> DeserializeAppCastFromFileAsync(string filePath)
        {
#if NETFRAMEWORK || NETSTANDARD
            var data = await Utilities.ReadAllTextAsync(filePath);
#else
            var data = await File.ReadAllTextAsync(filePath);
#endif
            return await Task.Run(() => DeserializeAppCast(data));
        }

        // https://stackoverflow.com/a/955698/3938401
        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }

        /// <summary>
        /// Get default settings for writing XMl
        /// </summary>
        /// <returns>Default XML writing settings</returns>
        protected XmlWriterSettings GetXmlWriterSettings()
        {
            return new XmlWriterSettings()
            {
                NewLineChars = Environment.NewLine, 
                Encoding = new UTF8Encoding(false), 
                Indent = HumanReadableOutput
            };
        }

        /// <inheritdoc/>
        public string SerializeAppCast(AppCast appCast)
        {
            var doc = GenerateAppCastXml(appCast.Items, appCast.Title, appCast.Link, appCast.Description, appCast.Language);
            var settings = GetXmlWriterSettings();
            using (TextWriter writer = new Utf8StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings))
                {
                    doc.Save(writer);
                }
                return writer.ToString() ?? "";
            }
        }

        /// <inheritdoc/>
        public async Task<string> SerializeAppCastAsync(AppCast appCast)
        {
            var doc = GenerateAppCastXml(appCast.Items, appCast.Title, appCast.Link, appCast.Description, appCast.Language);
            var settings = GetXmlWriterSettings();
            using (TextWriter writer = new Utf8StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings))
                {
#if NETFRAMEWORK || NETSTANDARD
                    await Task.Run(() => doc.Save(writer));
#else
                    var cancelTokenSource = new CancellationTokenSource();
                    var cancelToken = cancelTokenSource.Token;
                    await doc.SaveAsync(xmlWriter, cancelToken);
#endif
                }
                return writer.ToString() ?? "";
            }
        }

        /// <inheritdoc/>
        public void SerializeAppCastToFile(AppCast appCast, string outputPath)
        {
            File.WriteAllText(outputPath, SerializeAppCast(appCast));
        }

        /// <inheritdoc/>
        public async Task SerializeAppCastToFileAsync(AppCast appCast, string outputPath)
        {
            string xml = await SerializeAppCastAsync(appCast);
#if NETFRAMEWORK || NETSTANDARD
            await Utilities.WriteTextAsync(outputPath, xml);
#else
            await File.WriteAllTextAsync(outputPath, xml);
#endif
        }

        private const string _itemNode = "item";
        private const string _titleNode = "title";
        private const string _enclosureNode = "enclosure";
        private const string _releaseNotesLinkNode = "releaseNotesLink";
        private const string _descriptionNode = "description";
        private const string _versionAttribute = "version";
        private const string _shortVersionAttribute = "shortVersionString";
        private const string _dsaSignatureAttribute = "dsaSignature";
        private const string _criticalAttribute = "criticalUpdate";
        private const string _operatingSystemAttribute = "os";
        private const string _lengthAttribute = "length";
        private const string _typeAttribute = "type";
        private const string _urlAttribute = "url";
        private const string _pubDateNode = "pubDate";

        /// <summary>
        /// Parse item Xml Node to AppCastItem
        /// </summary>
        /// <param name="item">The item XML node</param>
        /// <returns>AppCastItem from Xml Node</returns>
        public AppCastItem ReadAppCastItem(XElement item)
        {
            var newAppCastItem = new AppCastItem()
            {
                UpdateSize = 0,
                IsCriticalUpdate = false,
                Title = item.Element(_titleNode)?.Value ?? string.Empty
            };

            //release notes
            var releaseNotesElement = item.Element(SparkleNamespace + _releaseNotesLinkNode);
            newAppCastItem.ReleaseNotesSignature = releaseNotesElement?.Attribute(SparkleNamespace + SignatureAttribute)?.Value ?? string.Empty;
            if (newAppCastItem.ReleaseNotesSignature == string.Empty)
            {
                newAppCastItem.ReleaseNotesSignature = releaseNotesElement?.Attribute(SparkleNamespace + _dsaSignatureAttribute)?.Value ?? string.Empty;
            }
            if (newAppCastItem.ReleaseNotesSignature == string.Empty)
            {
                newAppCastItem.ReleaseNotesSignature = releaseNotesElement?.Attribute(SparkleNamespace + Ed25519SignatureAttribute)?.Value ?? string.Empty;
            }
            newAppCastItem.ReleaseNotesLink = releaseNotesElement?.Value.Trim() ?? string.Empty;

            //description
            newAppCastItem.Description = item.Element(_descriptionNode)?.Value.Trim() ?? string.Empty;

            //enclosure
            var enclosureElement = item.Element(_enclosureNode) ?? item.Element(SparkleNamespace + _enclosureNode);

            newAppCastItem.Version = enclosureElement?.Attribute(SparkleNamespace + _versionAttribute)?.Value ?? string.Empty;
            newAppCastItem.ShortVersion = enclosureElement?.Attribute(SparkleNamespace + _shortVersionAttribute)?.Value ?? string.Empty;
            newAppCastItem.DownloadLink = enclosureElement?.Attribute(_urlAttribute)?.Value ?? string.Empty;
            newAppCastItem.DownloadLink = newAppCastItem.DownloadLink;
            //if (!string.IsNullOrWhiteSpace(newAppCastItem.DownloadLink) && !newAppCastItem.DownloadLink.Contains("/"))
            //{
            //    // Download link contains only the filename -> complete with _castUrl
            //    if (castUrl == null && !string.IsNullOrWhiteSpace(castUrl))
            //    {
            //        newAppCastItem.DownloadLink = newAppCastItem.DownloadLink;
            //    }
            //    else
            //    {
            //        newAppCastItem.DownloadLink = castUrl.Substring(0, castUrl.LastIndexOf('/') + 1) + newAppCastItem.DownloadLink;
            //    }
            //}

            newAppCastItem.DownloadSignature = enclosureElement?.Attribute(SparkleNamespace + SignatureAttribute)?.Value ?? string.Empty;
            if (newAppCastItem.DownloadSignature == string.Empty)
            {
                newAppCastItem.DownloadSignature = enclosureElement?.Attribute(SparkleNamespace + _dsaSignatureAttribute)?.Value ?? string.Empty;
            }
            if (newAppCastItem.DownloadSignature == string.Empty)
            {
                newAppCastItem.DownloadSignature = enclosureElement?.Attribute(SparkleNamespace + Ed25519SignatureAttribute)?.Value ?? string.Empty;
            }
            string length = enclosureElement?.Attribute(_lengthAttribute)?.Value ?? string.Empty;
            if (length != null)
            {
                if (long.TryParse(length, out var size))
                {
                    newAppCastItem.UpdateSize = size;
                }
                else
                {
                    newAppCastItem.UpdateSize = 0;
                }
            }
            bool isCritical = false;
            string critical = enclosureElement?.Attribute(SparkleNamespace + _criticalAttribute)?.Value ?? string.Empty;
            if (critical != null && critical == "true" || critical == "1")
            {
                isCritical = true;
            }
            newAppCastItem.IsCriticalUpdate = isCritical;

            var osAttribute = enclosureElement?.Attribute(SparkleNamespace + _operatingSystemAttribute);
            if (!string.IsNullOrWhiteSpace(osAttribute?.Value))
            {
                newAppCastItem.OperatingSystem = osAttribute?.Value;
            }
            var mimeTypeAttribute = enclosureElement?.Attribute(SparkleNamespace + _typeAttribute);
            if (mimeTypeAttribute?.Value != null && !string.IsNullOrWhiteSpace(mimeTypeAttribute.Value))
            {
                newAppCastItem.MIMEType = mimeTypeAttribute.Value;
            }

            //pub date
            var pubDateElement = item.Element(_pubDateNode);
            if (pubDateElement != null)
            {
                // "ddd, dd MMM yyyy HH:mm:ss zzz" => Standard date format
                //      e.g. "Sat, 26 Oct 2019 22:05:11 -05:00"
                // "ddd, dd MMM yyyy HH:mm:ss Z" => Check for MS AppCenter Sparkle date format which ends with GMT
                //      e.g. "Sat, 26 Oct 2019 22:05:11 GMT"
                // "ddd, dd MMM yyyy HH:mm:ss" => Standard date format with no timezone (fallback)
                //      e.g. "Sat, 26 Oct 2019 22:05:11"
                string[] formats = { "ddd, dd MMM yyyy HH:mm:ss zzz", "ddd, dd MMM yyyy HH:mm:ss Z", 
                    "ddd, dd MMM yyyy HH:mm:ss", "ddd, dd MMM yyyy HH:mm:ss K" };
                string dt = pubDateElement.Value.Trim();
                if (DateTime.TryParseExact(dt, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
                {
                    _logWriter?.PrintMessage("While parsing app cast item, converted '{0}' to {1}.", dt, dateValue);
                    newAppCastItem.PublicationDate = dateValue;
                }
                else
                {
                    _logWriter?.PrintMessage("Cannot parse item's DateTime: {0}", dt);
                }
            }

            return newAppCastItem;
        }

        /// <summary>
        /// Create XML node from this instance of AppCastItem
        /// </summary>
        /// <returns>An XML node</returns>
        public static XElement GetXElement(AppCastItem appCastItem, string signatureAttribute = "")
        {
            if (string.IsNullOrWhiteSpace(signatureAttribute))
            {
                signatureAttribute = SignatureAttribute;
            }

            var item = new XElement(_itemNode);
            item.Add(new XElement(_titleNode) { Value = appCastItem.Title ?? "" });

            if (!string.IsNullOrWhiteSpace(appCastItem.ReleaseNotesLink))
            {
                var releaseNotes = new XElement(SparkleNamespace + _releaseNotesLinkNode) { Value = appCastItem.ReleaseNotesLink };
                if (!string.IsNullOrWhiteSpace(appCastItem.ReleaseNotesSignature))
                {
                    releaseNotes.Add(new XAttribute(SparkleNamespace + signatureAttribute, appCastItem.ReleaseNotesSignature));
                }
                item.Add(releaseNotes);
            }

            if (!string.IsNullOrWhiteSpace(appCastItem.Description))
            {
                item.Add(new XElement(_descriptionNode) { Value = appCastItem.Description });
            }

            if (appCastItem.PublicationDate != DateTime.MinValue && appCastItem.PublicationDate != DateTime.MaxValue)
            {
                item.Add(new XElement(_pubDateNode) { Value = appCastItem.PublicationDate.ToString("ddd, dd MMM yyyy HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture) });
            }

            if (!string.IsNullOrWhiteSpace(appCastItem.DownloadLink))
            {
                var enclosure = new XElement(_enclosureNode);
                enclosure.Add(new XAttribute(_urlAttribute, appCastItem.DownloadLink));
                if (!string.IsNullOrWhiteSpace(appCastItem.Version))
                {
                    enclosure.Add(new XAttribute(SparkleNamespace + _versionAttribute, appCastItem.Version));
                }

                if (!string.IsNullOrWhiteSpace(appCastItem.ShortVersion))
                {
                    enclosure.Add(new XAttribute(SparkleNamespace + _shortVersionAttribute, appCastItem.ShortVersion));
                }

                enclosure.Add(new XAttribute(_lengthAttribute, appCastItem.UpdateSize));
                enclosure.Add(new XAttribute(SparkleNamespace + _operatingSystemAttribute, appCastItem.OperatingSystem ?? AppCastItem.DefaultOperatingSystem));
                enclosure.Add(new XAttribute(_typeAttribute, appCastItem.MIMEType ?? AppCastItem.DefaultMIMEType));
                enclosure.Add(new XAttribute(SparkleNamespace + _criticalAttribute, appCastItem.IsCriticalUpdate));

                // enhance compatibility with Sparkle app casts (#275)
                item.Add(new XElement(SparkleNamespace + _versionAttribute, appCastItem.Version));
                item.Add(new XElement(SparkleNamespace + _shortVersionAttribute, appCastItem.ShortVersion));
                if (appCastItem.IsCriticalUpdate)
                {
                    item.Add(new XElement(SparkleNamespace + _criticalAttribute));
                }

                if (!string.IsNullOrWhiteSpace(appCastItem.DownloadSignature))
                {
                    enclosure.Add(new XAttribute(SparkleNamespace + signatureAttribute, appCastItem.DownloadSignature));
                }
                item.Add(enclosure);
            }
            return item;
        }

        /// <summary>
        /// Create app cast XML document as an <seealso cref="XDocument"/> object
        /// </summary>
        /// <param name="appCastItems">The <seealso cref="AppCastItem"/> list to include in the output file</param>
        /// <param name="title">Application title/title for the app cast</param>
        /// <param name="link">Link to the where the app cast is going to be downloaded</param>
        /// <param name="description">Text that describes the app cast (e.g. what it provides)</param>
        /// <param name="language">Language of the app cast file</param>
        /// <returns>An <seealso cref="XDocument"/> xml document that describes the list of passed in update items</returns>
        public XDocument GenerateAppCastXml(List<AppCastItem> appCastItems, string? title, string? link = "", string? description = "", string? language = "en")
        {
            var channel = new XElement("channel");
            channel.Add(new XElement("title", title));

            if (!string.IsNullOrWhiteSpace(link))
            {
                channel.Add(new XElement("link", link));
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                channel.Add(new XElement("description", description));
            }

            channel.Add(new XElement("language", language));

            foreach (var item in appCastItems)
            {
                channel.Add(GetXElement(item, OutputSignatureAttribute));
            }

            var document = new XDocument(
                new XElement("rss", new XAttribute("version", "2.0"), new XAttribute(XNamespace.Xmlns + "sparkle", SparkleNamespace),
                    channel)
            );

            return document;
        }
    }
}