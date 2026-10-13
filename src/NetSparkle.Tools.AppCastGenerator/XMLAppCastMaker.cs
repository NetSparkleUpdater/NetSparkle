using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Console = Colorful.Console;

namespace NetSparkleUpdater.AppCastGenerator
{
    public class XMLAppCastMaker : AppCastMaker
    {
        /// <summary>
        /// Sparkle XML namespace
        /// </summary>
        public static readonly XNamespace SparkleNamespace = "http://www.andymatuschak.org/xml-namespaces/sparkle";

        public XMLAppCastMaker(SignatureManager signatureManager, Options options) : base(signatureManager, options)
        {
            HumanReadableOutput = options.HumanReadableOutput;
        }

        /// <summary>
        /// True if output should be human readable (indents, newslines). 
        /// False by default.
        /// </summary>
        public bool HumanReadableOutput { get; set; }

        /// <inheritdoc/>
        public override string GetAppCastExtension()
        {
            return "xml";
        }

        /// <inheritdoc/>
        public override (List<AppCastItem>, string?) GetItemsAndProductNameFromExistingAppCast(Stream stream, bool overwriteOldItemsInAppcast)
        {
            Console.WriteLine("Parsing existing app cast...");
            var items = new List<AppCastItem>();
            string? productName = null;
            try
            {
                using var reader = new StreamReader(stream);
                string data = reader.ReadToEnd();

                XDocument doc = XDocument.Parse(data);
                // for any .xml file, there is a product name - we can pull this out automatically when there is just one channel.
                List<XElement> allTitles = doc.Root?.Element("channel")?.Elements("title")?.ToList() ?? new List<XElement>();
                if (allTitles.Count == 1 && !string.IsNullOrWhiteSpace(allTitles[0].Value))
                {
                    productName = allTitles[0].Value;
                    Console.WriteLine("Using title in app cast: {0}...", productName, Color.LightBlue);
                }

                var docDescendants = doc.Descendants("item");
                var logWriter = new LogWriter(LogWriterOutputMode.Console);
                var xmlGenerator = new XMLAppCastGenerator(logWriter);
                foreach (var item in docDescendants)
                {
                    var currentItem = xmlGenerator.ReadAppCastItem(item);
                    Console.WriteLine("Found an item in the app cast: version {0} ({1}) -- os = {2}",
                        currentItem.Version, currentItem.ShortVersion, currentItem.OperatingSystem);
                    var itemFound = items.Where(x => x.Version != null && x.Version == currentItem.Version?.Trim()
                        && x.OperatingSystem == currentItem.OperatingSystem?.Trim()).FirstOrDefault();
                    if (itemFound == null)
                    {
                        items.Add(currentItem);
                    }
                    else
                    {
                        Console.WriteLine($"Duplicate item with version {currentItem.Version} found in app cast. This is likely an invalid state" +
                            $" and you should fix your app cast so that it does not have duplicate items.", Color.Yellow);
                        if (overwriteOldItemsInAppcast)
                        {
                            items.Remove(itemFound); // remove old item.
                            items.Add(currentItem);
                            Console.WriteLine("Overwriting old item with newly found one...", Color.Yellow);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading previous app cast: {e.Message}. Not using it for any items...", Color.Red);
                return (new List<AppCastItem>(), null);
            }
            items.Sort((a, b) =>
            {
                if (a.Version == null && b.Version == null)
                {
                    return 0;
                }
                if (a.Version != null && b.Version == null)
                {
                    return -1;
                }
                if (a.Version == null && b.Version != null)
                {
                    return 1;
                }
                return b.Version?.CompareTo(a.Version) ?? 0;
            });
            return (items, productName);
        }

        /// <inheritdoc/>
        public override void SerializeItems(List<AppCastItem> items, string applicationTitle, Stream stream)
        {
            var xmlGenerator = new XMLAppCastGenerator()
            {
                HumanReadableOutput = HumanReadableOutput,
                OutputSignatureAttribute = _opts.UseEd25519SignatureAttributeForXml
                    ? XMLAppCastGenerator.Ed25519SignatureAttribute
                    : XMLAppCastGenerator.SignatureAttribute
            };
            var appCast = new AppCast()
            {
                Items = items,
                Title = applicationTitle,
                Link = _opts.AppCastLink,
                Description = _opts.AppCastDescription,
                Language = "en"
            };
            xmlGenerator.SerializeAppCast(appCast, stream);
        }
    }
}
