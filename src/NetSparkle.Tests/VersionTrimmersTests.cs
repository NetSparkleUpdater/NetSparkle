using NetSparkleUpdater.AppCastHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NetSparkleUnitTests
{
    public class VersionTrimmersTests
    {
        [Theory]
        [InlineData("2.1", "2.1")]
        [InlineData("1.2.3", "1.2.3")]
        [InlineData("1.2.3.4", "1.2.3.4")]
        [InlineData("0.1-alpha.1", "0.1")]
        [InlineData("0.2+123", "0.2")]
        public void DefaultVersionTrimmerTest(string from, string to)
        {
            Assert.Equal(
                expected: to,
                actual: VersionTrimmers.DefaultVersionTrimmer(SemVerLike.Parse(from)).ToString()
            );
        }
    }
}
