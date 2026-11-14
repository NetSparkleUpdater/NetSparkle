using NetSparkleUpdater.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <summary>
    /// A Json-based app cast document downloader and handler
    /// </summary>
    public class JsonAppCastGenerator : AppCastGenerator
    {
        /// <summary>
        /// An app cast generator that reads/writes Json
        /// </summary>
        /// <param name="logger">Optional <seealso cref="ILogger"/> for logging data</param>
        public JsonAppCastGenerator(ILogger? logger = null) : base(logger)
        {
            HumanReadableOutput = true;
        }

        /// <summary>
        /// Set to true to make serialized output human readable (newlines, indents) when written to a file.
        /// Set to false to make this output not necessarily human readable.
        /// Defaults to true.
        /// </summary>
        public bool HumanReadableOutput { get; set; }

        /// <summary>
        /// Deserialize the app cast string into a list of <see cref="AppCastItem"/> objects.
        /// When complete, the <seealso cref="AppCast.Items"/> list should contain the parsed information
        /// as <see cref="AppCastItem"/> objects that are sorted in reverse order (if shouldSort is true) like so:
        /// appCast.Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
        /// </summary>
        /// <param name="appCastString">the non-null app cast</param>
        /// <param name="shouldSort">whether or not output should be sorted</param>
        public AppCast DeserializeAppCast(string appCastString, bool shouldSort = true)
        {
            var options = GetSerializerOptions();
#if NETFRAMEWORK || NETSTANDARD
            var appCast = JsonSerializer.Deserialize<AppCast>(appCastString, options) ?? new AppCast();
#else
            var jsonContext = new SourceGenerationContext(options);
            var appCast = JsonSerializer.Deserialize<AppCast>(appCastString, jsonContext.AppCast) ?? new AppCast();
#endif

            if (shouldSort)
            {
                // sort versions in reverse order
                appCast.Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
            }

            return appCast;
        }

        /// <inheritdoc/>
        public override AppCast DeserializeAppCast(string appCastString)
        {
            return DeserializeAppCast(appCastString, true);
        }

        /// <inheritdoc/>
        public override async Task<AppCast> DeserializeAppCastAsync(string appCastString, CancellationToken cancellationToken = default)
        {
            var options = GetSerializerOptions();
            
            using var stream = Utilities.GenerateStreamFromString(appCastString, Encoding.UTF8);

#if NETFRAMEWORK || NETSTANDARD
            var output = await JsonSerializer.DeserializeAsync<AppCast>(stream, options, cancellationToken) ?? new AppCast();
#else
            var jsonContext = new SourceGenerationContext(options);
            var output = await JsonSerializer.DeserializeAsync(stream, jsonContext.AppCast, cancellationToken) ?? new AppCast();
#endif

            // sort versions in reverse order
            output.Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));

            return output;
        }

        /// <summary>
        /// Deserialize the app cast from a file at the given path 
        /// into a list of <see cref="AppCastItem"/> objects.
        /// When complete, the <seealso cref="AppCast.Items"/> list is explicitly not sorted.
        /// </summary>
        /// <param name="stream">Stream containing the app cast data to deserialize</param>
        public AppCast DeserializeAppCastWithoutSorting(Stream stream)
        {
            using var reader = new StreamReader(stream);
            string json = reader.ReadToEnd();

            return DeserializeAppCast(json, false);
        }

        /// <inheritdoc/>
        public override async Task<AppCast> DeserializeAppCastAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var options = GetSerializerOptions();

#if NETFRAMEWORK || NETSTANDARD
            var output = await JsonSerializer.DeserializeAsync<AppCast>(stream, options, cancellationToken) ?? new AppCast();
#else
            var jsonContext = new SourceGenerationContext(options);
            var output = await JsonSerializer.DeserializeAsync(stream, jsonContext.AppCast, cancellationToken) ?? new AppCast();
#endif
            
            // sort versions in reverse order
            output.Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));

            return output;
        }

        /// <summary>
        /// Class to convert DateTime to universal time when serializing and then
        /// convert to local time when unserializing so that time zones are always
        /// written.
        /// From: https://github.com/dotnet/runtime/issues/1566
        /// </summary>
        public class DateTimeConverter : JsonConverter<DateTime>
        {
            /// <inheritdoc/>
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetDateTime().ToLocalTime();
            }

            /// <inheritdoc/>
            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToUniversalTime());
            }
        }

        /// <summary>
        /// Get <seealso cref="JsonSerializerOptions"/> for serialization methods
        /// </summary>
        /// <returns></returns>
        protected JsonSerializerOptions GetSerializerOptions()
        {
            var opts = new JsonSerializerOptions
            {
                WriteIndented = HumanReadableOutput,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
            
            opts.Converters.Add(new DateTimeConverter());

            return opts;
        }

        /// <inheritdoc/>
        public override string SerializeAppCast(AppCast appCast)
        {
            var options = GetSerializerOptions();

#if NETFRAMEWORK || NETSTANDARD
            return JsonSerializer.Serialize(appCast, options);
#else
            var jsonContext = new SourceGenerationContext(options);

            return JsonSerializer.Serialize(appCast, jsonContext.AppCast);
#endif
        }

        /// <inheritdoc/>
        public override async Task<string> SerializeAppCastAsync(AppCast appCast, CancellationToken cancellationToken = default)
        {
            var options = GetSerializerOptions();

            using MemoryStream memoryStream = new MemoryStream();

#if NETFRAMEWORK || NETSTANDARD
            await JsonSerializer.SerializeAsync(memoryStream, appCast, options, cancellationToken);
#else
            var jsonContext = new SourceGenerationContext(options);
            
            await JsonSerializer.SerializeAsync(memoryStream, appCast, jsonContext.AppCast, cancellationToken);
#endif
            memoryStream.Position = 0;

            using var reader = new StreamReader(memoryStream);

#if NET7_0_OR_GREATER
            return await reader.ReadToEndAsync(cancellationToken);
#else
            return await reader.ReadToEndAsync();
#endif
        }
    }
}