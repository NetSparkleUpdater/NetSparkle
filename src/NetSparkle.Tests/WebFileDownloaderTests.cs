using NetSparkleUpdater.Downloaders;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Tests for <see cref="WebFileDownloader.Headers"/>.
    /// </summary>
    public class WebFileDownloaderTests
    {
        private sealed class RecordingHandler : HttpMessageHandler
        {
            public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes("fake-binary-data")),
                };
                return Task.FromResult(response);
            }
        }

        private sealed class TestableDownloader : WebFileDownloader
        {
            public RecordingHandler Recorder { get; } = new RecordingHandler();

            protected override HttpClient CreateHttpClient()
            {
                var client = new HttpClient(Recorder);
                ApplyHeaders(client);
                return client;
            }
        }

        [Fact]
        public async Task CustomHeaders_AreSentWithFileDownload()
        {
            var downloader = new TestableDownloader();
            downloader.Headers["User-Agent"] = "MyApp/1.2.3";
            downloader.Headers["X-Custom"] = "custom-value";

            var tempFile = Path.GetTempFileName();
            try
            {
                await downloader.DownloadFile(new Uri("https://example.com/app.zip"), tempFile);

                var request = Assert.Single(downloader.Recorder.Requests);
                Assert.True(request.Headers.TryGetValues("User-Agent", out var userAgent));
                Assert.Equal("MyApp/1.2.3", userAgent.Single());
                Assert.True(request.Headers.TryGetValues("X-Custom", out var custom));
                Assert.Equal("custom-value", custom.Single());
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task NoHeadersConfigured_SendsNoExtraHeaders()
        {
            var downloader = new TestableDownloader();

            var tempFile = Path.GetTempFileName();
            try
            {
                await downloader.DownloadFile(new Uri("https://example.com/app.zip"), tempFile);

                var request = Assert.Single(downloader.Recorder.Requests);
                Assert.False(request.Headers.TryGetValues("User-Agent", out _));
                Assert.False(request.Headers.TryGetValues("X-Custom", out _));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task BlankHeaderName_IsSkippedWithoutThrowing()
        {
            var downloader = new TestableDownloader();
            downloader.Headers[""] = "ignored";
            downloader.Headers["   "] = "ignored";
            downloader.Headers["X-Valid"] = "kept";

            var tempFile = Path.GetTempFileName();
            try
            {
                await downloader.DownloadFile(new Uri("https://example.com/app.zip"), tempFile);

                var request = Assert.Single(downloader.Recorder.Requests);
                Assert.True(request.Headers.TryGetValues("X-Valid", out _));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
