#nullable enable

using NetSparkleUpdater.Configurations;
using System.Text.Json.Serialization;

namespace NetSparkleUpdater
{
#if !NETFRAMEWORK && !NETSTANDARD
    [JsonSerializable(typeof(SavedConfigurationData))]
    internal partial class SourceGenerationContext : JsonSerializerContext { }
#endif
}
