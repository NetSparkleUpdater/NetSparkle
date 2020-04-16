using System;
using System.ComponentModel;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using NetSparkle.Interfaces;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using NetSparkle.Enums;
using System.Net.Http;
using NetSparkle.Events;
using System.Collections.Generic;
using NetSparkle.Downloaders;
using NetSparkle.Configurations;
#if NETSTANDARD
using System.Runtime.InteropServices;
#endif

namespace NetSparkle
{
    /// <summary>
    /// Class to communicate with a sparkle-based appcast
    /// </summary>
    public class Sparkle : IDisposable
    {
        #region Events

        /// <summary>
        /// Subscribe to this to get a chance to shut down gracefully before quitting.
        /// If <see cref="AboutToExitForInstallerRunAsync"/> is set, this has no effect.
        /// </summary>
        public event CancelEventHandler AboutToExitForInstallerRun;

        /// <summary>
        /// Subscribe to this to get a chance to asynchronously shut down gracefully before quitting.
        /// This overrides <see cref="AboutToExitForInstallerRun"/>.
        /// </summary>
        public event CancelEventHandlerAsync AboutToExitForInstallerRunAsync;

        /// <summary>
        /// This event will be raised when a check loop will be started
        /// </summary>
        public event LoopStartedOperation CheckLoopStarted;

        /// <summary>
        /// This event will be raised when a check loop is finished
        /// </summary>
        public event LoopFinishedOperation CheckLoopFinished;

        /// <summary>
        /// This event can be used to override the standard user interface
        /// process when an update is detected
        /// </summary>
        public event UpdateDetected UpdateDetected;

        /// <summary>
        /// Event for custom shutdown logic. If this is set, it is called instead of
        /// Application.Current.Shutdown or Application.Exit.
        /// If <see cref="CloseApplicationAsync"/> is set, this has no effect.
        /// <para>Warning: The batch file that launches your executable only waits for 90 seconds before
        /// giving up! Make sure that your software closes within 90 seconds if you implement this event!
        /// If you need an event that can be canceled, use <see cref="AboutToExitForInstallerRun"/>.</para>
        /// </summary>
        public event CloseApplication CloseApplication;

        /// <summary>
        /// Event for asynchronous custom shutdown logic. If this is set, it is called instead of
        /// Application.Current.Shutdown or Application.Exit.
        /// This overrides <see cref="CloseApplication"/>.
        /// <para>Warning: The batch file that launches your executable only waits for 90 seconds before
        /// giving up! Make sure that your software closes within 90 seconds if you implement this event!
        /// If you need an event that can be canceled, use <see cref="AboutToExitForInstallerRunAsync"/>.</para>
        /// </summary>
        public event CloseApplicationAsync CloseApplicationAsync;

        /// <summary>
        /// Called when update check has just started
        /// </summary>
        public event UpdateCheckStarted UpdateCheckStarted;

        /// <summary>
        /// Called when update check is all done. May or may not have called <see cref="UpdateDetected"/> in the middle.
        /// </summary>
        public event UpdateCheckFinished UpdateCheckFinished;

        /// <summary>
        /// Called when the downloaded file is fully downloaded and verified regardless of the value for
        /// SilentMode. Note that if you are installing fully silently, this will be called before the
        /// install file is executed, so don't manually initiate the file or anything.
        /// </summary>
        public event DownloadedFileReady DownloadedFileReady;

        /// <summary>
        /// Called when the downloaded file is downloaded (or at least partially on disk) and the DSA
        /// signature doesn't match. When this is called, Sparkle is not taking any further action to
        /// try to download the install file during this instance of the software. In order to make Sparkle
        /// try again, you must delete the file off disk yourself. Sparkle will try again after the software
        /// is restarted.
        /// </summary>
        public event DownloadedFileIsCorrupt DownloadedFileIsCorrupt;

        /// <summary>
        /// Called when the user skips some version of the application.
        /// </summary>
        public event UserSkippedVersion UserSkippedVersion;
        /// <summary>
        /// Called when the user skips some version of the application by clicking
        /// the 'Remind Me Later' button.
        /// </summary>
        public event RemindMeLaterSelected RemindMeLaterSelected;
        /// <summary>
        /// Called when the download has just started
        /// </summary>
        public event DownloadEvent StartedDownloading;
        /// <summary>
        /// Called when the download has finished successfully
        /// </summary>
        public event DownloadEvent FinishedDownloading;
        /// <summary>
        /// Called when the download has been canceled
        /// </summary>
        public event DownloadEvent DownloadCanceled;
        /// <summary>
        /// Called when the download has downloaded but has an error other than corruption
        /// </summary>
        public event DownloadEvent DownloadError;

        #endregion

        #region Protected/Private Members
        
        protected Process _installerProcess;

        private LogWriter _logWriter;
        private readonly Task _taskWorker;
        private CancellationToken _cancelToken;
        private readonly CancellationTokenSource _cancelTokenSource;
        private readonly SynchronizationContext _syncContext;
        private string _appCastUrl;
        private readonly string _appReferenceAssembly;

        private bool _doInitialCheck;
        private bool _forceInitialCheck;

        private readonly EventWaitHandle _exitHandle;
        private readonly EventWaitHandle _loopingHandle;
        private TimeSpan _checkFrequency;
        private bool _useNotificationToast;

        private string _tmpDownloadFilePath;
        private string _downloadTempFileName;
        private AppCastItem _itemBeingDownloaded;
        private bool _hasAttemptedFileRedownload;
        private UpdateInfo _latestDownloadedUpdateInfo;
        private IUIFactory _uiFactory;
        private bool _disposed;
        private bool _checkServerFileName = true;

