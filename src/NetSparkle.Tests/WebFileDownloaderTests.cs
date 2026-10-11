using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetSparkleUpdater.Downloaders;
using Xunit;

namespace NetSparkleUnitTests
{
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

        private const int SlowBodyChunkSize = 32 * 1024;
        private const int SlowBodyChunkCount = 1600;

        private static int GetFreePort()
        {
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }

        /// <summary>
        /// Starts a local server that answers a single request. The body is sent slowly so that
        /// a download can be canceled while it is still in progress.
        /// </summary>
        private static HttpListener StartServer(Uri url, Func<HttpListenerContext, Task> handler)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(url.GetLeftPart(UriPartial.Authority) + "/");
            listener.Start();
            Task.Run(async () =>
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    await handler(context);
                }
                catch
                {
                    // client went away or the listener was closed; nothing to do
                }
            });
            return listener;
        }

        private static async Task SendSlowBody(HttpListenerContext context)
        {
            context.Response.ContentLength64 = (long)SlowBodyChunkSize * SlowBodyChunkCount;
            var buffer = new byte[SlowBodyChunkSize];
            for (int i = 0; i < SlowBodyChunkCount; i++)
            {
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                await Task.Delay(5);
            }
            context.Response.Close();
        }

        private static string GetTempFilePath()
        {
            return Path.Combine(Path.GetTempPath(), "netsparkle-test-" + Guid.NewGuid().ToString("N") + ".bin");
        }

        [Theory]
        [InlineData(false)] // cancel from another thread (e.g. the user clicking a Cancel button)
        [InlineData(true)]  // cancel from within the progress callback
        public async Task CancelingDownloadReportsCancellationInsteadOfError(bool cancelInsideProgressCallback)
        {
            var url = new Uri("http://127.0.0.1:" + GetFreePort() + "/file.bin");
            var path = GetTempFilePath();
            var listener = StartServer(url, SendSlowBody);
            try
            {
                using (var downloader = new WebFileDownloader())
                {
                    downloader.PrepareToDownloadFile();
                    var completedEvents = new List<AsyncCompletedEventArgs>();
                    downloader.DownloadFileCompleted += (sender, e) => completedEvents.Add(e);
                    bool cancelRequested = false;
                    downloader.DownloadProgressChanged += (sender, e) =>
                    {
                        if (!cancelRequested && e.BytesReceived > 1024 * 1024)
                        {
                            cancelRequested = true;
                            if (cancelInsideProgressCallback)
                            {
                                downloader.CancelDownload();
                            }
                            else
                            {
                                Task.Run(async () =>
                                {
                                    await Task.Delay(20);
                                    downloader.CancelDownload();
                                });
                            }
                        }
                    };

                    await downloader.DownloadFile(url, path);

                    Assert.True(cancelRequested);
                    var completed = Assert.Single(completedEvents);
                    Assert.True(completed.Cancelled);
                    Assert.Null(completed.Error);
                    Assert.False(downloader.IsDownloading);
                }
            }
            finally
            {
                listener.Abort();
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public async Task FinishedDownloadIsNotReportedAsCanceled()
        {
            var url = new Uri("http://127.0.0.1:" + GetFreePort() + "/file.bin");
            var path = GetTempFilePath();
            var body = new byte[100 * 1024];
            var listener = StartServer(url, async context =>
            {
                context.Response.ContentLength64 = body.Length;
                await context.Response.OutputStream.WriteAsync(body, 0, body.Length);
                context.Response.Close();
            });
            try
            {
                using (var downloader = new WebFileDownloader())
                {
                    downloader.PrepareToDownloadFile();
                    var completedEvents = new List<AsyncCompletedEventArgs>();
                    downloader.DownloadFileCompleted += (sender, e) => completedEvents.Add(e);

                    await downloader.DownloadFile(url, path);

                    var completed = Assert.Single(completedEvents);
                    Assert.False(completed.Cancelled);
                    Assert.Null(completed.Error);
                    Assert.Equal(body.Length, new FileInfo(path).Length);
                }
            }
            finally
            {
                listener.Abort();
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public async Task FailedDownloadIsStillReportedAsError()
        {
            var url = new Uri("http://127.0.0.1:" + GetFreePort() + "/file.bin");
            var path = GetTempFilePath();
            var listener = StartServer(url, context =>
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                return Task.CompletedTask;
            });
            try
            {
                using (var downloader = new WebFileDownloader())
                {
                    downloader.PrepareToDownloadFile();
                    var completedEvents = new List<AsyncCompletedEventArgs>();
                    downloader.DownloadFileCompleted += (sender, e) => completedEvents.Add(e);

                    await downloader.DownloadFile(url, path);

                    var completed = Assert.Single(completedEvents);
                    Assert.False(completed.Cancelled);
                    Assert.NotNull(completed.Error);
                }
            }
            finally
            {
                listener.Abort();
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
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
