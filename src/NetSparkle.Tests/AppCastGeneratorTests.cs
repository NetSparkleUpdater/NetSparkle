using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.IO;
using Xunit.Sdk;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using System.Xml;

namespace NetSparkleUnitTests
{
    public class AppCastGeneratorTests
    {
        public enum AppCastMakerType
        {
            Xml = 0,
            Json = 1
        }

        private IAppCastGenerator GetGeneratorForType(AppCastMakerType appCastMakerType)
        {
            var logger = new LogWriter()
            {
                OutputMode = NetSparkleUpdater.Enums.LogWriterOutputMode.Console
            };
            if (appCastMakerType == AppCastMakerType.Xml)
            {
                return new XMLAppCastGenerator(logger);
            }
            else
            {
                return new JsonAppCastGenerator(logger);
            }
        }

        [Theory]
        [InlineData(AppCastMakerType.Xml)]
        [InlineData(AppCastMakerType.Json)]
        public async void GeneratorOutputIsSorted(AppCastMakerType appCastMakerType)
        {
            var appCast = new AppCast()
            {
                Title = "My App",
                Items = new List<AppCastItem>()
                {
                    // intentionally put them out of order in the AppCast object
                    new AppCastItem() { Version = "0.9", DownloadLink = "https://mysite.com/update.exe" },
                    new AppCastItem() { Version = "1.3", DownloadLink = "https://mysite.com/update.exe" },
                    new AppCastItem() { Version = "1.1", DownloadLink = "https://mysite.com/update.exe" },
                }
            };

            IAppCastGenerator maker = GetGeneratorForType(appCastMakerType);
            var serialized = maker.SerializeAppCast(appCast); // .Serialize() makes no promises about sorting
            var deserialized = maker.DeserializeAppCast(serialized);
            Assert.Equal(3, deserialized.Items.Count);
            Assert.Equal("1.3", deserialized.Items[0].Version);
            Assert.Equal("1.1", deserialized.Items[1].Version);
            Assert.Equal("0.9", deserialized.Items[2].Version);
            // test with other methods
            deserialized = await maker.DeserializeAppCastAsync(serialized);
            Assert.Equal(3, deserialized.Items.Count);
            Assert.Equal("1.3", deserialized.Items[0].Version);
            Assert.Equal("1.1", deserialized.Items[1].Version);
            Assert.Equal("0.9", deserialized.Items[2].Version);
            // write to file
            var path = System.IO.Path.GetTempFileName();
            try 
            {
                maker.SerializeAppCastToFile(appCast, path);
                deserialized = maker.DeserializeAppCastFromFile(path);
                Assert.Equal(3, deserialized.Items.Count);
                Assert.Equal("1.3", deserialized.Items[0].Version);
                Assert.Equal("1.1", deserialized.Items[1].Version);
                Assert.Equal("0.9", deserialized.Items[2].Version);
                deserialized = await maker.DeserializeAppCastFromFileAsync(path);
                Assert.Equal(3, deserialized.Items.Count);
                Assert.Equal("1.3", deserialized.Items[0].Version);
                Assert.Equal("1.1", deserialized.Items[1].Version);
                Assert.Equal("0.9", deserialized.Items[2].Version);
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        [Theory]
        [InlineData(AppCastMakerType.Xml, nameof(IAppCastGenerator.SerializeAppCast))]
        [InlineData(AppCastMakerType.Xml, nameof(IAppCastGenerator.SerializeAppCastAsync))]
        [InlineData(AppCastMakerType.Xml, nameof(IAppCastGenerator.SerializeAppCastToFile))]
        [InlineData(AppCastMakerType.Xml, nameof(IAppCastGenerator.SerializeAppCastToFileAsync))]
        [InlineData(AppCastMakerType.Json, nameof(IAppCastGenerator.SerializeAppCast))]
        [InlineData(AppCastMakerType.Json, nameof(IAppCastGenerator.SerializeAppCastAsync))]
        [InlineData(AppCastMakerType.Json, nameof(IAppCastGenerator.SerializeAppCastToFile))]
        [InlineData(AppCastMakerType.Json, nameof(IAppCastGenerator.SerializeAppCastToFileAsync))]
        public async void TestCanSerializeAppCast(AppCastMakerType appCastMakerType, string serializeFuncName)
        {
            var appCast = new AppCast()
            {
                Title = "My App",
                Description = "My App Updates",
                Link = "https://mysite.com/updates",
                Language = "en_US",
                Items = new List<AppCastItem>()
                {
                    new AppCastItem() 
                    { 
                        Version = "1.3", 
                        DownloadLink = "https://mysite.com/update.deb",
                        DownloadSignature = "seru3112",
                        IsCriticalUpdate = true,
                        OperatingSystem = "linux",
                        PublicationDate = new DateTime(2023, 12, 09, 12, 12, 12),
                        Channel = "",
                    },
                    new AppCastItem() 
                    {
                        Title = "Super Beta",
                        Version = "0.9-beta1",
                        ShortVersion = "0.9",
                        DownloadLink = "https://mysite.com/update09beta.exe",
                        DownloadSignature = "seru311b2",
                        ReleaseNotesLink = "https://mysite.com/update09beta.md",
                        ReleaseNotesSignature = "srjlwj",
                        PublicationDate = new DateTime(1999, 12, 09, 11, 11, 11),
                        Channel = "beta",
                    },
                }
            };
            IAppCastGenerator maker = GetGeneratorForType(appCastMakerType);
            var serialized = "";
            if (serializeFuncName == nameof(IAppCastGenerator.SerializeAppCast))
            {
                serialized = maker.SerializeAppCast(appCast);
            }
            else if (serializeFuncName == nameof(IAppCastGenerator.SerializeAppCastAsync))
            {
                serialized = await maker.SerializeAppCastAsync(appCast);
            }
            else if (serializeFuncName == nameof(IAppCastGenerator.SerializeAppCastToFile))
            {
                var path = System.IO.Path.GetTempFileName();
                maker.SerializeAppCastToFile(appCast, path);
                serialized = await File.ReadAllTextAsync(path);
                File.Delete(path);
            }
            else if (serializeFuncName == nameof(IAppCastGenerator.SerializeAppCastToFileAsync))
            {
                var path = System.IO.Path.GetTempFileName();
                await maker.SerializeAppCastToFileAsync(appCast, path);
                serialized = await File.ReadAllTextAsync(path);
                File.Delete(path);
            }
            // manually parse things
            if (appCastMakerType == AppCastMakerType.Xml)
            {
                XDocument doc = XDocument.Parse(serialized);
                var rss = doc.Element("rss");
                var channel = rss.Element("channel");
                Assert.Equal("My App", channel.Element("title").Value);
                Assert.Equal("My App Updates", channel.Element("description").Value);
                Assert.Equal("https://mysite.com/updates", channel.Element("link").Value);
                Assert.Equal("en_US", channel.Element("language").Value);
                var items = channel?.Descendants("item");
                Assert.Equal(2, items.Count());
                var element = items.ElementAt(0);
                var nspace = XMLAppCastGenerator.SparkleNamespace;
                var enclosureElement = element.Element("enclosure");
                Assert.NotNull(enclosureElement);
                Assert.Equal("1.3", element.Element(nspace + "version").Value);
                Assert.Equal("https://mysite.com/update.deb", enclosureElement.Attribute("url").Value);
                Assert.Equal("seru3112", enclosureElement.Attribute(nspace + "signature").Value);
                Assert.Equal("", element.Element(nspace + "criticalUpdate").Value);
                Assert.Equal("linux", enclosureElement.Attribute(nspace + "os").Value);
                Assert.Contains("Sat, 09 Dec 2023 12:12:12", element.Element("pubDate").Value);
                Assert.Null(element.Element(nspace + "channel"));
                // test other item
                element = items.ElementAt(1);
                enclosureElement = element.Element("enclosure");
                Assert.NotNull(enclosureElement);
                Assert.Equal("Super Beta", element.Element("title").Value);
                Assert.Equal("0.9-beta1", element.Element(nspace + "version").Value);
                Assert.Equal("0.9", element.Element(nspace + "shortVersionString").Value);
                Assert.Equal("https://mysite.com/update09beta.exe", enclosureElement.Attribute("url").Value);
                Assert.Equal("seru311b2", enclosureElement.Attribute(nspace + "signature").Value);
                Assert.Equal("https://mysite.com/update09beta.md", element.Element(nspace + "releaseNotesLink").Value);
                Assert.Equal("srjlwj", element.Element(nspace + "releaseNotesLink").Attribute(nspace + "signature").Value);
                Assert.Null(element.Element(nspace + "criticalUpdate"));
                Assert.Null(enclosureElement.Element(nspace + "os"));
                Assert.Contains("Thu, 09 Dec 1999 11:11:11", element.Element("pubDate").Value);
                Assert.Equal("beta", element.Element(nspace + "channel").Value);
            }
            else
            {
                JsonNode mainNode = JsonNode.Parse(serialized);
                Assert.Equal("My App", mainNode["title"].ToString());
                Assert.Equal("My App Updates", mainNode["description"].ToString());
                Assert.Equal("https://mysite.com/updates", mainNode["link"].ToString());
                Assert.Equal("en_US", mainNode["language"].ToString());
                var items = mainNode["items"].AsArray();
                var element = items.ElementAt(0);
                Assert.Equal("1.3", element["version"].ToString());
                Assert.Equal("https://mysite.com/update.deb", element["url"].ToString());
                Assert.Equal("seru3112", element["signature"].ToString());
                Assert.Equal("true", element["is_critical"].ToString());
                Assert.Equal("linux", element["os"].ToString());
                Assert.Contains("2023-12-09T12:12:12", element["publication_date"].ToString());
                Assert.Equal("", element["channel"].ToString());
                element = items.ElementAt(1);
                Assert.Equal("Super Beta", element["title"].ToString());
                Assert.Equal("0.9-beta1", element["version"].ToString());
                Assert.Equal("0.9", element["short_version"].ToString());
                Assert.Equal("https://mysite.com/update09beta.exe", element["url"].ToString());
                Assert.Equal("seru311b2", element["signature"].ToString());
                Assert.Equal("https://mysite.com/update09beta.md", element["release_notes_link"].ToString());
                Assert.Equal("srjlwj", element["release_notes_signature"].ToString());
                Assert.Equal("false", element["is_critical"].ToString());
                Assert.Equal(AppCastItem.DefaultOperatingSystem, element["os"].ToString());
                Assert.Contains("1999-12-09T11:11:11", element["publication_date"].ToString());
                Assert.Equal("beta", element["channel"].ToString());
            }
        }
    }
}