        private Configuration _configuration;
        private IUpdateDownloader _updateDownloader;
        private IAppCastDataDownloader _appCastDataDownloader;
        private IAppCastHandler _appCastHandler;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Sparkle"/> class with the given appcast URL
        /// and an <see cref="Icon"/> for the update UI.
        /// </summary>
        /// <param name="appcastUrl">the URL of the appcast file</param>
        /// <param name="applicationIcon"><see cref="Icon"/> to be displayed in the update UI.
        /// If you're invoking this from a form, this would be <c>this.Icon</c>.</param>
        public Sparkle(string appcastUrl)
            : this(appcastUrl, SecurityMode.Strict, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url
        /// </summary>
        /// <param name="appcastUrl">the URL of the appcast file</param>
        /// <param name="securityMode">the security mode to be used when checking DSA signatures</param>
        public Sparkle(string appcastUrl, SecurityMode securityMode)
            : this(appcastUrl, securityMode, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url
        /// </summary>
        /// <param name="appcastUrl">the URL of the appcast file</param>
        /// <param name="securityMode">the security mode to be used when checking DSA signatures</param>
        /// <param name="dsaPublicKey">the DSA public key for checking signatures, in XML Signature (&lt;DSAKeyValue&gt;) format.
        /// If null, a file named "NetSparkle_DSA.pub" is used instead.</param>
        public Sparkle(string appcastUrl, SecurityMode securityMode, string dsaPublicKey)
            : this(appcastUrl, securityMode, dsaPublicKey, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url and a referenceassembly
        /// </summary>        
        /// <param name="appcastUrl">the URL of the appcast file</param>
        /// <param name="securityMode">the security mode to be used when checking DSA signatures</param>
        /// <param name="dsaPublicKey">the DSA public key for checking signatures, in XML Signature (&lt;DSAKeyValue&gt;) format.
        /// If null, a file named "NetSparkle_DSA.pub" is used instead.</param>
        /// <param name="referenceAssembly">the name of the assembly to use for comparison when checking update versions</param>
        public Sparkle(string appcastUrl, SecurityMode securityMode, string dsaPublicKey, string referenceAssembly)
            : this(appcastUrl, securityMode, dsaPublicKey, referenceAssembly, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url and a referenceassembly
        /// </summary>        
        /// <param name="appcastUrl">the URL of the appcast file</param>
        /// <param name="securityMode">the security mode to be used when checking DSA signatures</param>
        /// <param name="dsaPublicKey">the DSA public key for checking signatures, in XML Signature (&lt;DSAKeyValue&gt;) format.
        /// If null, a file named "NetSparkle_DSA.pub" is used instead.</param>
        /// <param name="referenceAssembly">the name of the assembly to use for comparison when checking update versions</param>
        /// <param name="factory">a UI factory to use in place of the default UI</param>
        public Sparkle(string appcastUrl, SecurityMode securityMode, string dsaPublicKey, string referenceAssembly, IUIFactory factory)
        {
            ExtraJsonData = "";
            _latestDownloadedUpdateInfo = null;
            _hasAttemptedFileRedownload = false;
            UIFactory = factory;
            DSAChecker = new DSAChecker(securityMode, dsaPublicKey);
            // Syncronisation Context
            _syncContext = SynchronizationContext.Current;
            if (_syncContext == null)
            {
                _syncContext = new SynchronizationContext();
            }
            TrustEverySSLConnection = false;
            // configure ssl cert link
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            // init UI
            UIFactory?.Init();
            _appReferenceAssembly = null;
            // set the reference assembly
            if (referenceAssembly != null)
            {
                _appReferenceAssembly = referenceAssembly;
                LogWriter.PrintMessage("Checking the following file: " + _appReferenceAssembly);
            }

            // adjust the delegates
            _taskWorker = new Task(() =>
            {
                OnWorkerDoWork(null, null);
            });
            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;

            // build the wait handle
            _exitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            _loopingHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            // set the url
            _appCastUrl = appcastUrl;
            LogWriter.PrintMessage("Using the following url: {0}", _appCastUrl);
            UserInteractionMode = UserInteractionMode.NotSilent;
            TmpDownloadFilePath = "";
            HideSkipButton = false;
            HideRemindMeLaterButton = false;
        }

        /// <summary>
        /// The security protocol used by NetSparkle. Setting this property will also set this 
        /// for the current AppDomain of the caller. Needs to be set to 
        /// SecurityProtocolType.Tls12 for some cases.
        /// </summary>
        public SecurityProtocolType SecurityProtocolType
        {
            get
            {
                return ServicePointManager.SecurityProtocol;
            }
            set
            {
                ServicePointManager.SecurityProtocol = value;
            }
        }

        #region Properties

        /// <summary>
        /// Hides the release notes view when an update is found.
        /// </summary>
        public bool HideReleaseNotes { get; set; }

        /// <summary>
        /// Hides the skip this update button when an update is found.
        /// </summary>
        public bool HideSkipButton { get; set; }

        /// <summary>
        /// Hides the remind me later button when an update is found.
        /// </summary>
        public bool HideRemindMeLaterButton { get; set; }

        /// <summary>
        /// Set the user interaction mode for Sparkle to use when there is a valid update for the software
        /// </summary>
        public UserInteractionMode UserInteractionMode { get; set; }

        /// <summary>
        /// If set, downloads files to this path. If the folder doesn't already exist, creates
        /// the folder at download time (and not before). 
        /// Note that this variable is a path, not a full file name.
        /// </summary>
        public string TmpDownloadFilePath
        {
            get { return _tmpDownloadFilePath; }
            set { _tmpDownloadFilePath = value?.Trim(); }
        }

        /// <summary>
        /// Defines if the application needs to be relaunched after executing the downloaded installer
        /// </summary>
        public bool RelaunchAfterUpdate { get; set; }

        /// <summary>
        /// Run the downloaded installer with these arguments
        /// </summary>
        public string CustomInstallerArguments { get; set; }

        /// <summary>
        /// Function that is called asynchronously to clean up old installers that have been
        /// downloaded with SilentModeTypes.DownloadNoInstall or SilentModeTypes.DownloadAndInstall.
        /// </summary>
        public Action ClearOldInstallers { get; set; }

        /// <summary>
        /// Whether or not the update loop is running
        /// </summary>
        public bool IsUpdateLoopRunning
        {
            get
            {
                return _loopingHandle.WaitOne(0);
            }
        }

        /// <summary>
        /// If true, don't check the validity of SSL certificates
        /// </summary>
        public bool TrustEverySSLConnection { get; set; }

        /// <summary>
        /// Factory for creating UI elements like progress window, etc.
        /// </summary>
        public IUIFactory UIFactory
        { 
            get { return _uiFactory; } 
            set { _uiFactory = value; _uiFactory?.Init(); }
        }

        /// <summary>
        /// The user interface window that shows the release notes and
        /// asks the user to skip, remind me later, or update
        /// </summary>
        public IUpdateAvailable UpdateAvailableWindow { get; set; }

        /// <summary>
        /// The user interface window that shows a download progress bar,
        /// and then asks to install and relaunch the application
        /// </summary>
        public IDownloadProgress ProgressWindow { get; set; }

        /// <summary>
        /// The user interface window that shows the 'Checking for Updates...'
        /// form.
        /// </summary>
        public ICheckingForUpdates CheckingForUpdatesWindow { get; set; }

        /// <summary>
        /// The NetSparkle configuration object for the current assembly.
        /// </summary>
        public Configuration Configuration 
        { 
            get
            {
                if (_configuration == null)
                {
#if NETSTANDARD
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _configuration = new RegistryConfiguration(_appReferenceAssembly);
                    }
                    else
                    {
                        _configuration = new JSONConfiguration(_appReferenceAssembly);
                    }
#else
                    _configuration = new RegistryConfiguration(_appReferenceAssembly);
#endif
                }
                _configuration.Reload();
                return _configuration;
            } 
            set { _configuration = value; } 
        }

        /// <summary>
        /// The DSA checker
        /// </summary>
        public DSAChecker DSAChecker { get; set; }

        /// <summary>
        /// Gets or sets the appcast URL
        /// </summary>
        public string AppcastUrl
        {
            get { return _appCastUrl; }
            set { _appCastUrl = value; }
        }

        /// <summary>
        /// Specifies if you want to use the notification toast
        /// </summary>
        public bool UseNotificationToast
        {
            get { return _useNotificationToast; }
            set { _useNotificationToast = value; }
        }

        /// <summary>
        /// WinForms only. If true, tries to run UI code on the main thread using <see cref="SynchronizationContext"/>.
        /// </summary>
        public bool ShowsUIOnMainThread { get; set; }

        /// <summary>
        /// If not "", sends extra JSON via POST to server with the web request for update information and for the DSA signature.
        /// </summary>
        public string ExtraJsonData { get; set; }

        /// <summary>
        /// Object that handles any diagnostic messages for NetSparkle.
        /// If you want to use your own class for this, you should just
        /// need to override <see cref="LogWriter.PrintMessage"/> in your own class.
        /// Make sure to set this object before calling <see cref="StartLoop(bool)"/> to guarantee
        /// that all messages will get sent to the right place!
        /// </summary>
        public LogWriter LogWriter
        {
            get
            {
                if (_logWriter == null)
                {
                    _logWriter = new LogWriter();
                }
                return _logWriter;
            }
            set
            {
                _logWriter = value;
            }
        }

        /// <summary>
        /// Whether or not to check with the online server to verify download
        /// file names.
        /// </summary>
        public bool CheckServerFileName
        {
            get { return _checkServerFileName; }
            set { _checkServerFileName = value; }
        }

        /// <summary>
        /// Returns the latest appcast items to the caller. Might be null.
        /// </summary>
        public List<AppCastItem> LatestAppCastItems
        {
            get
            {
                return _latestDownloadedUpdateInfo?.Updates;
            }
        }

        /// <summary>
        /// Loops through all of the most recently grabbed app cast items
        /// and checks if any of them are marked as critical
        /// </summary>
        public bool UpdateMarkedCritical
        {
            get
            {
                if (LatestAppCastItems != null)
                {
                    foreach (AppCastItem item in LatestAppCastItems)
                    {
                        if (item.IsCriticalUpdate)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public IUpdateDownloader UpdateDownloader
        {
            get { return _updateDownloader; }
            set { _updateDownloader = value; }
        }

        public IAppCastDataDownloader AppCastDataDownloader
        {
            get { return _appCastDataDownloader; }
            set { _appCastDataDownloader = value; }
        }

        public IAppCastHandler AppCastHandler
        {
            get { return _appCastHandler; }
            set { _appCastHandler = value; }
        }

#endregion

        /// <summary>
        /// Starts a NetSparkle background loop to check for updates every 24 hours.
        /// <para>You should only call this function when your app is initialized and shows its main window.</para>
        /// </summary>
        /// <param name="doInitialCheck">whether the first check should happen before or after the first interval</param>
        public void StartLoop(bool doInitialCheck)
        {
            StartLoop(doInitialCheck, false);
        }

        /// <summary>
        /// Starts a NetSparkle background loop to check for updates on a given interval.
        /// <para>You should only call this function when your app is initialized and shows its main window.</para>
        /// </summary>
        /// <param name="doInitialCheck">whether the first check should happen before or after the first interval</param>
        /// <param name="checkFrequency">the interval to wait between update checks</param>
        public void StartLoop(bool doInitialCheck, TimeSpan checkFrequency)
        {
            StartLoop(doInitialCheck, false, checkFrequency);
        }

        /// <summary>
        /// Starts a NetSparkle background loop to check for updates every 24 hours.
        /// <para>You should only call this function when your app is initialized and shows its main window.</para>
        /// </summary>
        /// <param name="doInitialCheck">whether the first check should happen before or after the first interval</param>
        /// <param name="forceInitialCheck">if <paramref name="doInitialCheck"/> is true, whether the first check
        /// should happen even if the last check was less than 24 hours ago</param>
        public void StartLoop(bool doInitialCheck, bool forceInitialCheck)
        {
            StartLoop(doInitialCheck, forceInitialCheck, TimeSpan.FromHours(24));
        }

        /// <summary>
        /// Starts a NetSparkle background loop to check for updates on a given interval.
        /// <para>You should only call this function when your app is initialized and shows its main window.</para>
        /// </summary>
        /// <param name="doInitialCheck">whether the first check should happen before or after the first period</param>
        /// <param name="forceInitialCheck">if <paramref name="doInitialCheck"/> is true, whether the first check
        /// should happen even if the last check was within the last <paramref name="checkFrequency"/> interval</param>
        /// <param name="checkFrequency">the interval to wait between update checks</param>
        public async void StartLoop(bool doInitialCheck, bool forceInitialCheck, TimeSpan checkFrequency)
        {
            if (ClearOldInstallers != null)
            {
                try
                {
                    await Task.Run(ClearOldInstallers);
                }
                catch
                {
                    LogWriter.PrintMessage("ClearOldInstallers threw an exception");
                }
            }
            // first set the event handle
            _loopingHandle.Set();

            // Start the helper thread as a background worker to 
            // get well ui interaction                        

            // store infos
            _doInitialCheck = doInitialCheck;
            _forceInitialCheck = forceInitialCheck;
            _checkFrequency = checkFrequency;

            LogWriter.PrintMessage("Starting background worker");

            // start the work
            //var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            //_taskWorker.Start(scheduler);
            _taskWorker.Start();
        }

        /// <summary>
        /// Stops the Sparkle background loop. Called automatically by <see cref="Dispose()"/>.
        /// </summary>
        public void StopLoop()
        {
            // ensure the work will finished
            _exitHandle.Set();
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~Sparkle()
        {
            Dispose(false);
        }

#region IDisposable

        /// <summary>
        /// Inherited from IDisposable. Stops all background activities.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of managed and unmanaged resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    StopLoop();
                    UnregisterEvents();
                    _cancelTokenSource?.Dispose();
                    _exitHandle?.Dispose();
                    _loopingHandle?.Dispose();
                    UpdateDownloader?.Dispose();
                    _installerProcess?.Dispose();
                }
                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.
            }
            _disposed = true;
        }

        /// <summary>
        /// Unregisters events so that we don't have multiple items updating
        /// </summary>
        private void UnregisterEvents()
        {
            ServicePointManager.ServerCertificateValidationCallback -= ValidateRemoteCertificate;
            _cancelTokenSource.Cancel();

            CleanUpUpdateDownloader();

            if (UpdateAvailableWindow != null)
            {
                UpdateAvailableWindow.UserResponded -= OnUserWindowUserResponded;
                UpdateAvailableWindow = null;
            }

            if (ProgressWindow != null)
            {
                ProgressWindow.DownloadProcessCompleted -= ProgressWindowCompleted;
                ProgressWindow = null;
            }
        }

#endregion

        /// <summary>
        /// This method checks if an update is required. During this process the appcast
        /// will be downloaded and checked against the reference assembly. Ensure that
        /// the calling process has read access to the reference assembly.
        /// This method is also called from the background loops.
        /// </summary>
        /// <param name="config">the NetSparkle configuration for the reference assembly</param>
        /// <returns><see cref="UpdateInfo"/> with information on whether there is an update available or not.</returns>
        public async Task<UpdateInfo> GetUpdateStatus(Configuration config)
        {
            List<AppCastItem> updates = null;
            // report
            LogWriter.PrintMessage("Downloading and checking appcast");

            // init the appcast
            if (AppCastDataDownloader == null)
            {
                AppCastDataDownloader = new WebRequestAppCastDataDownloader(TrustEverySSLConnection, ExtraJsonData);
            }
            IAppCastHandler appcastHandler = AppCastHandler;
            if (appcastHandler == null)
            {
                appcastHandler = AppCastHandler = new XMLAppCast();
            }
            appcastHandler.SetupAppCastHandler(AppCastDataDownloader, _appCastUrl, config, DSAChecker, LogWriter);
            // check if any updates are available
            try
            {
                var task = Task.Factory.StartNew(() =>
                {
                    if (appcastHandler.DownloadAndParse())
                    {
                        updates = appcastHandler.GetNeededUpdates();
                    }
                });
                await task;
            }
            catch (Exception e)
            {
                LogWriter.PrintMessage("Couldn't read/parse the app cast: {0}", e.Message);
                updates = null;
            }

            if (updates == null)
            {
                LogWriter.PrintMessage("No version information in app cast found");
                return new UpdateInfo(UpdateStatus.CouldNotDetermine);
            }

            // set the last check time
            LogWriter.PrintMessage("Touch the last check timestamp");
            config.TouchCheckTime();

            // check if the version will be the same then the installed version
            if (updates.Count == 0)
            {
                LogWriter.PrintMessage("Installed version is valid, no update needed ({0})", config.InstalledVersion);
                return new UpdateInfo(UpdateStatus.UpdateNotAvailable);
            }
            LogWriter.PrintMessage("Latest version on the server is {0}", updates[0].Version);

            // check if the available update has to be skipped
            if (updates[0].Version.Equals(config.LastVersionSkipped))
            {
                LogWriter.PrintMessage("Latest update has to be skipped (user decided to skip version {0})", config.LastVersionSkipped);
                return new UpdateInfo(UpdateStatus.UserSkipped);
            }

            // ok we need an update
            return new UpdateInfo(UpdateStatus.UpdateAvailable, updates);
        }

        /// <summary>
        /// Shows the update needed UI with the given set of updates.
        /// </summary>
        /// <param name="updates">updates to show UI for</param>
        /// <param name="isUpdateAlreadyDownloaded">If true, make sure UI text shows that the user is about to install the file instead of download it.</param>
        public void ShowUpdateNeededUI(List<AppCastItem> updates, bool isUpdateAlreadyDownloaded = false)
        {
            if (updates != null)
            {
                if (_useNotificationToast)
                {
                    UIFactory?.ShowToast(updates, OnToastClick);
                }
                else
                {
                    ShowUpdateNeededUIInner(updates, isUpdateAlreadyDownloaded);
                }
            }
        }

        /// <summary>
        /// Shows the update UI with the latest downloaded update information.
        /// </summary>
        /// <param name="isUpdateAlreadyDownloaded">If true, make sure UI text shows that the user is about to install the file instead of download it.</param>
        public void ShowUpdateNeededUI(bool isUpdateAlreadyDownloaded = false)
        {
            ShowUpdateNeededUI(_latestDownloadedUpdateInfo?.Updates, isUpdateAlreadyDownloaded);
        }

        private void OnToastClick(List<AppCastItem> updates)
        {
            ShowUpdateNeededUIInner(updates);
        }

        private void ShowUpdateNeededUIInner(List<AppCastItem> updates, bool isUpdateAlreadyDownloaded = false)
        {
            // TODO: In the future, instead of remaking the window, just send the new data to the old window
            if (UpdateAvailableWindow != null)
            {
                // close old window
                if (ShowsUIOnMainThread)
                {
                    _syncContext.Post((state) =>
                    {
                        UpdateAvailableWindow.Close();
                        UpdateAvailableWindow = null;
                    }, null);
                }
                else
                {
                    UpdateAvailableWindow.Close();
                    UpdateAvailableWindow = null;
                }
            }

            // create the form
            Thread thread = new Thread(() =>
            {
                try
                {
                    // define action
                    Action<object> showSparkleUI = (state) =>
                    {
                        UpdateAvailableWindow = UIFactory?.CreateUpdateAvailableWindow(this, updates, isUpdateAlreadyDownloaded);

                        if (UpdateAvailableWindow != null)
                        {
                            if (HideReleaseNotes)
                            {
                                UpdateAvailableWindow.HideReleaseNotes();
                            }
                            if (HideSkipButton)
                            {
                                UpdateAvailableWindow.HideSkipButton();
                            }
                            if (HideRemindMeLaterButton)
                            {
                                UpdateAvailableWindow.HideRemindMeLaterButton();
                            }

                            // clear if already set.
                            UpdateAvailableWindow.UserResponded += OnUserWindowUserResponded;
                            UpdateAvailableWindow.Show(ShowsUIOnMainThread);
                        }
                    };

                    // call action
                    if (ShowsUIOnMainThread)
                    {
                        _syncContext.Post((state) => showSparkleUI(state), null);
                    }
                    else
                    {
                        showSparkleUI(null);
                    }
                }
                catch (Exception e)
                {
                    LogWriter.PrintMessage("Error showing sparkle form: {0}", e.Message);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private async Task<string> RetrieveDestinationFileNameAsync(AppCastItem item)
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            LogWriter.PrintMessage("Getting file name from server for app cast item. Download link is {0}", item.DownloadLink);
            try
            {
                using (var response =
                    await httpClient.GetAsync(item.DownloadLink, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    LogWriter.PrintMessage("Got response. Successful? {0}", response.IsSuccessStatusCode);
                    if (response.IsSuccessStatusCode)
                    {
                        //var totalBytes = response.Content.Headers.ContentLength; // TODO: Use this value as well for a more accurate download %?
                        string destFilename = response.RequestMessage?.RequestUri?.LocalPath;

                        return Path.GetFileName(destFilename);
                    }
                    return null;
                }
            }
            catch (Exception e)
            {
                LogWriter.PrintMessage("Got an exception while getting file name: {0}", e.Message);
            }
            return null;
        }

        /// <summary>
        /// Get the download path for a given app cast item.
        /// If any directories need to be created, this function
        /// will create those directories.
        /// </summary>
        /// <param name="item">The item that you want to generate a download path for</param>
        /// <returns>The download path for an app cast item if item is not null and has valid download link
        /// Otherwise returns null.</returns>
        public async Task<string> DownloadPathForAppCastItem(AppCastItem item)
        {
            if (item != null && item.DownloadLink != null)
            {
                string filename = string.Empty;

                // default to using the server's file name as the download file name
                if (CheckServerFileName)
                {
                    try
                    {
                        filename = await RetrieveDestinationFileNameAsync(item);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }

                if (string.IsNullOrEmpty(filename))
                {
                    // attempt to get download file name based on download link
                    try
                    {
                        filename = Path.GetFileName(new Uri(item.DownloadLink).LocalPath);
                    }
                    catch (UriFormatException)
                    {
                        // ignore
                    }
                }

                if (!string.IsNullOrEmpty(filename))
                {
                    string tmpPath = string.IsNullOrEmpty(TmpDownloadFilePath) ? Path.GetTempPath() : TmpDownloadFilePath;

                    // Creates all directories and subdirectories in the specific path unless they already exist.
                    Directory.CreateDirectory(tmpPath);

                    return Path.Combine(tmpPath, filename);
                }
            }
            return null;
        }

        /// <summary>
        /// Starts the download process
        /// </summary>
        /// <param name="item">the appcast item to download</param>
        private async Task InitDownloadAndInstallProcess(AppCastItem item)
        {
            // TODO: is this a good idea? What if it's a user initiated request,
            // and they want to watch progress instead of it being a silent download?
            if (UpdateDownloader != null && UpdateDownloader.IsDownloading)
            {
                return; // file is already downloading, don't do anything!
            }
            LogWriter.PrintMessage("Preparing to download {0}", item.DownloadLink);
            _itemBeingDownloaded = item;
            _downloadTempFileName = await DownloadPathForAppCastItem(item);
            // Make sure the file doesn't already exist on disk. If it's already downloaded and the
            // DSA signature checks out, don't redownload the file!
            bool needsToDownload = true;
            if (File.Exists(_downloadTempFileName))
            {
                ValidationResult result = DSAChecker.VerifyDSASignatureFile(item.DownloadDSASignature, _downloadTempFileName);
                if (result == ValidationResult.Valid)
                {
                    LogWriter.PrintMessage("File is already downloaded");
                    // We already have the file! Don't redownload it!
                    needsToDownload = false;
                    // Still need to set up the ProgressWindow for non-silent downloads, though,
                    // so that the user can actually perform the install
                    CreateAndShowProgressWindow(item, true);
                }
                else if (!_hasAttemptedFileRedownload)
                {
                    // The file exists but it either has a bad DSA signature or SecurityMode is set to Unsafe.
                    // Redownload it!
                    _hasAttemptedFileRedownload = true;
                    LogWriter.PrintMessage("File is corrupt or DSA signature is Unchecked; deleting file and redownloading...");
                    try
                    {
                        File.Delete(_downloadTempFileName);
                    }
                    catch (Exception e)
                    {
                        LogWriter.PrintMessage("Hm, seems as though we couldn't delete the temporary file even though it is apparently corrupt. {0}",
                            e.Message);
                        // we won't be able to download anyway since we couldn't delete the file :( we'll try next time the
                        // update loop goes around.
                        needsToDownload = false;
                    }
                }
                else
                {
                    CallFuncConsideringUIThreads(() => { DownloadedFileIsCorrupt?.Invoke(item, _downloadTempFileName); });
                }
            }
            if (needsToDownload)
            {

                CleanUpUpdateDownloader();
                CreateUpdateDownloaderIfNeeded();

                CreateAndShowProgressWindow(item, false, () =>
                {
                    if (ProgressWindow != null)
                    {
                        UpdateDownloader.DownloadProgressChanged += ProgressWindow.OnDownloadProgressChanged;
                    }
                    UpdateDownloader.DownloadFileCompleted += OnDownloadFinished;

                    Uri url = Utilities.GetAbsoluteURL(item.DownloadLink, AppcastUrl);
                    LogWriter.PrintMessage("Starting to download {0} to {1}", item.DownloadLink, _downloadTempFileName);
                    UpdateDownloader.StartFileDownload(url, _downloadTempFileName);
                    CallFuncConsideringUIThreads(() => { StartedDownloading?.Invoke(_downloadTempFileName); });
                });
            }
        }

        private void CleanUpUpdateDownloader()
        {
            if (UpdateDownloader != null)
            {
                if (ProgressWindow != null)
                {
                    UpdateDownloader.DownloadProgressChanged -= ProgressWindow.OnDownloadProgressChanged;
                }
                UpdateDownloader.DownloadFileCompleted -= OnDownloadFinished;
                UpdateDownloader?.Dispose();
                UpdateDownloader = null;
            }
        }

        private void CreateUpdateDownloaderIfNeeded()
        {
            if (UpdateDownloader == null)
            {
                UpdateDownloader = new WebClientFileDownloader();
            }
        }

        private void CreateAndShowProgressWindow(AppCastItem castItem, bool shouldShowAsDownloadedAlready, Action actionToRunOnceCreatedBeforeShown = null)
        {
            if (ProgressWindow != null)
            {
                ProgressWindow.DownloadProcessCompleted -= ProgressWindowCompleted;
                ProgressWindow = null;
            }
            if (ProgressWindow == null && !IsDownloadingSilently())
            {
                if (!IsDownloadingSilently() && ProgressWindow == null)
                {
                    // create the form
                    Action<object> showSparkleDownloadUI = (state) =>
                    {
                        ProgressWindow = UIFactory?.CreateProgressWindow(castItem);
                        ProgressWindow.DownloadProcessCompleted += ProgressWindowCompleted;
                        if (shouldShowAsDownloadedAlready)
                        {
                            ProgressWindow?.FinishedDownloadingFile(true);
                            _syncContext.Post((state2) => OnDownloadFinished(null, new AsyncCompletedEventArgs(null, false, null)), null);
                        }

                        actionToRunOnceCreatedBeforeShown?.Invoke();
                    };
                    Thread thread = new Thread(() =>
                    {
                        // call action
                        if (ShowsUIOnMainThread)
                        {
                            _syncContext.Post((state) =>
                            {
                                showSparkleDownloadUI(null);
                                ProgressWindow?.Show(ShowsUIOnMainThread);
                            }, null);
                        }
                        else
                        {
                            showSparkleDownloadUI(null);
                            ProgressWindow?.Show(ShowsUIOnMainThread);
                        }
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
            }
        }

        private async void ProgressWindowCompleted(object sender, DownloadInstallArgs args)
        {
            if (args.ShouldInstall)
            {
                ProgressWindow?.SetDownloadAndInstallButtonEnabled(false); // disable while we ask if we can close up the software
                if (await AskApplicationToSafelyCloseUp())
                {
                    ProgressWindow?.Close();
                    await RunDownloadedInstaller(_downloadTempFileName);
                }
                else
                {
                    ProgressWindow?.SetDownloadAndInstallButtonEnabled(true);
                }
            }
            else
            {
                CancelFileDownload();
                ProgressWindow?.Close();
            }
        }

        /// <summary>
        /// Run the provided app cast item update regardless of what else is going on.
        /// Note that a more up to date download may be taking place, so if you don't
        /// want to run a potentially out-of-date installer, don't use this. This should
        /// only be used if your user wants to update before another update has been
        /// installed AND the file is already downloaded.
        /// This function will verify that the file exists and that the DSA 
        /// signature is valid before running. It will also utilize the
        /// AboutToExitForInstallerRun event to ensure that the application can close.
        /// </summary>
        /// <param name="item"></param>
        public async void RunUpdate(AppCastItem item)
        {
            ProgressWindow?.SetDownloadAndInstallButtonEnabled(false); // disable while we ask if we can close up the software
            if (await AskApplicationToSafelyCloseUp())
            {
                var path = await DownloadPathForAppCastItem(item);
                if (File.Exists(path))
                {
                    var result = DSAChecker.VerifyDSASignatureFile(item.DownloadDSASignature, path);
                    if (result == ValidationResult.Valid || result == ValidationResult.Unchecked)
                    {
                        await RunDownloadedInstaller(path);
                    }
                }
            }
            ProgressWindow?.SetDownloadAndInstallButtonEnabled(true);
        }

        public bool IsDownloadingItem(AppCastItem item)
        {
            return _itemBeingDownloaded?.DownloadDSASignature == item.DownloadDSASignature;
        }

        /// <summary>
        /// True if the user has silent updates enabled; false otherwise.
        /// </summary>
        private bool IsDownloadingSilently()
        {
            return UserInteractionMode != UserInteractionMode.NotSilent;
        }

        /// <summary>
        /// Return installer runner command. May throw InvalidDataException
        /// </summary>
        protected virtual string GetInstallerCommand(string downloadFilePath)
        {
            // get the file type
            string installerExt = Path.GetExtension(downloadFilePath);
            if (".exe".Equals(installerExt, StringComparison.CurrentCultureIgnoreCase))
            {
                // build the command line 
                return "\"" + downloadFilePath + "\"";
            }
            if (".msi".Equals(installerExt, StringComparison.CurrentCultureIgnoreCase))
            {
                // buid the command line
                return "msiexec /i \"" + downloadFilePath + "\"";
            }
            if (".msp".Equals(installerExt, StringComparison.CurrentCultureIgnoreCase))
            {
                // build the command line
                return "msiexec /p \"" + downloadFilePath + "\"";
            }

            throw new InvalidDataException("Unknown installer format");
        }

        /// <summary>
        /// Runs the downloaded installer
        /// </summary>
        protected virtual async Task RunDownloadedInstaller(string downloadFilePath)
        {
            LogWriter.PrintMessage("Running downloaded installer");
            // get the commandline 
            string cmdLine = Environment.CommandLine;
            string workingDir = Environment.CurrentDirectory;

            // generate the batch file path
            string batchFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cmd");
            string installerCmd;
            try
            {
                installerCmd = GetInstallerCommand(downloadFilePath);

                if (!string.IsNullOrEmpty(CustomInstallerArguments))
                {
                    installerCmd += " " + CustomInstallerArguments;
                }
            }
            catch (InvalidDataException)
            {
                UIFactory?.ShowUnknownInstallerFormatMessage(downloadFilePath);
                return;
            }

            // generate the batch file                
            LogWriter.PrintMessage("Generating batch in {0}", Path.GetFullPath(batchFilePath));

            using (StreamWriter write = new StreamWriter(batchFilePath))
            {
                write.WriteLine("@echo off");
                // We should wait until the host process has died before starting the installer.
                // This way, any DLLs or other items can be replaced properly.
                // Code from: http://stackoverflow.com/a/22559462/3938401
                string processID = Process.GetCurrentProcess().Id.ToString();
                string relaunchAfterUpdate = "";
                if (RelaunchAfterUpdate)
                {
                    relaunchAfterUpdate = $@"
                        cd {workingDir}
                        {cmdLine}";
                }

                string output = $@"
                    set /A counter=0                       
                    setlocal ENABLEDELAYEDEXPANSION
                    :loop
                    set /A counter=!counter!+1
                    if !counter! == 90 (
                        goto :afterinstall
                    )
                    tasklist | findstr ""\<{processID}\>"" > nul
                    if not errorlevel 1 (
                        timeout /t 1 > nul
                        goto :loop
                    )
                    :install
                    {installerCmd}
                    {relaunchAfterUpdate}
                    :afterinstall
                    endlocal";
                write.Write(output);
                write.Close();
            }

            // report
            LogWriter.PrintMessage("Going to execute batch: {0}", batchFilePath);

            // init the installer helper
            _installerProcess = new Process
            {
                StartInfo =
                        {
                            FileName = batchFilePath,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
            };
            // start the installer process. the batch file will wait for the host app to close before starting.
            _installerProcess.Start();
            await QuitApplication();
        }

        /// <summary>
        /// Quits the application (host application) 
        /// </summary>
        /// <returns>Runs asynchrously, so returns a Task</returns>
        public async Task QuitApplication()
        {
            // quit the app
            _exitHandle?.Set(); // make SURE the loop exits!
                                // In case the user has shut the window that started this Sparkle window/instance, don't crash and burn.
                                // If you have better ideas on how to figure out if they've shut all other windows, let me know...
            try
            {
                if (CloseApplicationAsync != null)
                {
                    await CloseApplicationAsync.Invoke();
                }
                else if (CloseApplication != null)
                {
                    CloseApplication.Invoke();
                }
                else
                {
                    // Because the download/install window is usually on a separate thread,
                    // send dual shutdown messages via both the sync context (kills "main" app)
                    // and the current thread (kills current thread)
                    _syncContext?.Post((state) =>
                        UIFactory?.Shutdown(),
                    null);
                    UIFactory?.Shutdown();
                }
            }
            catch (Exception e)
            {
                LogWriter.PrintMessage(e.Message);
            }
        }

        /// <summary>
        /// Apps may need, for example, to let user save their work
        /// </summary>
        /// <returns>true if it's OK to run the installer</returns>
        private async Task<bool> AskApplicationToSafelyCloseUp()
        {
            try
            {
                // In case the user has shut the window that started this Sparkle window/instance, don't crash and burn.
                // If you have better ideas on how to figure out if they've shut all other windows, let me know...
                if (AboutToExitForInstallerRunAsync != null)
                {
                    var args = new CancelEventArgs();
                    await AboutToExitForInstallerRunAsync(this, args);
                    return !args.Cancel;
                }
                else if (AboutToExitForInstallerRun != null)
                {
                    var args = new CancelEventArgs();
                    AboutToExitForInstallerRun(this, args);
                    return !args.Cancel;
                }
            }
            catch (Exception e)
            {
                LogWriter.PrintMessage(e.Message);
            }
            return true;
        }

        /// <summary>
        /// Determine if the remote X509 certificate is valid
        /// </summary>
        /// <param name="sender">the web request</param>
        /// <param name="certificate">the certificate</param>
        /// <param name="chain">the chain</param>
        /// <param name="sslPolicyErrors">how to handle policy errors</param>
        /// <returns><c>true</c> if the cert is valid</returns>
        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (TrustEverySSLConnection)
            {
                // verify if we talk about our app cast dll 
                if (sender is HttpWebRequest req && req.RequestUri.Equals(new Uri(_appCastUrl)))
                {
                    return true;
                }
            }

            // check our cert                 
            return sslPolicyErrors == SslPolicyErrors.None && certificate is X509Certificate2 cert2 && cert2.Verify();
        }

        /// <summary>
        /// Check for updates, using interaction appropriate for if the user just said "check for updates".
        /// </summary>
        public async Task<UpdateInfo> CheckForUpdatesAtUserRequest()
        {
            CheckingForUpdatesWindow = UIFactory?.ShowCheckingForUpdates();
            if (CheckingForUpdatesWindow != null)
            {
                CheckingForUpdatesWindow.UpdatesUIClosing += CheckingForUpdatesWindow_Closing; // to detect canceling
                CheckingForUpdatesWindow.Show();
            }
            // TODO: in the future, instead of pseudo-canceling the request and only making it appear as though it was canceled, 
            // actually cancel the request using a BackgroundWorker or something
            UpdateInfo updateData = await CheckForUpdates(false /* toast not appropriate, since they just requested it */);
            if (CheckingForUpdatesWindow != null) // if null, user closed 'Checking for Updates...' window or the UIFactory was null
            {
                CheckingForUpdatesWindow?.Close();
                UpdateStatus updateAvailable = updateData.Status;

                Action<object> UIAction = (state) =>
                {
                    switch (updateAvailable)
                    {
                        case UpdateStatus.UpdateAvailable:
                            if (_useNotificationToast)
                                UIFactory?.ShowToast(updateData.Updates, OnToastClick);
                            break;
                        case UpdateStatus.UpdateNotAvailable:
                            UIFactory?.ShowVersionIsUpToDate();
                            break;
                        case UpdateStatus.UserSkipped:
                            UIFactory?.ShowVersionIsSkippedByUserRequest(); // TODO: pass skipped version number
                            break;
                        case UpdateStatus.CouldNotDetermine:
                            UIFactory?.ShowCannotDownloadAppcast(_appCastUrl);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                };
                CallFuncConsideringUIThreads(() => UIAction(null));
            }

            return updateData;// in this case, we've already shown UI talking about the new version
        }

        private void CheckingForUpdatesWindow_Closing(object sender, EventArgs e)
        {
            CheckingForUpdatesWindow = null;
        }

        /// <summary>
        /// Check for updates, using interaction appropriate for where the user doesn't know you're doing it, so be polite.
        /// </summary>
        public async Task<UpdateInfo> CheckForUpdatesQuietly()
        {
            return await CheckForUpdates(true);
        }

        /// <summary>
        /// Does a one-off check for updates
        /// </summary>
        /// <param name="useNotificationToast">set false if you want the big dialog to open up, without the user having the chance to ignore the popup toast notification</param>
        private async Task<UpdateInfo> CheckForUpdates(bool useNotificationToast)
        {
            // artificial delay -- if internet is super fast and the update check is super fast, the flash (fast show/hide) of the
            // 'Checking for Updates...' window is very disorienting
            // TODO: how could we improve this?
            bool isUserManuallyCheckingForUpdates = CheckingForUpdatesWindow != null;
            if (isUserManuallyCheckingForUpdates)
            {
                await Task.Delay(250);
            }
            UpdateCheckStarted?.Invoke(this);
            Configuration config = Configuration;

            // check if update is required
            _latestDownloadedUpdateInfo = await GetUpdateStatus(config);
            List<AppCastItem> updates = _latestDownloadedUpdateInfo.Updates;
            if (_latestDownloadedUpdateInfo.Status == UpdateStatus.UpdateAvailable)
            {
                // show the update window
                LogWriter.PrintMessage("Update needed from version {0} to version {1}", config.InstalledVersion, updates[0].Version);

                UpdateDetectedEventArgs ev = new UpdateDetectedEventArgs
                {
                    NextAction = NextUpdateAction.ShowStandardUserInterface,
                    ApplicationConfig = config,
                    LatestVersion = updates[0],
                    AppCastItems = updates
                };

                // if the client wants to intercept, send an event
                if (UpdateDetected != null)
                {
                    UpdateDetected(this, ev);
                    // if the client wants the default UI then show them
                    switch (ev.NextAction)
                    {
                        case NextUpdateAction.ShowStandardUserInterface:
                            LogWriter.PrintMessage("Showing Standard Update UI");
                            OnWorkerProgressChanged(_taskWorker, new ProgressChangedEventArgs(1, updates));
                            break;
                    }
                }
                else
                {
                    // otherwise just go forward with the UI notification
                    if (isUserManuallyCheckingForUpdates && CheckingForUpdatesWindow != null)
                    {
                        ShowUpdateNeededUI(updates);
                    }
                }
            }
            UpdateCheckFinished?.Invoke(this, _latestDownloadedUpdateInfo.Status);
            return _latestDownloadedUpdateInfo;
        }

        /// <summary>
        /// Updates from appcast
        /// </summary>
        /// <param name="updates">updates to be installed</param>
        private async void Update(List<AppCastItem> updates)
        {
            if (updates != null)
            {
                if (IsDownloadingSilently())
                {
                    await InitDownloadAndInstallProcess(updates[0]); // install only latest
                }
                else
                {
                    // show the update ui
                    ShowUpdateNeededUI(updates);
                }
            }
        }

        /// <summary>
        /// Cancels an in-progress download and deletes the temporary file.
        /// </summary>
        public void CancelFileDownload()
        {
            LogWriter.PrintMessage("Canceling download...");
            if (UpdateDownloader != null && UpdateDownloader.IsDownloading)
            {
                UpdateDownloader.CancelDownload();
            }
        }

        /// <summary>
        /// Events should always be fired on the thread that started the Sparkle object.
        /// Used for events that are fired after coming from an update available window
        /// or the download progress window.
        /// Basically, if ShowsUIOnMainThread, just invokes the action. Otherwise,
        /// uses the SynchronizationContext to call the action. Ensures that the action
        /// is always on the main thread.
        /// </summary>
        /// <param name="action"></param>
        private void CallFuncConsideringUIThreads(Action action)
        {
            if (ShowsUIOnMainThread)
            {
                action?.Invoke();
            }
            else
            {
                _syncContext.Post((state) => action?.Invoke(), null);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="args">Info on the user response and what update item they responded to</param>
        private async void OnUserWindowUserResponded(object sender, UpdateResponseArgs args)
        {
            LogWriter.PrintMessage("Update window response: {0}", args.Result);
            var currentItem = args.UpdateItem;
            var result = args.Result;
            if (result == UpdateAvailableResult.SkipUpdate)
            {
                // skip this version
                Configuration config = Configuration;
                config.SetVersionToSkip(currentItem.Version);
                CallFuncConsideringUIThreads(() => { UserSkippedVersion?.Invoke(currentItem, _downloadTempFileName); });
            }
            else if (result == UpdateAvailableResult.InstallUpdate)
            {
                if (UserInteractionMode == UserInteractionMode.DownloadNoInstall && File.Exists(_downloadTempFileName))
                {
                    // Binary should already be downloaded. Run it!
                    ProgressWindowCompleted(this, new DownloadInstallArgs(true));
                }
                else
                {
                    // download the binaries
                    await InitDownloadAndInstallProcess(currentItem);
                }
            }
            else if (result == UpdateAvailableResult.RemindMeLater && currentItem != null)
            {
                CallFuncConsideringUIThreads(() => { RemindMeLaterSelected?.Invoke(currentItem); });
            }
            UpdateAvailableWindow?.Close();
            UpdateAvailableWindow = null; // done using the window so don't hold onto reference
            CheckingForUpdatesWindow?.Close();
            CheckingForUpdatesWindow = null;
        }

        /// <summary>
        /// This method will be executed as worker thread
        /// </summary>
        private async void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            // store the did run once feature
            bool goIntoLoop = true;
            bool checkTSP = true;
            bool doInitialCheck = _doInitialCheck;
            bool isInitialCheck = true;

            // start our lifecycles
            do
            {
                if (_cancelToken.IsCancellationRequested)
                {
                    break;
                }
                // set state
                bool bUpdateRequired = false;

                // notify
                CheckLoopStarted?.Invoke(this);

                // report status
                if (doInitialCheck)
                {
                    // report status
                    LogWriter.PrintMessage("Starting update loop...");

                    // read the config
                    LogWriter.PrintMessage("Reading config...");
                    Configuration config = Configuration;

                    // calc CheckTasp
                    bool checkTSPInternal = checkTSP;

                    if (isInitialCheck && checkTSPInternal)
                    {
                        checkTSPInternal = !_forceInitialCheck;
                    }

                    // check if it's ok the recheck to software state
                    TimeSpan csp = DateTime.Now - config.LastCheckTime;

                    if (!checkTSPInternal || csp >= _checkFrequency)
                    {
                        checkTSP = true;
                        // when sparkle will be deactivated wait another cycle
                        if (config.CheckForUpdate == true)
                        {
                            // update the runonce feature
                            goIntoLoop = !config.DidRunOnce;

                            // check if update is required
                            if (_cancelToken.IsCancellationRequested)
                            {
                                break;
                            }
                            _latestDownloadedUpdateInfo = await GetUpdateStatus(config);
                            if (_cancelToken.IsCancellationRequested)
                            {
                                break;
                            }
                            bUpdateRequired = _latestDownloadedUpdateInfo.Status == UpdateStatus.UpdateAvailable;
                            if (bUpdateRequired)
                            {
                                List<AppCastItem> updates = _latestDownloadedUpdateInfo.Updates;
                                // show the update window
                                LogWriter.PrintMessage("Update needed from version {0} to version {1}", config.InstalledVersion, updates[0].Version);

                                // send notification if needed
                                UpdateDetectedEventArgs ev = new UpdateDetectedEventArgs
                                {
                                    NextAction = NextUpdateAction.ShowStandardUserInterface,
                                    ApplicationConfig = config,
                                    LatestVersion = updates[0],
                                    AppCastItems = updates
                                };
                                UpdateDetected?.Invoke(this, ev);

                                // check results
                                switch (ev.NextAction)
                                {
                                    case NextUpdateAction.PerformUpdateUnattended:
                                        {
                                            LogWriter.PrintMessage("Unattended update desired from consumer");
                                            UserInteractionMode = UserInteractionMode.DownloadAndInstall;
                                            OnWorkerProgressChanged(_taskWorker, new ProgressChangedEventArgs(1, updates));
                                            break;
                                        }
                                    case NextUpdateAction.ProhibitUpdate:
                                        {
                                            LogWriter.PrintMessage("Update prohibited from consumer");
                                            break;
                                        }
                                    default:
                                        {
                                            LogWriter.PrintMessage("Showing Standard Update UI");
                                            OnWorkerProgressChanged(_taskWorker, new ProgressChangedEventArgs(1, updates));
                                            break;
                                        }
                                }
                            }
                        }
                        else
                        {
                            LogWriter.PrintMessage("Check for updates disabled");
                        }
                    }
                    else
                    {
                        LogWriter.PrintMessage("Update check performed within the last {0} minutes!", _checkFrequency.TotalMinutes);
                    }
                }
                else
                {
                    LogWriter.PrintMessage("Initial check prohibited, going to wait");
                    doInitialCheck = true;
                }

                // checking is done; this is now the "let's wait a while" section

                // reset initial check
                isInitialCheck = false;

                // notify
                CheckLoopFinished?.Invoke(this, bUpdateRequired);

                // report wait statement
                LogWriter.PrintMessage("Sleeping for an other {0} minutes, exit event or force update check event", _checkFrequency.TotalMinutes);

                // wait for
                if (!goIntoLoop || _cancelToken.IsCancellationRequested)
                {
                    break;
                }

                // build the event array
                WaitHandle[] handles = new WaitHandle[1];
                handles[0] = _exitHandle;

                // wait for any
                if (_cancelToken.IsCancellationRequested)
                {
                    break;
                }
                int i = WaitHandle.WaitAny(handles, _checkFrequency);
                if (_cancelToken.IsCancellationRequested)
                {
                    break;
                }
                if (WaitHandle.WaitTimeout == i)
                {
                    LogWriter.PrintMessage("{0} minutes are over", _checkFrequency.TotalMinutes);
                    continue;
                }

                // check the exit handle
                if (i == 0)
                {
                    LogWriter.PrintMessage("Got exit signal");
                    break;
                }

                // check an other check needed
                if (i == 1)
                {
                    LogWriter.PrintMessage("Got force update check signal");
                    checkTSP = false;
                }
                if (_cancelToken.IsCancellationRequested)
                {
                    break;
                }
            } while (goIntoLoop);

            // reset the islooping handle
            _loopingHandle.Reset();
        }

        /// <summary>
        /// This method will be notified
        /// </summary>
        private void OnWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 1:
                    Update(e.UserState as List<AppCastItem>);
                    break;
                case 0:
                    LogWriter.PrintMessage(e.UserState.ToString());
                    break;
            }
        }

        /// <summary>
        /// Called when the installer is downloaded
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">used to determine if the download was successful.</param>
        private void OnDownloadFinished(object sender, AsyncCompletedEventArgs e)
        {
            bool shouldShowUIItems = !IsDownloadingSilently();

            if (e.Cancelled)
            {
                DownloadCanceled?.Invoke(_downloadTempFileName);
                _hasAttemptedFileRedownload = false;
                if (File.Exists(_downloadTempFileName))
                {
                    File.Delete(_downloadTempFileName);
                }
                LogWriter.PrintMessage("Download was canceled");
                string errorMessage = "Download canceled";
                if (shouldShowUIItems && ProgressWindow != null && !ProgressWindow.DisplayErrorMessage(errorMessage))
                {
                    UIFactory?.ShowDownloadErrorMessage(errorMessage, _appCastUrl);
                }
                return;
            }
            if (e.Error != null)
            {
                DownloadError?.Invoke(_downloadTempFileName);
                // Clean temp files on error too
                if (File.Exists(_downloadTempFileName))
                {
                    File.Delete(_downloadTempFileName);
                }
                LogWriter.PrintMessage("Error on download finished: {0}", e.Error.Message);
                if (shouldShowUIItems && ProgressWindow != null && !ProgressWindow.DisplayErrorMessage(e.Error.Message))
                {
                    UIFactory?.ShowDownloadErrorMessage(e.Error.Message, _appCastUrl);
                }
                return;
            }
            // test the item for DSA signature
            var validationRes = ValidationResult.Invalid;
            if (!e.Cancelled && e.Error == null)
            {
                LogWriter.PrintMessage("Fully downloaded file exists at {0}", _downloadTempFileName);

                LogWriter.PrintMessage("Performing DSA check");

                // get the assembly
                if (File.Exists(_downloadTempFileName))
                {
                    // check if the file was downloaded successfully
                    string absolutePath = Path.GetFullPath(_downloadTempFileName);
                    if (!File.Exists(absolutePath))
                    {
                        throw new FileNotFoundException();
                    }

                    // check the DSA signature
                    validationRes = DSAChecker.VerifyDSASignatureFile(_itemBeingDownloaded?.DownloadDSASignature, _downloadTempFileName);
                }
            }

            bool isSignatureInvalid = validationRes == ValidationResult.Invalid; // if Unchecked, we accept download as valid
            if (shouldShowUIItems)
            {
                ProgressWindow?.FinishedDownloadingFile(!isSignatureInvalid);
            }
            // signature of file isn't valid so exit with error
            if (isSignatureInvalid)
            {
                LogWriter.PrintMessage("Invalid signature for downloaded file for app cast: {0}", _downloadTempFileName);
                string errorMessage = "Downloaded file has invalid signature!";
                DownloadedFileIsCorrupt?.Invoke(_itemBeingDownloaded, _downloadTempFileName);
                // Default to showing errors in the progress window. Only go to the UIFactory to show errors if necessary.
                if (shouldShowUIItems && ProgressWindow != null && !ProgressWindow.DisplayErrorMessage(errorMessage))
                {
                    UIFactory?.ShowDownloadErrorMessage(errorMessage, _appCastUrl);
                }
                // Let the progress window handle closing itself.
            }
            else
            {
                FinishedDownloading?.Invoke(_downloadTempFileName);
                LogWriter.PrintMessage("DSA Signature is valid. File successfully downloaded!");
                DownloadedFileReady?.Invoke(_itemBeingDownloaded, _downloadTempFileName);
                bool shouldInstallAndRelaunch = UserInteractionMode == UserInteractionMode.DownloadAndInstall;
                if (shouldInstallAndRelaunch)
                {
                    ProgressWindowCompleted(this, new DownloadInstallArgs(true));
                }
            }
            _itemBeingDownloaded = null;
        }
    }
}
