using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Configurations;
using System.Text.Json.Serialization;

namespace NetSparkleUpdater
{
#if !NETFRAMEWORK && !NETSTANDARD
    [JsonSerializable(typeof(AppCast))]
    [JsonSerializable(typeof(AppCastItem))]
    [JsonSerializable(typeof(SavedConfigurationData))]
    [JsonSerializable(typeof(SemVerLike))]
    internal partial class SourceGenerationContext : JsonSerializerContext { }
#endif
}
