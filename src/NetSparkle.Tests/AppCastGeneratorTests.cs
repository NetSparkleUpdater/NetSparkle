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
    }
}