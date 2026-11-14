using NetSparkleUpdater.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <inheritdoc/>
    public abstract class AppCastGenerator : IAppCastGenerator
    {
        /// <inheritdoc/>
        protected readonly ILogger? _logWriter;

        /// <summary>
        /// An app cast generator that reads/writes Json
        /// </summary>
        /// <param name="logger">Optional <seealso cref="ILogger"/> for logging data</param>
        protected AppCastGenerator(ILogger? logger = null)
        {
            _logWriter = logger;
        }

        /// <inheritdoc/>
        public abstract AppCast DeserializeAppCast(string appCastString);

        /// <inheritdoc/>
        public AppCast DeserializeAppCast(Stream stream)
        {
            using var reader = new StreamReader(stream);
            string data = reader.ReadToEnd();

            return DeserializeAppCast(data);
        }

        /// <inheritdoc/>
        public abstract Task<AppCast> DeserializeAppCastAsync(string appCastString, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<AppCast> DeserializeAppCastAsync(Stream stream, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract string SerializeAppCast(AppCast appCast);

        /// <inheritdoc/>
        public void SerializeAppCast(AppCast appCast, Stream stream)
        {
            string data = SerializeAppCast(appCast);

            using var writer = new StreamWriter(stream);

            writer.Write(data.AsMemory());
            writer.Flush();
        }

        /// <inheritdoc/>
        public abstract Task<string> SerializeAppCastAsync(AppCast appCast, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public async Task SerializeAppCastAsync(AppCast appCast, Stream stream, CancellationToken cancellationToken = default)
        {
            string data = await SerializeAppCastAsync(appCast, cancellationToken);

            using var writer = new StreamWriter(stream);

#if NET8_0_OR_GREATER
            await writer.WriteAsync(data.AsMemory(), cancellationToken);
            await writer.FlushAsync(cancellationToken);
#else
            await writer.WriteAsync(data);
            await writer.FlushAsync();
#endif
        }
    }
}
