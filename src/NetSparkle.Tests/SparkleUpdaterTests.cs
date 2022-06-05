using System;
using System.Linq;
using System.Collections.Generic;
using NetSparkleUpdater;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;
using Xunit;

namespace NetSparkleUnitTests
{
    [Collection(SparkleUpdaterFixture.CollectionName)]
    public class SparkleUpdaterTests : IAppCastFilter
    {
        private readonly SparkleUpdaterFixture _fixture;
        private bool _filterOutBetaAppCastItems = false;
        private bool _alwaysInstallLatest = false;

        public SparkleUpdaterTests(SparkleUpdaterFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// Test the app cast filtering.  The InlineData provides the matrix of possibilities for regression to a stable version (alwaysInstallLatest),
        /// the currently installed version and what is expected to be observed as a result.
        /// </summary>
        /// <param name="alwaysInstallLatest"></param>
        /// <param name="currentInstalledVersion"></param>
        /// <param name="expectedVersionToInstall"></param>
        /// <param name="expectedStatus"></param>
        [Theory]
        // first param true - means effectively: we want to use a stable channel (beta items are being removed)
        [InlineData(true, false, "2.1-prerelease", null, UpdateStatus.CouldNotDetermine)] // because System.Version() cannot handle semver. 
        [InlineData(true, true,  "2.1-prerelease", null, UpdateStatus.CouldNotDetermine)] // because System.Version() cannot handle semver. 
        [InlineData(true, false, "2.1", null, UpdateStatus.UpdateNotAvailable)]
        [InlineData(true, true,  "2.1", "2.0", UpdateStatus.UpdateAvailable)]
        [InlineData(true, false, "2.0", null, UpdateStatus.UpdateNotAvailable)]
        [InlineData(true, true,  "2.0", "2.0", UpdateStatus.UpdateAvailable)]
        [InlineData(true, true,  "1.9", "2.0", UpdateStatus.UpdateAvailable)]
        [InlineData(true, false, "1.9", "2.0", UpdateStatus.UpdateAvailable)]
        // first param false - means effectively: we are using a beta channel (beta items are left in)
        [InlineData(false, false, "2.1", null, UpdateStatus.UpdateNotAvailable)]
        [InlineData(false, true, "2.1", "2.1", UpdateStatus.UpdateAvailable)]
        [InlineData(false, false, "2.0", "2.1", UpdateStatus.UpdateAvailable)]
        [InlineData(false, true, "2.0", "2.1", UpdateStatus.UpdateAvailable)]
        [InlineData(false, true, "1.9", "2.1", UpdateStatus.UpdateAvailable)]
        [InlineData(false, false, "1.9", "2.1", UpdateStatus.UpdateAvailable)]
        public async void TestFilteringAndForceInstalls(bool removeBetaItems, bool alwaysInstallLatest, string currentInstalledVersion, string expectedVersionToInstall, UpdateStatus expectedStatus)
        {
            var appCast = _fixture.GetXmlAppCastDataWithBetaItems();
            SparkleUpdater updater = _fixture.CreateUpdater(appCast, currentInstalledVersion, this);

            // engage filtering... 
            this._filterOutBetaAppCastItems = removeBetaItems;
            this._alwaysInstallLatest = alwaysInstallLatest;
            
            UpdateInfo after = await updater.CheckForUpdatesQuietly();
            Assert.Equal(expectedStatus, after.Status);
            if (expectedVersionToInstall != null)
            {
                Assert.True(after.Updates.Count >= 1);
                AppCastItem item = after.Updates[0];
                Assert.DoesNotContain(item.Title, "prerelease");
                Assert.Equal(item.Version, expectedVersionToInstall);
            }
        }
        
        [Fact]
        public async void TestFetchOfLatestAppCastItem()
        {
            SparkleUpdater updater = _fixture.CreateUpdater(_fixture.GetSimpleXmlAppCastData(), "1.9");

            // fetch the valid updates from the updater...
            UpdateInfo info = await updater.CheckForUpdatesQuietly();
            Assert.Single(info.Updates);
            AppCastItem item = info.Updates[0];
            Assert.Equal("2.0", item.Version);
        }

        public FilterResult GetFilteredAppCastItems(Version installed, List<AppCastItem> items)
        {
            List<AppCastItem> results = items.Where((item) =>
            {
                if (_filterOutBetaAppCastItems)
                {
                    if (item.Title.Contains("prerelease"))
                        return false;
                }

                return true;
            }).ToList();

            return new FilterResult(_alwaysInstallLatest, results);
        }
    }
}