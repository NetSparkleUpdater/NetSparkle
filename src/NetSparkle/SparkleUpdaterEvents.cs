using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace NetSparkleUpdater
{
    public partial class SparkleUpdater : IDisposable
    {
        /// <summary>
        /// This event will be raised when an update check is about to be started
        /// </summary>
        public event LoopStartedOperation LoopStarted;
        /// <summary>
        /// This event will be raised when an update check has finished
        /// </summary>
        public event LoopFinishedOperation LoopFinished;

        /// <summary>
        /// Called when update check has just begun
        /// </summary>
        public event UpdateCheckStarted UpdateCheckStarted;
        /// <summary>
        /// This event can be used to override the standard user interface
        /// process when an update is detected
        /// </summary>
        public event UpdateDetected UpdateDetected;
        /// <summary>
        /// Called when update check is all done. <see cref="UpdateDetected"/> may have been 
        /// called between the start and end of the update check.
        /// </summary>
        public event UpdateCheckFinished UpdateCheckFinished;

        /// <summary>
        /// The user responded to the update UI with a given response.
        /// This is called _after_ NetSparkle handles the response in all cases EXCEPT
        /// when the result is UpdateAvailableResult.InstallUpdate -- in that case, this
        /// event is called BEFORE downloading the update begins.
        /// </summary>
        public event UserRespondedToUpdate UserRespondedToUpdate;

        /// <summary>
        /// Called when the download of an app cast file has just started
        /// </summary>
        public event DownloadEvent DownloadStarted;
        /// <summary>
        /// Called when the download of an app cast file has been canceled
        /// </summary>
        public event DownloadEvent DownloadCanceled;
        /// <summary>
        /// Called when the download of an app cast file has downloaded but has an error other than corruption
        /// </summary>
        public event DownloadErrorEvent DownloadHadError;
        /// <summary>
        /// Called when the download of an app cast file has made some progress. 
        /// This data is also sent to the progress window if one is available.
        /// </summary>
        public event ItemDownloadProgressEvent DownloadMadeProgress;
        /// <summary>
        /// Called when the downloaded file is fully downloaded and verified regardless of the value for
        /// SilentMode. Note that if you are installing fully silently, this will be called before the
        /// install file is executed, so don't manually initiate the file or anything. Useful when using 
        /// SilentModeTypes.DownloadNoInstall so you can let your user know when the downloaded
        /// update is ready.
        /// </summary>
        public event DownloadEvent DownloadFinished;
        /// <summary>
        /// Called when the downloaded file is already downloaded (or at least partially on disk) and the DSA
        /// signature doesn't match. When this is called, Sparkle is not taking any further action to
        /// try to download the install file during this instance of the software. In order to make Sparkle
        /// try again, you must delete the file off disk yourself. Sparkle will try again after the software
        /// is restarted. This event could allow you to tell the user what happened if updates are silent.
        /// </summary>
        public event DownloadEvent DownloadedFileIsCorrupt;
        /// <summary>
        /// Called when the downloaded file is fully downloaded and has its signature checked, but something
        /// went wrong during the signature check.
        /// If you're sure the file is there, you can try verifying the file again AND triggering an install
        /// with SparkleUpdater.InstallUpdate, but make sure to warn your users first!
        /// </summary>
        public event DownloadEvent DownloadedFileThrewWhileCheckingSignature;

        /// <summary>
        /// Subscribe to this event to get a chance to shut down gracefully before the application is closed.
        /// If <see cref="PreparingToExitAsync"/> is set, this has no effect.
        /// </summary>
        public event CancelEventHandler PreparingToExit;
        /// <summary>
        /// Subscribe to this event to get a chance to asynchronously shut down gracefully before the
        /// application is closed.
        /// This overrides <see cref="PreparingToExit"/>.
        /// </summary>
        public event CancelEventHandlerAsync PreparingToExitAsync;

        /// <summary>
        /// Event for custom shutdown logic. If this is set, it is called instead of
        /// Application.Current.Shutdown or Application.Exit.
        /// If <see cref="CloseApplicationAsync"/> is set, this has no effect.
        /// <para>Warning: The script that launches your executable only waits for 90 seconds before
        /// giving up! Make sure that your software closes within 90 seconds if you implement this event!
        /// If you need an event that can be canceled, use <see cref="PreparingToExit"/>.</para>
        /// </summary>
        public event CloseApplication CloseApplication;

        /// <summary>
        /// Event for asynchronous custom shutdown logic. If this is set, it is called instead of
        /// Application.Current.Shutdown or Application.Exit.
        /// This overrides <see cref="CloseApplication"/>.
        /// <para>Warning: The script that launches your executable only waits for 90 seconds before
        /// giving up! Make sure that your software closes within 90 seconds if you implement this event!
        /// If you need an event that can be canceled, use <see cref="PreparingToExitAsync"/>.</para>
        /// </summary>
        public event CloseApplicationAsync CloseApplicationAsync;
    }
}
