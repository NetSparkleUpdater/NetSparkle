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
        // pre-release stages are ordered by maturity
        [InlineData("1.0-pre-alpha", "1.0-alpha", -1)]
        [InlineData("1.0-pre-alpha.2", "1.0-alpha.1", -1)]
        [InlineData("1.0-prealpha", "1.0-pre-alpha", 0)]
        [InlineData("1.0-pre-alpha", "1.0-beta", -1)]
        [InlineData("1.0-dev", "1.0-alpha", -1)]
        [InlineData("1.0-nightly.5", "1.0-alpha.1", -1)]
        [InlineData("1.0-canary", "1.0-beta", -1)]
        [InlineData("1.0-beta", "1.0-preview", -1)]
        [InlineData("1.0-preview.3", "1.0-rc.1", -1)]
        [InlineData("1.0-rc.1", "1.0-final", -1)]
        [InlineData("1.0-rc.1", "1.0", -1)]
        [InlineData("1.0-preview.7", "1.0", -1)]
        // casing and separators do not matter, numbers are compared as numbers
        [InlineData("1.0-Beta", "1.0-beta", 0)]
        [InlineData("1.0-BETA.2", "1.0-beta.10", -1)]
        [InlineData("1.0-RC1", "1.0-rc.1", 0)]
        [InlineData("1.0-beta1", "1.0-beta.1", 0)]
        [InlineData("1.0-beta-1", "1.0-beta.1", 0)]
        [InlineData("1.0-beta2", "1.0-beta.10", -1)]
        [InlineData("1.0-beta1", "1.0-beta.11", -1)]
        [InlineData("1.0-beta.01", "1.0-beta.1", 0)]
        [InlineData("1.0-beta.99999999999999999999", "1.0-beta.100000000000000000000", -1)]
        // identifiers are compared one by one (SemVer 2.0.0 section 11)
        [InlineData("1.0-beta", "1.0-beta.1", -1)]
        [InlineData("1.0-alpha.1", "1.0-alpha.beta", -1)]
        [InlineData("1.0-alpha.beta", "1.0-beta", -1)]
        [InlineData("1.0-beta.11", "1.0-rc.1", -1)]
        public void CompareTest(string left, string right, int result)
        {
            Assert.Equal(
                expected: result,
                actual: SemVerLike.Parse(left).CompareTo(SemVerLike.Parse(right))
            );
        }

        [Fact]
        public void PreReleasesAreSortedFromLeastToMostMature()
        {
            // the example from the SemVer 2.0.0 spec, extended with other common stages
            var expected = new[]
            {
                "1.0.0-dev", "1.0.0-pre-alpha", "1.0.0-alpha", "1.0.0-alpha.1", "1.0.0-alpha.beta", "1.0.0-beta",
                "1.0.0-beta.2", "1.0.0-beta.11", "1.0.0-preview.1", "1.0.0-rc.1", "1.0.0", "1.0.1-alpha", "1.1.0-beta1"
            };
            var shuffled = expected.OrderBy(v => v.GetHashCode()).Reverse().ToList();
            var sorted = shuffled.OrderBy(v => SemVerLike.Parse(v)).ToList();
            Assert.Equal(expected, sorted);
        }

        [Fact]
        public void VersionComparisonIsAConsistentOrder()
        {
            var versions = new[]
            {
                "1.0", "1.0+abc", "1.0-dev", "1.0-nightly.3", "1.0-pre-alpha", "1.0-prealpha.1", "1.0-alpha", "1.0-Alpha.1", "1.0-alpha.10",
                "1.0-alpha.beta", "1.0-beta", "1.0-beta1", "1.0-beta.1", "1.0-beta-2", "1.0-beta.11+x", "1.0-preview", "1.0-pre.1", "1.0-rc",
                "1.0-RC.1", "1.0-final", "1.0-hotfix", "1.0-1", "1.0-01", "1.0-2.beta", "1.1-beta", "0.9"
            }.Select(v => SemVerLike.Parse(v)).ToList();
            foreach (var a in versions)
            {
                Assert.Equal(0, a.CompareTo(a));
                foreach (var b in versions)
                {
                    Assert.Equal(Math.Sign(a.CompareTo(b)), -Math.Sign(b.CompareTo(a)));
                    if (a.CompareTo(b) == 0)
                    {
                        Assert.Equal(a.GetHashCode(), b.GetHashCode());
                    }
                    foreach (var c in versions)
                    {
                        if (a.CompareTo(b) <= 0 && b.CompareTo(c) <= 0)
                        {
                            Assert.True(a.CompareTo(c) <= 0, $"{a} <= {b} <= {c} but {a} > {c}");
                        }
                    }
                }
            }
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
