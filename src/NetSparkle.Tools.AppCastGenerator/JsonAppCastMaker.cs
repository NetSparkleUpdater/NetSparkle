using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Console = Colorful.Console;

namespace NetSparkleUpdater.AppCastGenerator
{
    public class JsonAppCastMaker : AppCastMaker
    {
        public JsonAppCastMaker(SignatureManager signatureManager, Options options) : base(signatureManager, options)
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
            return "json";
        }

        /// <inheritdoc/>
        public override (List<AppCastItem>, string?) GetItemsAndProductNameFromExistingAppCast(string appCastFileName, bool overwriteOldItemsInAppcast)
        {
            Console.WriteLine("Parsing existing app cast at {0}...", appCastFileName);
            var items = new List<AppCastItem>();
            string? productName = null;
            try
            {
                if (!File.Exists(appCastFileName))
                {
                    Console.WriteLine("App cast does not exist at {0}, so creating it anew...", appCastFileName, Color.Red);
                }
                else
                {
                    var logWriter = new LogWriter(LogWriterOutputMode.Console);
                    var generator = new JsonAppCastGenerator(logWriter);
                    var appCast = generator.ReadAppCastFromFile(appCastFileName);
                    productName = appCast.Title;

                    foreach (var currentItem in appCast.Items)
                    {
                        Console.WriteLine("Found an item in the app cast: version {0} ({1}) -- os = {2}",
                            currentItem.Version, currentItem.ShortVersion, currentItem.OperatingSystem);
                        var itemFound = items.Where(x => x.Version != null && x.Version == currentItem.Version?.Trim()).FirstOrDefault();
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
            } 
            catch (Exception e)
            {
                Console.WriteLine($"Error reading previous app cast: {e.Message}. Not using it for any items...", Color.Red);
                return (new List<AppCastItem>(), null);
            }
            items.Sort((a, b) => {
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
        public override void SerializeItemsToFile(List<AppCastItem> items, string applicationTitle, string path)
        {
            var jsonGenerator = new JsonAppCastGenerator()
            {
                HumanReadableOutput = HumanReadableOutput
            };
            Console.WriteLine("Writing json app cast to {0}", path);
            var appCast = new AppCast() 
            { 
                Items = items,
                Title = applicationTitle,
                Link = _opts.AppCastLink,
                Description = _opts.AppCastDescription,
                Language = "en"
            };
            jsonGenerator.SerializeAppCastToFile(appCast, path);
        }
    }
}
