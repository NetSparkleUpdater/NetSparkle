using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xunit;


namespace NetSparkle.Tests.AppCastGenerator
{
    public class URLEncodeTests
    {
        [Fact]
        public void TestURLEncoding()
        {
            // https://stackoverflow.com/questions/3572173/server-urlencode-vs-uri-escapedatastring
            var appName = "My FavoriteNew App";
            var encoded = HttpUtility.UrlEncode(appName);
            Assert.Equal("My+FavoriteNew+App", encoded);
            var encodedUri = Uri.EscapeDataString(appName);
            Assert.Equal("My%20FavoriteNew%20App", encodedUri); // <----- this is preferred!
        }
    }
}
