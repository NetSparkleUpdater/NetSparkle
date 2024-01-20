using NetSparkleUpdater;
using NetSparkleUpdater.AppCastHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NetSparkleUnitTests
{
    public class AppCastReducersTests
    {
        private readonly AppCastItem[] _items;

        public AppCastReducersTests()
        {
            _items = new AppCastItem[]
            {
                new AppCastItem { Version = "2.1-prerelease", },
                new AppCastItem { Version = "2.0", },
                new AppCastItem { Version = "1.3", },
            };
        }

        [Theory]
        [InlineData("2.2", "2.0")]
        public void OnlyRetailVersionsTests(string installedVersion, string firstAvailable)
        {
            TestFilteredResult(
                AppCastReducers.OnlyRetailVersions(SemVerLike.Parse(installedVersion), _items),
                firstAvailable
            );
        }

        [Theory]
        [InlineData("2.2", "2.1-prerelease")]
        public void OnlyPreReleasedVersionsTests(string installedVersion, string firstAvailable)
        {
            TestFilteredResult(
                AppCastReducers.OnlyPreReleasedVersions(SemVerLike.Parse(installedVersion), _items),
                firstAvailable
            );
        }

        [Theory]
        [InlineData("2.2", "2.1-prerelease")]
        public void NewerFirstTests(string installedVersion, string firstAvailable)
        {
            TestFilteredResult(
                AppCastReducers.NewerFirst(SemVerLike.Parse(installedVersion), _items),
                firstAvailable
            );
        }

        [Theory]
        [InlineData("3.0", null)]
        [InlineData("2.2", null)]
        [InlineData("2.1", null)]
        [InlineData("2.1-prerelease", null)]
        [InlineData("2.0", "2.1-prerelease")]
        [InlineData("1.3", "2.1-prerelease")]
        [InlineData("1.2", "2.1-prerelease")]
        public void NoOlderVersionsTests(string installedVersion, string firstAvailable)
        {
            TestFilteredResult(
                AppCastReducers.RemoveOlderVersions(SemVerLike.Parse(installedVersion), _items),
                firstAvailable
            );
        }

        [Theory]
        [InlineData("3.0", null)]
        [InlineData("2.2", null)]
        [InlineData("2.1", null)]
        [InlineData("2.1-prerelease", null)]
        [InlineData("2.0", null)]
        [InlineData("1.3", "2.0")]
        [InlineData("1.2", "2.0")]
        public void MixTests(string installedVersion, string firstAvailable)
        {
            TestFilteredResult(
                AppCastReducers.Mix(
                    AppCastReducers.OnlyRetailVersions,
                    AppCastReducers.RemoveOlderVersions,
                    AppCastReducers.NewerFirst
                )(
                    SemVerLike.Parse(installedVersion),
                    _items
                ),
                firstAvailable
            );
        }

        private void TestFilteredResult(IEnumerable<AppCastItem> items, string firstAvailable)
        {
            if (firstAvailable == null)
            {
                Assert.Empty(items);
            }
            else
            {
                Assert.Equal(expected: firstAvailable, actual: items.First().Version);
            }
        }
    }
}
