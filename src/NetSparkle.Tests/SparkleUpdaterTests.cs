using NetSparkleUpdater;
using Xunit;

namespace NetSparkleUnitTests
{
    [Collection(SparkleUpdaterFixture.CollectionName)]
    public class SparkleUpdaterTests
    {
        private SparkleUpdaterFixture _fixture;

        public SparkleUpdaterTests(SparkleUpdaterFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async void TestCanFilterOutBetaItems()
        {
            SparkleUpdater updater = _fixture.CreateUpdater(_fixture.GetSimpleXmlAppCastData(), "1.9");
            // fetch the valid updates from the updater...
            UpdateInfo info = await _fixture.Updater.CheckForUpdatesQuietly();
            Assert.Single(info.Updates);
            AppCastItem item = info.Updates[0];
        }
    }
}