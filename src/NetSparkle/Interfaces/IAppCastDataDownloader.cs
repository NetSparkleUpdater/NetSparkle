using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater.Interfaces
{
    /// <summary>
    /// Interface used by objects that can download app casts from the internet.
    /// Use this interface to provide a custom method of grabbing your app cast
    /// from the internet or from a local file.
    /// </summary>
    public interface IAppCastDataDownloader
    {
        /// <summary>
        /// Download a string of data at the given URL.
        /// Used for both downloading app cast and the app cast's .signature file.
        /// Note that you must handle your own exceptions if they occur. 
        /// Otherwise, <see cref="SparkleUpdater"></see> will act as though the appcast 
        /// failed to download.
        /// </summary>
        /// <param name="url">non-null string URL for the place where the app cast can be downloaded</param>
        /// <returns>The app cast data encoded as a string</returns>
        string DownloadAndGetAppCastData(string url);

        /// <summary>
        /// Async download a string of data at the given URL.
        /// Used for both downloading app cast and the app cast's .signature file.
        /// Note that you must handle your own exceptions if they occur. 
        /// Otherwise, <see cref="SparkleUpdater"></see> will act as though the appcast 
        /// failed to download.
        /// </summary>
        /// <param name="url">non-null string URL for the place where the app cast can be downloaded</param>
        /// <returns>The app cast data encoded as a string</returns>
        Task<string> DownloadAndGetAppCastDataAsync(string url);

        /// <summary>
        /// Get the string encoding (e.g. UTF8 or ASCII) of the 
        /// app cast file so that it can be converted to bytes.
        /// (WebRequestAppCastDataDownloader defaults to UTF8.)
        /// </summary>
        /// <returns>The <seealso cref="Encoding"/> of the app cast</returns>
        Encoding GetAppCastEncoding();
    }
}
