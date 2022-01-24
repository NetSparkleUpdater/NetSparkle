using NetSparkleUpdater.AppCastHandlers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Console = Colorful.Console;

namespace NetSparkleUpdater.AppCastGenerator
{
    public class XMLAppCastMaker : AppCastMaker
    {
        public XMLAppCastMaker(SignatureManager signatureManager, Options options) : base(signatureManager, options)
        {
        }

        public override string GetAppCastExtension()
        {
            return "xml";
        }

        public override (List<AppCastItem>, string) GetItemsAndProductNameFromExistingAppCast(string appCastFileName)
        {
            Console.WriteLine("Parsing existing app cast at {0}...", appCastFileName);
            var items = new List<AppCastItem>();
            string productName = null;
            if (!File.Exists(appCastFileName))
            {
                Console.WriteLine("App cast does not exist at {0}, so creating it anew...", appCastFileName, Color.Red);
            }
            else
            {
                XDocument doc = XDocument.Parse(File.ReadAllText(appCastFileName));
                // for any .xml file, there is a product name - we can pull this out automatically when there is just one channel.
                List<XElement> allTitles = doc.Root?.Element("channel")?.Elements("title")?.ToList() ?? new List<XElement>();
                if (allTitles.Count == 1 && !string.IsNullOrWhiteSpace(allTitles[0].Value))
                {
                    productName = allTitles[0].Value;
                    Console.WriteLine("Using title in app cast: {0}...", productName, Color.LightBlue);
                }

                var docDescendants = doc.Descendants("item");
                var logWriter = new LogWriter(true);
                foreach (var item in docDescendants)
                {
                    var currentItem = AppCastItem.Parse("", "", "/", item, logWriter);
                    Console.WriteLine("Found an item in the app cast: version {0} ({1}) -- os = {2}",
                        currentItem?.Version, currentItem?.ShortVersion, currentItem.OperatingSystemString);
                    items.Add(currentItem);
                }
            }
            return (items, productName);
        }

        public override void SerializeItemsToFile(List<AppCastItem> items, string applicationTitle, string path)
        {
            var appcastXmlDocument = XMLAppCast.GenerateAppCastXml(items, applicationTitle);
            Console.WriteLine("Writing app cast to {0}", path);
            using (var xmlWriter = XmlWriter.Create(path, new XmlWriterSettings { NewLineChars = "\n", Encoding = new UTF8Encoding(false) }))
            {
                appcastXmlDocument.Save(xmlWriter);
            }
        }
    }
}
