using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NetSparkleUpdater
{
    /// <summary>
    /// The loop that checks for updates every now and again has started
    /// </summary>
    /// <param name="sender">the object that initiated the call</param>
    public delegate void LoopStartedOperation(object sender);

    /// <summary>
    /// The loop that checks for updates has finished checking for updates
    /// </summary>
    /// <param name="sender">the object that initiated the call</param>
    /// <param name="updateRequired"><c>true</c> if an update is required; false otherwise</param>
    public delegate void LoopFinishedOperation(object sender, bool updateRequired);

    /// <summary>
    /// An update was detected for the user's currently running software  
    /// </summary>
    /// <param name="sender">the object that initiated the call</param>
    /// <param name="e">Information about the update that was detected</param>
    public delegate void UpdateDetected(object sender, UpdateDetectedEventArgs e);

    /// <summary>
    /// <see cref="SparkleUpdater"/> has started checking for updates
    /// </summary>
    /// <param name="sender">The <see cref="SparkleUpdater"/> instance that is checking for an update.</param>
    public delegate void UpdateCheckStarted(object sender);

    /// <summary>
    /// <see cref="SparkleUpdater"/> has finished checking for updates
    /// </summary>
    /// <param name="sender"><see cref="SparkleUpdater"/> that finished checking for an update.</param>
    /// <param name="status">Update status (e.g. whether an update is available)</param>
    public delegate void UpdateCheckFinished(object sender, UpdateStatus status);

    /// <summary>
    /// An asynchronous cancel event handler.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A System.ComponentModel.CancelEventArgs that contains the event data.</param>
    public delegate Task CancelEventHandlerAsync(object sender, CancelEventArgs e);

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
    /// A delegate for download events (start, canceled).
    /// </summary>
    public delegate void DownloadEvent(AppCastItem item, string path);

    /// <summary>
    /// Delegate that provides information about some download progress that has been made
    /// </summary>
    /// <param name="sender">The object that initiated the event</param>
    /// <param name="args">The information on how much data has been downloaded and how much
    /// needs to be downloaded</param>
    public delegate void DownloadProgressEvent(object sender, ItemDownloadProgressEventArgs args);

    /// <summary>
    /// Delegate that provides information about some download progress that has been made
    /// when downloading a specific <see cref="AppCastItem"/>.
    /// </summary>
    /// <param name="sender">The object that initiated the event</param>
    /// <param name="item">The item that is being downloaded</param>
    /// <param name="args">The information on how much data has been downloaded and how much
    /// needs to be downloaded</param>
    public delegate void ItemDownloadProgressEvent(object sender, AppCastItem item, ItemDownloadProgressEventArgs args);

    /// <summary>
    /// A handler called when the user responsed to an available update
    /// </summary>
    /// <param name="sender">The object that initiated the event</param>
    /// <param name="e">An UpdateResponse object that contains the information on how the user
    /// responded to the available update (e.g. skip, remind me later). Be warned that 
    /// <see cref="UpdateResponseEventArgs.UpdateItem"/> might be null.</param>
    public delegate void UserRespondedToUpdate(object sender, UpdateResponseEventArgs e);

    /// <summary>
    /// A delegate for a download error that occurred for some reason
    /// </summary>
    /// <param name="item">The item that is being downloaded</param>
    /// <param name="path">The path to the place where the file was being downloaded</param>
    /// <param name="exception">The <seealso cref="Exception"/> that occurred to cause the error</param>
    public delegate void DownloadErrorEvent(AppCastItem item, string path, Exception exception);

    /// <summary>
    /// A delegate for a handling redirects from one URL to another URL manually
    /// </summary>
    /// <param name="fromURL">The original URL that was going to be downloaded from</param>
    /// <param name="toURL">The location that the redirect is pointing to</param>
    /// <param name="responseMessage">The <seealso cref="HttpResponseMessage"/> for the response from the server</param>
    public delegate bool RedirectHandler(string fromURL, string toURL, HttpResponseMessage responseMessage);

    /// <summary>
    /// A delegate to allow users to modify/see the installer process before it actually begins.
    /// Return true to keep the installer process going, return false to have SparkleUpdater not run the installer
    /// (you can choose not to run it at all or run it yourself after that).
    /// </summary>
    /// <param name="process">The installer process about to be started</param>
    /// <param name="downloadFilePath">The path to the downloaded installer that will be started by the new process</param>
    /// <returns>true if the installer should continue, false to not start the installer automatically</returns>
    public delegate bool BeforeBeginInstallerProcess(Process process, string downloadFilePath);

    /// <summary>
    /// Delegate that tells a user why the update installation failed
    /// </summary>
    /// <param name="failureReason"><seealso cref="InstallUpdateFailureReason"/> reason for failure</param>
    /// <param name="installPath">Path for installer (can be null)</param>
    /// <returns></returns>
    public delegate bool InstallUpdateFailure(InstallUpdateFailureReason failureReason, string installPath);
}
