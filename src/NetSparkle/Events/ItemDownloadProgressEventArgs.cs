#nullable enable

using NetSparkleUpdater.Interfaces;
using System.ComponentModel;

namespace NetSparkleUpdater.Events
{
    /// <summary>
    /// Provides data for a progress event for downloading an AppCastItem from a
    /// web server.
    /// </summary>
    public class ItemDownloadProgressEventArgs : ProgressChangedEventArgs
    {
        /// <summary>
        /// Create an <see cref="ItemDownloadProgressEventArgs"/> object based on
        /// the total percentage (0-100, inclusive) and the custom user state.
        /// </summary>
        /// <param name="progressPercentage">the total download progress as an int (between 0-100)</param>
        /// <param name="userState">the custom user state sent along with the download progress; 
        /// in NetSparkleUpdater's case, usually the <seealso cref="IUpdateDownloader"/> performing 
        /// the download operation</param>
        public ItemDownloadProgressEventArgs(int progressPercentage, object? userState)
            : this(progressPercentage, userState, 0, 0)
        {
        }

        /// <summary>
        /// Create an <see cref="ItemDownloadProgressEventArgs"/> object based on
        /// the total percentage (0-100, inclusive), the custom user state, the
        /// number of bytes received, and the number of total bytes that need to
        /// be downloaded.
        /// </summary>
        /// <param name="progressPercentage">the total download progress as an int (between 0-100)</param>
        /// <param name="userState">the custom user state sent along with the download progress; 
        /// in NetSparkleUpdater's case, usually the <seealso cref="IUpdateDownloader"/> performing 
        /// the download operation</param>
        /// <param name="bytesReceived">the number of bytes received by the downloader</param>
        /// <param name="totalBytesToReceive">the total number of bytes that need to be downloadeds</param>
        public ItemDownloadProgressEventArgs(int progressPercentage, object? userState, long bytesReceived, long totalBytesToReceive) : base(progressPercentage, userState)
        {
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
        }

        /// <summary>
        /// The number of bytes received by the downloader
        /// </summary>
        public long BytesReceived { get; private set; }
        
        /// <summary>
        /// The total number of bytes that need to be downloaded
        /// </summary>
        public long TotalBytesToReceive { get; private set; }
    }
}
