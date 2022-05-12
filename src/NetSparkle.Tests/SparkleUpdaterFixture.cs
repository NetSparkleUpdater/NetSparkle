using System;
using System.IO;
using NetSparkleUpdater;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Interfaces;
using Xunit;

namespace NetSparkleUnitTests
{
    public class SparkleUpdaterFixture : IDisposable
    {
        public const string CollectionName = "xmlappcast-fixture";

        public SparkleUpdater CreateUpdater(string xmlData, string installedVersion, IAppCastFilter filter = null)
        {
            SparkleUpdater updater = new SparkleUpdater("test-url", new AlwaysSucceedSignatureChecker());
            updater.AppCastDataDownloader = new StringCastDataDownloader(xmlData);
            updater.Configuration = new EmptyTestDataConfguration(new FakeTestDataAssemblyAccessor()
            {
                AssemblyCompany = "NetSparkle Test App",
                AssemblyCopyright = "@ (C) Thinking",
                AssemblyProduct = "Test",
                AssemblyVersion = installedVersion
            });

            if (filter != null)
            {
                XMLAppCast cast = updater.AppCastHandler as XMLAppCast;
                if(cast != null)
                    cast.AppCastFilter = filter;
            }

            return updater;
        }

        public string GetSimpleXmlAppCastData()
        {
            return ReadXmlFile("appcast_simple.xml");
        }

        public void Dispose()
        {
        }

        public string GetXmlAppCastDataWithBetaItems()
        {
            //
            // 2.1-prerelease - the prerelease tag is used to detect a "beta" version.
            // 2.0
            // 1.3
            //
            return ReadXmlFile("appcast_with_beta_items.xml");
        }

        private static string ReadXmlFile(string localFileName)
        {
            using (var xmlFile = new StreamReader(localFileName))
            {
                return xmlFile.ReadToEnd().Trim();
            }
        }
    }
}