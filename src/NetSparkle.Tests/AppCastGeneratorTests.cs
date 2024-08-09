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
                        PublicationDate = new DateTime(2023, 12, 09, 12, 12, 12, DateTimeKind.Local),
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
                        PublicationDate = new DateTime(1999, 12, 09, 11, 11, 11, DateTimeKind.Local),
                        Channel = "beta",
                    },
                }
            };
            Console.WriteLine(appCast.Items[0].PublicationDate.ToString());
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
                Assert.Contains("2023-12-09T03:12:12Z", element["publication_date"].ToString());
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
                Assert.Contains("1999-12-09T02:11:11Z", element["publication_date"].ToString());
                Assert.Equal("beta", element["channel"].ToString());
            }
        }

        [Theory]
        [InlineData(AppCastMakerType.Xml, nameof(IAppCastGenerator.DeserializeAppCast))]
        [InlineData(AppCastMakerType.Xml, nameof(IAppCastGenerator.DeserializeAppCastAsync))]
        [InlineData(AppCastMakerType.Xml, nameof(IAppCastGenerator.DeserializeAppCastFromFile))]
        [InlineData(AppCastMakerType.Xml, nameof(IAppCastGenerator.DeserializeAppCastFromFileAsync))]
        [InlineData(AppCastMakerType.Json, nameof(IAppCastGenerator.DeserializeAppCast))]
        [InlineData(AppCastMakerType.Json, nameof(IAppCastGenerator.DeserializeAppCastAsync))]
        [InlineData(AppCastMakerType.Json, nameof(IAppCastGenerator.DeserializeAppCastFromFile))]
        [InlineData(AppCastMakerType.Json, nameof(IAppCastGenerator.DeserializeAppCastFromFileAsync))]
        public async void TestCanDeserializeAppCast(AppCastMakerType appCastMakerType, string deserializeFuncName)
        {
            var appCastToDeserialize = "";
            if (appCastMakerType == AppCastMakerType.Xml)
            {
                appCastToDeserialize = @"<?xml version=""1.0"" encoding=""utf-8""?>
<rss version=""2.0"" xmlns:sparkle=""http://www.andymatuschak.org/xml-namespaces/sparkle"">
  <channel>
    <title>My App</title>
    <link>https://mysite.com/updates</link>
    <description>My App Updates</description>
    <language>en_US</language>
    <item>
      <title></title>
      <pubDate>Sat, 09 Dec 2023 12:12:12 +09:00</pubDate>
      <sparkle:version>1.3</sparkle:version>
      <sparkle:shortVersionString />
      <sparkle:criticalUpdate />
      <enclosure url=""https://mysite.com/update.deb"" sparkle:version=""1.3"" length=""0"" sparkle:os=""linux"" type=""application/octet-stream"" sparkle:criticalUpdate=""true"" sparkle:signature=""seru3112"" />
    </item>
    <item>
      <title>Super Beta</title>
      <sparkle:releaseNotesLink sparkle:signature=""srjlwj"">https://mysite.com/update09beta.md</sparkle:releaseNotesLink>
      <pubDate>Thu, 09 Dec 1999 11:11:11 +09:00</pubDate>
      <sparkle:version>0.9-beta1</sparkle:version>
      <sparkle:shortVersionString>0.9</sparkle:shortVersionString>
      <sparkle:channel>beta</sparkle:channel>
      <enclosure url=""https://mysite.com/update09beta.exe"" sparkle:version=""0.9-beta1"" sparkle:shortVersionString=""0.9"" length=""0"" sparkle:os=""windows"" type=""application/octet-stream"" sparkle:criticalUpdate=""false"" sparkle:signature=""seru311b2"" />
    </item>
  </channel>
</rss>".Trim();
            }
            else
            {
                appCastToDeserialize = @"{
  ""title"": ""My App"",
  ""language"": ""en_US"",
  ""description"": ""My App Updates"",
  ""link"": ""https://mysite.com/updates"",
  ""items"": [
    {
      ""version"": ""1.3"",
      ""url"": ""https://mysite.com/update.deb"",
      ""signature"": ""seru3112"",
      ""publication_date"": ""2023-12-09T03:12:12Z"",
      ""is_critical"": true,
      ""size"": 0,
      ""os"": ""linux"",
      ""channel"": """",
      ""type"": ""application/octet-stream""
    },
    {
      ""title"": ""Super Beta"",
      ""version"": ""0.9-beta1"",
      ""short_version"": ""0.9"",
      ""release_notes_link"": ""https://mysite.com/update09beta.md"",
      ""release_notes_signature"": ""srjlwj"",
      ""url"": ""https://mysite.com/update09beta.exe"",
      ""signature"": ""seru311b2"",
      ""publication_date"": ""1999-12-09T02:11:11Z"",
      ""is_critical"": false,
      ""size"": 0,
      ""os"": ""windows"",
      ""channel"": ""beta"",
      ""type"": ""application/octet-stream""
    }
  ]
}";
            }
            IAppCastGenerator maker = GetGeneratorForType(appCastMakerType);
            AppCast deserialized = null;
            if (deserializeFuncName == nameof(IAppCastGenerator.DeserializeAppCast))
            {
                deserialized = maker.DeserializeAppCast(appCastToDeserialize);
            }
            else if (deserializeFuncName == nameof(IAppCastGenerator.DeserializeAppCastAsync))
            {
                deserialized = await maker.DeserializeAppCastAsync(appCastToDeserialize);
            }
            else if (deserializeFuncName == nameof(IAppCastGenerator.DeserializeAppCastFromFile))
            {
                var path = System.IO.Path.GetTempFileName();
                await File.WriteAllTextAsync(path, appCastToDeserialize);
                deserialized = maker.DeserializeAppCastFromFile(path);
                File.Delete(path);
            }
            else if (deserializeFuncName == nameof(IAppCastGenerator.DeserializeAppCastFromFileAsync))
            {
                var path = System.IO.Path.GetTempFileName();
                await File.WriteAllTextAsync(path, appCastToDeserialize);
                deserialized = await maker.DeserializeAppCastFromFileAsync(path);
                File.Delete(path);
            }
            // now that we have the app cast, test the data in it
            Assert.Equal("My App", deserialized.Title);
            Assert.Equal("My App Updates", deserialized.Description);
            Assert.Equal("https://mysite.com/updates", deserialized.Link);
            Assert.Equal("en_US", deserialized.Language);
            Assert.Equal(2, deserialized.Items.Count);

            Assert.Equal("1.3", deserialized.Items[0].Version);
            Assert.Equal("https://mysite.com/update.deb", deserialized.Items[0].DownloadLink);
            Assert.Equal("seru3112", deserialized.Items[0].DownloadSignature);
            Assert.True(deserialized.Items[0].IsCriticalUpdate);
            Assert.Equal("linux", deserialized.Items[0].OperatingSystem);
            Assert.Equal(2023, deserialized.Items[0].PublicationDate.Year);
            Assert.Equal(12, deserialized.Items[0].PublicationDate.Month);
            Assert.Equal(9, deserialized.Items[0].PublicationDate.Day);
            Assert.Equal(12, deserialized.Items[0].PublicationDate.Hour);
            Assert.Equal(12, deserialized.Items[0].PublicationDate.Minute);
            Assert.Equal(12, deserialized.Items[0].PublicationDate.Second);
            Assert.True(string.IsNullOrWhiteSpace(deserialized.Items[0].Channel));

            Assert.Equal("Super Beta", deserialized.Items[1].Title);
            Assert.Equal("0.9-beta1", deserialized.Items[1].Version);
            Assert.Equal("0.9", deserialized.Items[1].ShortVersion);
            Assert.Equal("https://mysite.com/update09beta.exe", deserialized.Items[1].DownloadLink);
            Assert.Equal("seru311b2", deserialized.Items[1].DownloadSignature);
            Assert.Equal("https://mysite.com/update09beta.md", deserialized.Items[1].ReleaseNotesLink);
            Assert.Equal("srjlwj", deserialized.Items[1].ReleaseNotesSignature);
            Assert.Equal(1999, deserialized.Items[1].PublicationDate.Year);
            Assert.Equal(12, deserialized.Items[1].PublicationDate.Month);
            Assert.Equal(9, deserialized.Items[1].PublicationDate.Day);
            Assert.Equal(11, deserialized.Items[1].PublicationDate.Hour);
            Assert.Equal(11, deserialized.Items[1].PublicationDate.Minute);
            Assert.Equal(11, deserialized.Items[1].PublicationDate.Second);
            Assert.Equal("beta", deserialized.Items[1].Channel);
            Assert.False(deserialized.Items[1].IsCriticalUpdate);
            Assert.Equal(AppCastItem.DefaultOperatingSystem, deserialized.Items[1].OperatingSystem);
        }
    }
}