using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NetSparkleUpdater.Events
{
    // TODO: Documentation updates -- copied from DownloadProgressChangedEventArgs
    //
    // Summary:
    //     Provides data for the System.Net.WebClient.DownloadProgressChanged event of a
    //     System.Net.WebClient.
    public class ItemDownloadProgressEventArgs : ProgressChangedEventArgs
    {
        public ItemDownloadProgressEventArgs(int progressPercentage, object userState) : base(progressPercentage, userState)
        {
            BytesReceived = 0;
            TotalBytesToReceive = 0;
        }

        public ItemDownloadProgressEventArgs(int progressPercentage, object userState, long bytesReceived, long totalBytesToReceive) : base(progressPercentage, userState)
        {
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
        }

        //
        // Summary:
        //     Gets the number of bytes received.
        //
        // Returns:
        //     An System.Int64 value that indicates the number of bytes received.
        public long BytesReceived { get; private set; }
        //
        // Summary:
        //     Gets the total number of bytes in a System.Net.WebClient data download operation.
        //
        // Returns:
        //     An System.Int64 value that indicates the number of bytes that will be received.
        public long TotalBytesToReceive { get; private set; }
    }
}
