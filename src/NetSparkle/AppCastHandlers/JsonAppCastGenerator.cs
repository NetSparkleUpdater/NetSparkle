using NetSparkleUpdater.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <summary>
    /// A Json-based app cast document downloader and handler
    /// </summary>
    public class JsonAppCastGenerator : IAppCastGenerator
    {
        private ILogger? _logWriter;

        /// <summary>
        /// An app cast generator that reads/writes Json
        /// </summary>
        /// <param name="logger">Optional <seealso cref="ILogger"/> for logging data</param>
        public JsonAppCastGenerator(ILogger? logger = null)
        {
            _logWriter = logger;
            HumanReadableOutput = true;
        }

        /// <summary>
        /// True if serialized files should be human readable; false otherwise
        /// </summary>
        public bool HumanReadableOutput { get; set; }

        /// <inheritdoc/>
        public AppCast DeserializeAppCast(string appCastString)
        {
#if NETFRAMEWORK || NETSTANDARD
            var appCast = JsonSerializer.Deserialize<AppCast>(appCastString) ?? new AppCast();
#else
            var appCast = JsonSerializer.Deserialize<AppCast>(appCastString, SourceGenerationContext.Default.AppCast) ?? new AppCast();
#endif
            // sort versions in reverse order
            appCast.Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
            return appCast;
        }

        /// <inheritdoc/>
        public async Task<AppCast> DeserializeAppCastAsync(string appCastString)
        {
            using (var stream = Utilities.GenerateStreamFromString(appCastString, Encoding.UTF8))
            {
#if NETFRAMEWORK || NETSTANDARD
                return await JsonSerializer.DeserializeAsync<AppCast>(stream) ?? new AppCast();
#else
                return await JsonSerializer.DeserializeAsync<AppCast>(stream, SourceGenerationContext.Default.AppCast) ?? new AppCast();
#endif
            }
        }

        /// <inheritdoc/>
        public AppCast DeserializeAppCastFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return DeserializeAppCast(json);
        }

        /// <inheritdoc/>
        public async Task<AppCast> DeserializeAppCastFromFileAsync(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            {
#if NETFRAMEWORK || NETSTANDARD
                return await JsonSerializer.DeserializeAsync<AppCast>(fileStream) ?? new AppCast();
#else
                return await JsonSerializer.DeserializeAsync<AppCast>(fileStream, SourceGenerationContext.Default.AppCast) ?? new AppCast();
#endif
            }
        }

        /// <summary>
        /// Get <seealso cref="JsonSerializerOptions"/> for serialization methods
        /// </summary>
        /// <returns></returns>
        protected JsonSerializerOptions GetSerializerOptions()
        {
            return new JsonSerializerOptions 
            { 
                WriteIndented = HumanReadableOutput,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
        }

        /// <inheritdoc/>
        public string SerializeAppCast(AppCast appCast)
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
        public async Task<string> SerializeAppCastAsync(AppCast appCast)
        {
            var options = GetSerializerOptions();
            using (MemoryStream memoryStream = new MemoryStream())
            {
#if NETFRAMEWORK || NETSTANDARD
                await JsonSerializer.SerializeAsync(memoryStream, appCast, options);
#else
                var jsonContext = new SourceGenerationContext(options);
                await JsonSerializer.SerializeAsync(memoryStream, appCast, jsonContext.AppCast);
#endif
                memoryStream.Position = 0;
                using var reader = new StreamReader(memoryStream);
                return await reader.ReadToEndAsync();
            }
        }

        /// <inheritdoc/>
        public void SerializeAppCastToFile(AppCast appCast, string outputPath)
        {
            string json = SerializeAppCast(appCast);
            File.WriteAllText(outputPath, json);
        }

        /// <inheritdoc/>
        public async Task SerializeAppCastToFileAsync(AppCast appCast, string outputPath)
        {
            string json = await SerializeAppCastAsync(appCast);
#if NETFRAMEWORK || NETSTANDARD
            await Utilities.WriteTextAsync(outputPath, json);
#else
            await File.WriteAllTextAsync(outputPath, json);
#endif
        }
    }
}