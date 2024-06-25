using NetSparkleUpdater.Configurations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetSparkleUpdater
{
    [JsonSerializable(typeof(SavedConfigurationData))]
    internal partial class SourceGenerationContext : JsonSerializerContext { }
}
