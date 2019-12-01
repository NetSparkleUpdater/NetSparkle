using NetSparkle.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkle
{
    /// <summary>
    /// The operation has started
    /// </summary>
    /// <param name="sender">the sender</param>
    public delegate void LoopStartedOperation(object sender);
    /// <summary>
    /// The operation has ended
    /// </summary>
    /// <param name="sender">the sender</param>
    /// <param name="updateRequired"><c>true</c> if an update is required</param>
    public delegate void LoopFinishedOperation(object sender, bool updateRequired);

    /// <summary>
    /// This delegate will be used when an update was detected to allow library 
    /// consumer to add own user interface capabilities.    
    /// </summary>
    public delegate void UpdateDetected(object sender, UpdateDetectedEventArgs e);

    /// <summary>
    /// Update check has started.
    /// </summary>
    /// <param name="sender">Sparkle updater that is checking for an update.</param>
    public delegate void UpdateCheckStarted(object sender);

    /// <summary>
    /// Update check has finished.
    /// </summary>
    /// <param name="sender">Sparkle updater that finished checking for an update.</param>
    /// <param name="status">Update status</param>
    public delegate void UpdateCheckFinished(object sender, UpdateStatus status);

    /// <summary>
    /// An asynchronous cancel event handler.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A System.ComponentModel.CancelEventArgs that contains the event data.</param>
    public delegate Task CancelEventHandlerAsync(object sender, CancelEventArgs e);

    /// <summary>
    /// Handler for when a downloaded file is ready. Useful when using 
    /// SilentModeTypes.DownloadNoInstall so you can let your user know when the downloaded
    /// update is ready.
    /// </summary>
    /// <param name="item">App cast details of the downloaded item</param>
    /// <param name="downloadPath">Path of the downloaded software in case you want to start it yourself</param>
    public delegate void DownloadedFileReady(AppCastItem item, string downloadPath);

    /// <summary>
    /// Called when the file is fully downloaded, but the DSA can't be verified.
    /// This could allow you to tell the user what happened if updates are silent.
    /// Note that silent updates will not delete the corrupted file until the next download loop.
    /// </summary>
    /// <param name="item">App cast details of the downloaded item</param>
    /// <param name="downloadPath">Path of the invalid software download</param>
    public delegate void DownloadedFileIsCorrupt(AppCastItem item, string downloadPath);

    /// <summary>
    /// Delegate called when the user decides to skip a version of the application.
    /// </summary>
    /// <param name="item">Item that the user chose to skip</param>
    /// <param name="downloadPath">Download path of the item so you can delete the download if you want</param>
    public delegate void UserSkippedVersion(AppCastItem item, string downloadPath);

    /// <summary>
    /// Delegate called when the user decides to be reminded about update later.
    /// </summary>
    /// <param name="item">Item that the user chose to skip</param>
    public delegate void RemindMeLaterSelected(AppCastItem item);

    /// <summary>
    /// Delegate for custom application shutdown logic
    /// </summary>
    public delegate void CloseApplication();

    /// <summary>
    /// Async version of CloseApplication().
    /// Delegate for custom application shutdown logic
    /// </summary>
    public delegate Task CloseApplicationAsync();

    /// <summary>
    /// A delegate for download events (start, finished, canceled).
    /// </summary>
    public delegate void DownloadEvent(string path);
}
