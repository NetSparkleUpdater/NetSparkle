using NetSparkleUpdater.Downloaders;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NetSparkleUnitTests
{
    /// <summary>
    /// Tests for <see cref="WebRequestAppCastDataDownloader.Headers"/>.
    /// </summary>
    public class WebRequestAppCastDataDownloaderTests
    {
        private const string FakeAppCastXml =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<rss version=\"2.0\" xmlns:sparkle=\"http://www.andymatuschak.org/xml-namespaces/sparkle\">" +
            "<channel><title>Test</title></channel></rss>";

        private sealed class RecordingHandler : HttpMessageHandler
        {
            public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(FakeAppCastXml, Encoding.UTF8, "text/xml"),
                };
                return Task.FromResult(response);
            }
        }

        private sealed class TestableDownloader : WebRequestAppCastDataDownloader
        {
            public RecordingHandler Recorder { get; } = new RecordingHandler();

            protected override HttpClient CreateHttpClient(HttpClientHandler? handler)
            {
                return new HttpClient(Recorder);
            }
        }

        [Fact]
        public async Task CustomHeaders_AreSentWithDownloadRequest()
        {
            var downloader = new TestableDownloader();
            downloader.Headers["User-Agent"] = "MyApp/1.2.3";
            downloader.Headers["X-Custom"] = "custom-value";

            var result = await downloader.DownloadAndGetAppCastDataAsync("https://example.com/appcast.xml");

            Assert.NotEmpty(result);
            var request = Assert.Single(downloader.Recorder.Requests);
            Assert.True(request.Headers.TryGetValues("User-Agent", out var userAgent));
            Assert.Equal("MyApp/1.2.3", userAgent.Single());
            Assert.True(request.Headers.TryGetValues("X-Custom", out var custom));
            Assert.Equal("custom-value", custom.Single());
        }

        [Fact]
        public async Task NoHeadersConfigured_SendsNoExtraHeaders()
        {
            var downloader = new TestableDownloader();

            await downloader.DownloadAndGetAppCastDataAsync("https://example.com/appcast.xml");

            var request = Assert.Single(downloader.Recorder.Requests);
            Assert.False(request.Headers.TryGetValues("User-Agent", out _));
            Assert.False(request.Headers.TryGetValues("X-Custom", out _));
        }

        [Fact]
        public async Task BlankHeaderName_IsSkippedWithoutThrowing()
        {
            var downloader = new TestableDownloader();
            downloader.Headers[""] = "ignored";
            downloader.Headers["   "] = "ignored";
            downloader.Headers["X-Valid"] = "kept";

            var result = await downloader.DownloadAndGetAppCastDataAsync("https://example.com/appcast.xml");

            Assert.NotEmpty(result);
            var request = Assert.Single(downloader.Recorder.Requests);
            Assert.True(request.Headers.TryGetValues("X-Valid", out _));
        }
    }
}
