using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NetSparkleUpdater.Interfaces
{
    /// <summary>
    /// Interface used by objects that serialize and deserialize
    /// <seealso cref="AppCast"/> objects to/from strings/files.
    /// 
    /// Implement this interface if you would like to use a custom serialize/deserialize
    /// method for your app cast that isn't yet built into NetSparkle.
    /// </summary>
    public interface IAppCastGenerator
    {
        /// <summary>
        /// Deserialize the app cast string into a list of <see cref="AppCastItem"/> objects.
        /// When complete, the <seealso cref="AppCast.Items"/> list should contain the parsed information
        /// as <see cref="AppCastItem"/> objects that are sorted in reverse order like so:
        /// appCast.Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
        /// </summary>
        /// <param name="appCastString">the non-null app cast</param>
        AppCast DeserializeAppCast(string appCastString);

        /// <summary>
        /// Deserialize the app cast string asynchronously into a list of <see cref="AppCastItem"/> objects.
        /// When complete, the <seealso cref="AppCast.Items"/> list should contain the parsed information
        /// as <see cref="AppCastItem"/> objects that are sorted in reverse order like so:
        /// appCast.Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
        /// </summary>
        /// <param name="appCastString">the non-null app cast</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        Task<AppCast> DeserializeAppCastAsync(string appCastString, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserialize the app cast from a file at the given path 
        /// into a list of <see cref="AppCastItem"/> objects.
        /// When complete, the <seealso cref="AppCast.Items"/> list should contain the parsed information
        /// as <see cref="AppCastItem"/> objects that are sorted in reverse order like so:
        /// appCast.Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
        /// </summary>
        /// <param name="stream">Stream containing the app cast data to deserialize</param>
        AppCast DeserializeAppCast(Stream stream);

        /// <summary>
        /// Deserialize the app cast from a file at the given path asynchronously 
        /// into a list of <see cref="AppCastItem"/> objects.
        /// When complete, the <seealso cref="AppCast.Items"/> list should contain the parsed information
        /// as <see cref="AppCastItem"/> objects that are sorted in reverse order like so:
        /// appCast.Items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
        /// </summary>
        /// <param name="stream">Stream containing the app cast data to deserialize</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        Task<AppCast> DeserializeAppCastAsync(Stream stream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serialize the given <see cref="AppCast"/> to a string.
        /// <seealso cref="AppCastItem"/> objects should have at least their version and download link set.
        /// </summary>
        /// <param name="appCast"><see cref="AppCast"/> to serialize</param>
        /// <returns>The string representation of the given <see cref="AppCast"/></returns>
        string SerializeAppCast(AppCast appCast);

        /// <summary>
        /// Serialize the given <see cref="AppCast"/> to a string asyncronously.
        /// <seealso cref="AppCastItem"/> objects should have at least their version and download link set.
        /// </summary>
        /// <param name="appCast"><see cref="AppCast"/> to serialize</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <returns>The string representation of the given <see cref="AppCast"/></returns>
        Task<string> SerializeAppCastAsync(AppCast appCast, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serialize the given <see cref="AppCast"/> to a file. Can overwrite
        /// old file, at any, at the given path.
        /// <seealso cref="AppCastItem"/> objects should have at least their version and download link set.
        /// </summary>
        /// <param name="appCast"><see cref="AppCast"/> to serialize</param>
        /// <param name="stream">Output stream for data.</param>
        void SerializeAppCast(AppCast appCast, Stream stream);

        /// <summary>
        /// Serialize the given <see cref="AppCast"/> to a file asyncronously. Can overwrite
        /// old file, at any, at the given path.
        /// <seealso cref="AppCastItem"/> objects should have at least their version and download link set.
        /// </summary>
        /// <param name="appCast"><see cref="AppCast"/> to serialize</param>
        /// <param name="stream">Output stream for data.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <returns>The <seealso cref="Task"/> that can be await'ed for this operation</returns>
        Task SerializeAppCastAsync(AppCast appCast, Stream stream, CancellationToken cancellationToken = default);
    }
}