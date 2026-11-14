using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NetSparkleUpdater.Interfaces
{
    /// <inheritdoc/>
    public static class AppCastGeneratorExtensions
    {
        /// <summary>
        /// Deserialize the app cast from a file at the given path 
        /// into a list of <see cref="AppCastItem"/> objects.
        /// When complete, the <seealso cref="AppCast.Items"/> list should contain the parsed information
        /// as <see cref="AppCastItem"/> objects that are sorted in reverse order like so:
        /// appCast.Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
        /// </summary>
        /// <param name="generator">The app cast generator</param>
        /// <param name="filePath">Path to the file on disk to deserialize</param>
        public static AppCast DeserializeAppCastFromFile(this IAppCastGenerator generator, string filePath)
        {
            using var stream = File.OpenRead(filePath);

            return generator.DeserializeAppCast(stream);
        }

        /// <summary>
        /// Deserialize the app cast from a file at the given path asynchronously 
        /// into a list of <see cref="AppCastItem"/> objects.
        /// When complete, the <seealso cref="AppCast.Items"/> list should contain the parsed information
        /// as <see cref="AppCastItem"/> objects that are sorted in reverse order like so:
        /// appCast.Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
        /// </summary>
        /// <param name="generator">The app cast generator</param>
        /// <param name="filePath">Path to the file on disk to deserialize</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        public static async Task<AppCast> DeserializeAppCastFromFileAsync(this IAppCastGenerator generator, string filePath, CancellationToken cancellationToken = default)
        {
            using var stream = File.OpenRead(filePath);

            return await generator.DeserializeAppCastAsync(stream, cancellationToken);
        }

        /// <summary>
        /// Serialize the given <see cref="AppCast"/> to a file. Can overwrite
        /// old file, at any, at the given path.
        /// <seealso cref="AppCastItem"/> objects should have at least their version and download link set.
        /// </summary>
        /// <param name="generator">The app cast generator</param>
        /// <param name="appCast"><see cref="AppCast"/> to serialize</param>
        /// <param name="outputPath">Output path for data.</param>
        public static void SerializeAppCastToFile(this IAppCastGenerator generator, AppCast appCast, string outputPath)
        {
            using var stream = File.OpenWrite(outputPath);

            generator.SerializeAppCast(appCast, stream);
        }

        /// <summary>
        /// Serialize the given <see cref="AppCast"/> to a file asyncronously. Can overwrite
        /// old file, at any, at the given path.
        /// <seealso cref="AppCastItem"/> objects should have at least their version and download link set.
        /// </summary>
        /// <param name="generator">The app cast generator</param>
        /// <param name="appCast"><see cref="AppCast"/> to serialize</param>
        /// <param name="outputPath">Output path for data.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <returns>The <seealso cref="Task"/> that can be await'ed for this operation</returns>
        public static async Task SerializeAppCastToFileAsync(this IAppCastGenerator generator, AppCast appCast, string outputPath, CancellationToken cancellationToken = default)
        {
            using var stream = File.OpenWrite(outputPath);

            await generator.SerializeAppCastAsync(appCast, stream, cancellationToken);
        }
    }
}
