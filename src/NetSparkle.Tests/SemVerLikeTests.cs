using NetSparkleUpdater.AppCastHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NetSparkleUnitTests
{
    public class SemVerLikeTests
    {
        [Theory]
        [InlineData("1.0", "1.0", "")]
        [InlineData("1.0-alpha.1", "1.0", "-alpha.1")]
        [InlineData("1.0-alpha.1+123", "1.0", "-alpha.1+123")]
        [InlineData("1.0+123", "1.0", "+123")]
        public void ParseTests(string input, string version, string allSuffixes)
        {
            var parsed = SemVerLike.Parse(input);
            Assert.Equal(expected: version, parsed.Version);
            Assert.Equal(expected: allSuffixes, parsed.AllSuffixes);
        }

        [Theory]
        [InlineData("0.1", "1.0", -1)]
        [InlineData("1.1", "0.0", 1)]
        [InlineData("1.0", "1.0", 0)]
        [InlineData("1.0", "1.0-alpha.1", 1)]
        [InlineData("1.0-alpha.1", "1.0-alpha.1", 0)]
        [InlineData("1.0-alpha.1", "1.0", -1)]
        [InlineData("100.0", "11.999.999", 1)]
        [InlineData("11.999.999", "100.0", -1)]
        [InlineData("1.0-alpha.1", "1.0-alpha.2", -1)]
        [InlineData("1.0-alpha.2", "1.0-alpha.1", 1)]
        [InlineData("1.0-alpha.1", "1.0-beta.2", -1)]
        [InlineData("1.0-rc.1", "1.0-beta.2", 1)]
        [InlineData("1", "1", 0)]
        [InlineData("1", "2", -1)]
        [InlineData("2", "1", 1)]
        // build metadata (e.g. the commit hash added by .NET 8+) does not affect precedence
        [InlineData("1.0+abc123", "1.0", 0)]
        [InlineData("1.0", "1.0+abc123", 0)]
        [InlineData("1.0+abc123", "1.0+def456", 0)]
        [InlineData("1.0.0+5", "1.0.0+10", 0)]
        [InlineData("1.0-alpha.1+abc123", "1.0-alpha.1", 0)]
        [InlineData("1.0-alpha.1+abc123", "1.0-alpha.2+abc123", -1)]
        [InlineData("1.0-alpha.1+abc123", "1.0+abc123", -1)]
        [InlineData("1.0+abc123", "1.0-rc.1+abc123", 1)]
        [InlineData("1.0+abc123", "1.1", -1)]
        [InlineData("1.1+abc123", "1.0", 1)]
        public void CompareTest(string left, string right, int result)
        {
            Assert.Equal(
                expected: result,
                actual: SemVerLike.Parse(left).CompareTo(SemVerLike.Parse(right))
            );
        }

        [Fact]
        public void VersionsWithTheSamePrecedenceHaveTheSameHashCode()
        {
            var withMetadata = SemVerLike.Parse("1.0+abc123");
            var withoutMetadata = SemVerLike.Parse("1.0");
            Assert.Equal(withoutMetadata, withMetadata);
            Assert.Equal(withoutMetadata.GetHashCode(), withMetadata.GetHashCode());
            Assert.NotEqual(SemVerLike.Parse("1.1").GetHashCode(), withMetadata.GetHashCode());
        }
    }
}
