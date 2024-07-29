using NetSparkleUpdater.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <summary>
    /// A Json-based app cast document downloader and handler
    /// </summary>
    public class JsonAppCastGenerator : IAppCastGenerator
    {
        private ILogger? _logWriter;

        public JsonAppCastGenerator(ILogger? logger = null)
        {
            _logWriter = logger;
            HumanReadableOutput = true;
        }

        public bool HumanReadableOutput { get; set; }

        /// <inheritdoc/>
        public AppCast ReadAppCast(string appCastString)
        {
#if NETFRAMEWORK || NETSTANDARD
            return JsonSerializer.Deserialize<AppCast>(appCastString) ?? new AppCast();
#else
            return JsonSerializer.Deserialize<AppCast>(appCastString, SourceGenerationContext.Default.AppCast) ?? new AppCast();
#endif
        }

        /// <inheritdoc/>
        public AppCast ReadAppCastFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return ReadAppCast(json);
        }

        /// <inheritdoc/>
        public string SerializeAppCast(AppCast appCast)
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = HumanReadableOutput,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
#if NETFRAMEWORK || NETSTANDARD
            return JsonSerializer.Serialize(appCast, options);
#else
            var jsonContext = new SourceGenerationContext(options);
            return JsonSerializer.Serialize(appCast, jsonContext.AppCast);
#endif
        }

        /// <inheritdoc/>
        public void SerializeAppCastToFile(AppCast appCast, string outputPath)
        {
            string json = SerializeAppCast(appCast);
            File.WriteAllText(outputPath, json);
        }
    }
}