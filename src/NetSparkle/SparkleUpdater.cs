using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using NetSparkleUpdater.Interfaces;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using System.Collections.Generic;
using NetSparkleUpdater.Downloaders;
using NetSparkleUpdater.Configurations;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.AssemblyAccessors;
using System.Text;
#if NETSTANDARD || NET6 || NET7 || NET8
using System.Runtime.InteropServices;
#endif

// TODO: Refactor download data to its own class so we can have 
// easier nullability checks/use and cleaner logic, etc.
// Basically this file needs a refactor for cleaner logic and easier
// use so that things flow more nicely, play nice with any UI, etc.

namespace NetSparkleUpdater
{
    /// <summary>
    /// Class to communicate with a sparkle-based appcast to download
    /// and install updates to an application
    /// </summary>
    public partial class SparkleUpdater : IDisposable
    {
        #region Protected/Private Members

        /// <summary>
        /// The <see cref="Process"/> responsible for launching the downloaded update.
        /// Only valid once the application is about to quit and the update is going to
        /// be launched.
        /// Use `InstallerProcess` if you are a software app that updates other apps 
        /// so you can monitor the installation process.
        /// </summary>
        protected Process? _installerProcess;
        // _shouldKillParentProcessWhenStartingInstaller defaults to true
        private bool _shouldKillParentProcessWhenStartingInstaller;
        private string? _processIDToKillBeforeInstallerRuns;

        private ILogger? _logWriter;
        private readonly Task _taskWorker;
        private CancellationToken _cancelToken;
        private readonly CancellationTokenSource _cancelTokenSource;
        private readonly string? _appReferenceAssembly;

        private bool _doInitialCheck;
        private bool _forceInitialCheck;

        private readonly EventWaitHandle _exitHandle;
        private readonly EventWaitHandle _loopingHandle;
        private TimeSpan _checkFrequency;
        private string? _tmpDownloadFilePath;
        private string? _downloadTempFileName;
        private AppCastItem? _itemBeingDownloaded;
        private bool _hasAttemptedFileRedownload;
        private UpdateInfo? _latestDownloadedUpdateInfo;
        private IUIFactory? _uiFactory;
        private bool _disposed;
        private Configuration? _configuration;
        private string? _restartExecutableName;
        private string? _restartExecutablePath;

        private AppCastHelper? _appCastHelper;
        private IAppCastGenerator? _appCastGenerator;
        private IUpdateDownloader? _updateDownloader;

        /// <summary>
        /// The progress window is shown on a separate thread.
        /// In order to ensure that the download starts after the progress window is shown,
        /// we create an Action to run after things are ready.
        /// It would be better if things ran using async/await, but this will
        /// suffice as a "fix" for now.
        /// </summary>
        private Action? _actionToRunOnProgressWindowShown;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor which requires the app cast url and the object that will verify app cast signatures
        /// </summary>
        /// <param name="appcastUrl">the URL of the app cast file</param>
        /// <param name="signatureVerifier">the object that will verify your app cast signatures.</param>
        public SparkleUpdater(string appcastUrl, ISignatureVerifier signatureVerifier)
            : this(appcastUrl, signatureVerifier, null)
        { }

        /// <summary>
        /// ctor which needs the app cast url, an object to verify app cast signatures, and a reference assembly
        /// </summary>
        /// <param name="appcastUrl">the URL of the app cast file</param>
        /// <param name="signatureVerifier">the object that will verify your app cast signatures.</param>
        /// <param name="referenceAssembly">the name of the assembly to use for comparison when checking update versions</param>
        public SparkleUpdater(string appcastUrl, ISignatureVerifier signatureVerifier, string? referenceAssembly)
            : this(appcastUrl, signatureVerifier, referenceAssembly, null)
        { }

        /// <summary>
        /// Constructor that performs all necessary initialization for software update checking
        /// </summary>
        /// <param name="appcastUrl">the URL of the app cast file</param>
        /// <param name="signatureVerifier">the object that will verify your app cast signatures.</param>
        /// <param name="referenceAssembly">the name of the assembly to use for comparison when checking update versions</param>
        /// <param name="factory">a UI factory to use in place of the default UI</param>
        public SparkleUpdater(string appcastUrl, ISignatureVerifier signatureVerifier, string? referenceAssembly, IUIFactory? factory)
        {
            _latestDownloadedUpdateInfo = null;
            _hasAttemptedFileRedownload = false;

            UIFactory = factory;
            SignatureVerifier = signatureVerifier;
            LogWriter = new LogWriter();
            // init UI
            UIFactory?.Init(this);
            _appReferenceAssembly = null;
            // set the reference assembly
            if (referenceAssembly != null)
            {
                _appReferenceAssembly = referenceAssembly;
                LogWriter?.PrintMessage("Checking the following file for assembly information: " + _appReferenceAssembly);
            }

            // adjust the delegates
            _taskWorker = new Task(OnWorkerDoWork);
            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;

            // build the wait handle
            _exitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            _loopingHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            // set the url
            AppCastUrl = appcastUrl;
            LogWriter?.PrintMessage("Using the following url for downloading the app cast: {0}", AppCastUrl);
            UserInteractionMode = UserInteractionMode.NotSilent;
            ShouldKillParentProcessWhenStartingInstaller = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The security protocol used by NetSparkle. Setting this property will also set this 
        /// for the current AppDomain of the caller. Needs to be set to 
        /// SecurityProtocolType.Tls12 for some cases (such as when downloading from GitHub).
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

        /// <summary>
        /// Set the user interaction mode for Sparkle to use when there is a valid update for the software
        /// </summary>
        public UserInteractionMode UserInteractionMode { get; set; }

        /// <summary>
        /// If set, downloads files to this path. If the folder doesn't already exist, creates
        /// the folder at download time (and not before). 
        /// Note that this variable is a path, not a full file name.
        /// </summary>
        public string? TmpDownloadFilePath
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
        public string? CustomInstallerArguments { get; set; }

        /// <summary>
        /// Function that is called asynchronously to clean up old installers that have been
        /// downloaded with SilentModeTypes.DownloadNoInstall or SilentModeTypes.DownloadAndInstall.
        /// </summary>
        public Action? ClearOldInstallers { get; set; }

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
        /// Factory for creating UI elements like progress window, etc.
        /// </summary>
        public IUIFactory? UIFactory
        {
            get { return _uiFactory; }
            set { _uiFactory = value; _uiFactory?.Init(this); }
        }

        /// <summary>
        /// The user interface that shows the release notes and
        /// asks the user to skip, remind me later, or update
        /// </summary>
        private IUpdateAvailable? UpdateAvailableWindow { get; set; }

        /// <summary>
        /// The user interface that shows a download progress bar,
        /// and then asks to install and relaunch the application
        /// </summary>
        private IDownloadProgress? ProgressWindow { get; set; }

        /// <summary>
        /// The user interface that shows the 'Checking for Updates...'
        /// UIrm.
        /// </summary>
        private ICheckingForUpdates? CheckingForUpdatesWindow { get; set; }

        /// <summary>
        /// The configuration object for a given assembly that has information on when
        /// updates were checked last, any updates that have been skipped, etc.
        /// </summary>
        public Configuration Configuration
        {
            get
            {
                if (_configuration == null)
                {
#if NETSTANDARD || NET6 || NET7 || NET8
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            _configuration = new RegistryConfiguration(new AsmResolverAccessor(_appReferenceAssembly));
                        }
                        else
                        {
                            try {
                                _configuration = new JSONConfiguration(new AsmResolverAccessor(_appReferenceAssembly));
                            } catch (Exception e) {
                                LogWriter?.PrintMessage("Unable to create JSONConfiguration object: {0}", e.Message);
                                _configuration = new DefaultConfiguration(new AsmResolverAccessor(_appReferenceAssembly));
                            }
                        }
#else
                        _configuration = new RegistryConfiguration(new AsmResolverAccessor(_appReferenceAssembly));
#endif
                }
                return _configuration;
            }
            set { _configuration = value; }
        }

        /// <summary>
        /// Path to the working directory for the current application.
        /// This is the directory that the current executable sits in --
        /// e.g. C:/Users/...Foo/. It will be used when restarting the
        /// application on Windows or will be used on macOS/Linux for
        /// overwriting files on an update.
        /// </summary>
        public string RestartExecutablePath
        {
            get
            {
                if (_restartExecutablePath != null && !string.IsNullOrWhiteSpace(_restartExecutablePath))
                {
                    return _restartExecutablePath;
                }
                return Utilities.GetFullBaseDirectory();
            }
            set
            {
                _restartExecutablePath = value;
            }
        }

        /// <summary>
        /// Executable name to use when restarting the software.
        /// This is the name that will be used/started when the update has been installed.
        /// This defaults to <see cref="Environment.CommandLine"/>.
        /// Used in conjunction with RestartExecutablePath to restart the application --
        /// cd "{RestartExecutablePath}"
        /// {RelaunchAfterUpdateCommandPrefix}"{RestartExecutableName}" is what is called to restart the app,
        /// so make sure you add a space after RelaunchAfterUpdateCommandPrefix if needed.
        /// </summary>
        public string RestartExecutableName
        {
            get
            {
                if (_restartExecutableName != null && !string.IsNullOrWhiteSpace(_restartExecutableName))
                {
                    return _restartExecutableName;
                }
#if NETCORE
                try
                {
                    var mainModule = Process.GetCurrentProcess().MainModule;
                    if (mainModule != null)
                    {
                        var path = Path.GetFileName(mainModule.FileName);
                        if (path != null && !string.IsNullOrWhiteSpace(path))
                        {
                            return path;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogWriter?.PrintMessage("Unable to get executable name: " + e.Message);
                }
#endif
                // we cannot just use Path.GetFileName because on .NET Framework it can fail with
                // invalid chars in the path, so we do some crazy things to get the file name another way
                var cmdLine = Environment.CommandLine.Trim().TrimStart('"').TrimEnd('"');
                return cmdLine.Substring(cmdLine.LastIndexOf(Path.DirectorySeparatorChar) + 1).Trim();
            }
            set
            {
                _restartExecutableName = value;
            }
        }

        /// <summary>
        /// Defines a command prefix that will be used to open the executable (like "dotnet " or "./" on Linux systems).
        /// This prefix command will not be auto-escaped with quotes for you, so if that's needed, make sure to do it yourself.
        /// Contribution from @daniel-pastalab
        /// </summary>
        public string? RelaunchAfterUpdateCommandPrefix { get; set; }

        /// <summary>
        /// The object that verifies signatures (DSA, Ed25519, or otherwise) of downloaded items
        /// </summary>
        public ISignatureVerifier SignatureVerifier { get; set; }

        /// <summary>
        /// Gets or sets the app cast URL
        /// </summary>
        public string AppCastUrl { get; set; }

        /// <summary>
        /// Specifies if you want to use the notification toast message (not implemented in all UIs).
        /// </summary>
        public bool UseNotificationToast { get; set; }

        /// <summary>
        /// Object that handles any diagnostic messages for NetSparkle.
        /// If you want to use your own class for this, you should just
        /// need to override <see cref="LogWriter.PrintMessage"/> in your own class.
        /// Make sure to set this object before calling <see cref="StartLoop(bool)"/> to guarantee
        /// that all messages will get sent to the right place!
        /// </summary>
        public ILogger? LogWriter
        {
            get
            {
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
        public bool CheckServerFileName { get; set; } = true;

        /// <summary>
        /// Returns the latest appcast items to the caller. Might be null.
        /// </summary>
        public List<AppCastItem>? LatestAppCastItems
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

        /// <summary>
        /// The object responsable for downloading update files for your application
        /// </summary>
        public IUpdateDownloader UpdateDownloader 
        { 
            get
            {
                if (_updateDownloader == null)
                {
                    _updateDownloader = new WebFileDownloader(LogWriter);
                }
                return _updateDownloader;
            }
            set => _updateDownloader = value;
        }

        /// <summary>
        /// The object responsible for downloading app cast and app cast signature
        /// information for your application
        /// </summary>
        public IAppCastDataDownloader? AppCastDataDownloader { get; set; }

        /// <summary>
        /// The object responsible for parsing app cast information and checking to
        /// see if any updates are available in a given app cast
        /// </summary>
        public AppCastHelper AppCastHelper
        {
            get
            {
                if (_appCastHelper == null)
                {
                    _appCastHelper = new AppCastHelper();
                }
                return _appCastHelper;
            }
            set => _appCastHelper = value;
        }

        /// <summary>
        /// Object responsible for serializing and deserializing 
        /// <seealso cref="AppCast"/> objects after data has been downloaded.
        /// </summary>
        public IAppCastGenerator AppCastGenerator
        {
            get
            {
                if (_appCastGenerator == null)
                {
                    _appCastGenerator = new XMLAppCastGenerator(LogWriter);
                }
                return _appCastGenerator;
            }
            set => _appCastGenerator = value;
        }

        /// <summary>
        /// When running the downloaded installer/update file, before installing it,
        /// whether or not to kill the parent process that initiated the software update
        /// process (generally the thing that is controlling the SparkleUpdater instance).
        /// Defaults to true. Set to false if for some reason you don't want your app to
        /// restart or die when the installer starts (e.g. if it is an optional update or an
        /// update for something outside of the software itself).
        /// </summary>
        public bool ShouldKillParentProcessWhenStartingInstaller
        {
            get => _shouldKillParentProcessWhenStartingInstaller;
            set => _shouldKillParentProcessWhenStartingInstaller = value;
        }

        /// <summary>
        /// The process ID that should be killed before the installer runs. This is nullable.
        /// On starting the installer/updater file, if this property is null/whitespace, the process that
        /// will be killed is Process.GetCurrentProcess().Id.ToString(), unless
        /// ShouldKillParentProcessWhenStartingInstaller is set to false.
        /// Does not matter what you set on this property if ShouldKillParentProcessWhenStartingInstaller
        /// is false.
        /// </summary>
        public string ProcessIDToKillBeforeInstallerRuns
        {
            get
            {
                if (_processIDToKillBeforeInstallerRuns == null || string.IsNullOrWhiteSpace(_processIDToKillBeforeInstallerRuns))
                {
#if NET5_0_OR_GREATER
                    return Environment.ProcessId.ToString();
#else
                    using (var process = Process.GetCurrentProcess())
                    {
                        var id = process.Id.ToString();
                        process.Dispose();
                        return id;
                    }
#endif
                }
                return _processIDToKillBeforeInstallerRuns;
            }
            set => _processIDToKillBeforeInstallerRuns = value;
        }

        /// <summary>
        /// The <see cref="Process"/> responsible for launching the downloaded update.
        /// Only valid once the application is about to quit and the update is going to
        /// be launched.
        /// Use this if you are a software app that updates other apps 
        /// so you can monitor the installation process. Otherwise, if you're just running
        /// an installer for the software that's running, this will not be very helpful to use.
        /// </summary>
        public Process? InstallerProcess
        {
            get => _installerProcess;
        }

        /// <summary>
        /// A cache / copy of the most recently downloaded <seealso cref="AppCast"/>.
        /// Set after parsing an app cast that was downloaded from online. 
        /// This property only for convenience's sake.
        /// Use if you aren't storing the app cast downloaded data yourself somewhere.
        /// Will be wiped/reset/changed after downloading a new app cast, so be careful using this
        /// if you aren't saving the data yourself somewhere.
        /// </summary>
        public AppCast? AppCastCache { get; set; }

        #endregion

        /// <summary>
        /// Starts a SparkleUpdater background loop to check for updates every 24 hours.
        /// <para>You should only call this function when your app is initialized and shows its main UI.</para>
        /// </summary>
        /// <param name="doInitialCheck">whether the first check should happen before or after the first interval</param>
        public async Task StartLoop(bool doInitialCheck)
        {
            await StartLoop(doInitialCheck, false);
        }

        /// <summary>
        /// Starts a SparkleUpdater background loop to check for updates on a given interval.
        /// <para>You should only call this function when your app is initialized and shows its main UI.</para>
        /// </summary>
        /// <param name="doInitialCheck">whether the first check should happen before or after the first interval</param>
        /// <param name="checkFrequency">the interval to wait between update checks</param>
        public async Task StartLoop(bool doInitialCheck, TimeSpan checkFrequency)
        {
            await StartLoop(doInitialCheck, false, checkFrequency);
        }

        /// <summary>
        /// Starts a SparkleUpdater background loop to check for updates every 24 hours.
        /// <para>You should only call this function when your app is initialized and shows its main UI.</para>
        /// </summary>
        /// <param name="doInitialCheck">whether the first check should happen before or after the first interval</param>
        /// <param name="forceInitialCheck">if <paramref name="doInitialCheck"/> is true, whether the first check
        /// should happen even if the last check was less than 24 hours ago</param>
        public async Task StartLoop(bool doInitialCheck, bool forceInitialCheck)
        {
            await StartLoop(doInitialCheck, forceInitialCheck, TimeSpan.FromHours(24));
        }

        /// <summary>
        /// Starts a SparkleUpdater background loop to check for updates on a given interval.
        /// <para>You should only call this function when your app is initialized and shows its main UIw.</para>
        /// </summary>
        /// <param name="doInitialCheck">whether the first check should happen before or after the first interval</param>
        /// <param name="forceInitialCheck">if <paramref name="doInitialCheck"/> is true, whether the first check
        /// should happen even if the last check was within the last <paramref name="checkFrequency"/> interval</param>
        /// <param name="checkFrequency">the interval to wait between update checks</param>
        public async Task StartLoop(bool doInitialCheck, bool forceInitialCheck, TimeSpan checkFrequency)
        {
            if (ClearOldInstallers != null)
            {
                try
                {
                    await Task.Run(ClearOldInstallers);
                }
                catch (Exception e)
                {
                    LogWriter?.PrintMessage("ClearOldInstallers threw an exception: {0}", e.Message);
                }
            }
            // first set the event handle
            _loopingHandle.Set();

            // Start the helper thread as a background worker                     

            // store info
            _doInitialCheck = doInitialCheck;
            _forceInitialCheck = forceInitialCheck;
            _checkFrequency = checkFrequency;

            LogWriter?.PrintMessage("Starting background worker");

            // start the work
            //var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            //_taskWorker.Start(scheduler);
            // don't allow starting the task 2x
            if (_taskWorker.IsCompleted == false && _taskWorker.Status != TaskStatus.Running &&
                _taskWorker.Status != TaskStatus.WaitingToRun && _taskWorker.Status != TaskStatus.WaitingForActivation)
            {
                _taskWorker.Start();
            }
        }

        /// <summary>
        /// Stops the SparkleUpdater background loop. Called automatically by <see cref="Dispose()"/>.
        /// </summary>
        public void StopLoop()
        {
            // ensure the work will finished
            _exitHandle.Set();
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~SparkleUpdater()
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
        /// <param name="disposing">true if the object is currently being disposed; false otherwise</param>
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
        /// Unregisters events so that we don't call events more often than we should
        /// </summary>
        private void UnregisterEvents()
        {
            _cancelTokenSource.Cancel();

            CleanUpUpdateDownloader();

            if (UpdateAvailableWindow != null)
            {
                UpdateAvailableWindow.UserResponded -= OnUpdateWindowUserResponded;
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
        /// <param name="config">the SparkleUpdater configuration for the reference assembly</param>
        /// <param name="ignoreSkippedVersions">true to ignore skipped versions, false otherwise</param>
        /// <returns><see cref="UpdateInfo"/> with information on whether there is an update available or not.</returns>
        protected async Task<UpdateInfo> GetUpdateStatus(Configuration config, bool ignoreSkippedVersions = false)
        {
            List<AppCastItem>? updates = null;
            // report
            LogWriter?.PrintMessage("Downloading and checking appcast");

            // init the appcast
            if (AppCastDataDownloader == null)
            {
                AppCastDataDownloader = new WebRequestAppCastDataDownloader(LogWriter);
            }
            AppCastHelper.SetupAppCastHelper(AppCastDataDownloader, AppCastUrl, 
                config.InstalledVersion, SignatureVerifier, LogWriter);
            // check if any updates are available
            try
            {
                LogWriter?.PrintMessage("About to start downloading the app cast...");
                var appCastStr = await AppCastHelper.DownloadAppCast();
                if (appCastStr != null && !string.IsNullOrWhiteSpace(appCastStr))
                {
                    LogWriter?.PrintMessage("App cast successfully downloaded. Parsing...");
                    var appCast = AppCastCache = await AppCastGenerator.DeserializeAppCastAsync(appCastStr);
                    LogWriter?.PrintMessage("App cast parsed; getting available updates...");
                    updates = AppCastHelper.FilterUpdates(appCast.Items);
                }
            }
            catch (Exception e)
            {
                LogWriter?.PrintMessage("Couldn't read/parse the app cast: {0}; {1}", e.Message, e.StackTrace ?? "[No stack trace available]");
                updates = null;
            }

            if (updates == null)
            {
                LogWriter?.PrintMessage("No version information in app cast found");
                return new UpdateInfo(UpdateStatus.CouldNotDetermine);
            }

            // set the last check time
            LogWriter?.PrintMessage("Touch the last check timestamp");
            config.TouchCheckTime();

            // check if the version will be the same then the installed version
            if (updates.Count == 0)
            {
                LogWriter?.PrintMessage("Installed version is latest, no update needed ({0})", config.InstalledVersion);
                return new UpdateInfo(UpdateStatus.UpdateNotAvailable, updates);
            }
            LogWriter?.PrintMessage("Latest version on the server is {0}", updates[0].Version ?? "[Unknown]");

            // check if the available update has to be skipped
            if (!ignoreSkippedVersions && (updates[0].Version?.Equals(config.LastVersionSkipped) ?? false))
            {
                LogWriter?.PrintMessage("Latest update has to be skipped (user decided to skip version {0})", config.LastVersionSkipped);
                return new UpdateInfo(UpdateStatus.UserSkipped, updates);
            }

            return new UpdateInfo(UpdateStatus.UpdateAvailable, updates);
        }

        /// <summary>
        /// Shows the update needed UI with the given set of updates. Shows nothing if the number of updates is 0.
        /// </summary>
        /// <param name="updates">updates to show UI for</param>
        /// <param name="isUpdateAlreadyDownloaded">If true, make sure UI text shows that the user is about to install the file instead of download it.</param>
        public void ShowUpdateNeededUI(List<AppCastItem>? updates, bool isUpdateAlreadyDownloaded = false)
        {
            if (updates != null && updates.Count > 0)
            {
                if (UseNotificationToast && (UIFactory?.CanShowToastMessages(this) ?? false))
                {
                    UIFactory?.ShowToast(this, updates, OnToastClick);
                }
                else
                {
                    ShowUpdateAvailableWindow(updates, isUpdateAlreadyDownloaded);
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
            ShowUpdateAvailableWindow(updates);
        }

        private void ShowUpdateAvailableWindow(List<AppCastItem> updates, bool isUpdateAlreadyDownloaded = false)
        {
            UpdateAvailableWindow?.Close();
            UpdateAvailableWindow = null;
            UpdateAvailableWindow = UIFactory?.CreateUpdateAvailableWindow(this, updates, isUpdateAlreadyDownloaded);

            if (UpdateAvailableWindow != null)
            {
                UpdateAvailableWindow.UserResponded += OnUpdateWindowUserResponded;
                UpdateAvailableWindow.Show();
            }
        }

        /// <summary>
        /// Get the download path for a given app cast item.
        /// If any directories need to be created, this function
        /// will create those directories.
        /// </summary>
        /// <param name="item">The item that you want to generate a download path for</param>
        /// <returns>The download path for an app cast item if item is not null and has valid download link
        /// Otherwise returns null.</returns>
        public async Task<string?> GetDownloadPathForAppCastItem(AppCastItem item)
        {
            if (item.DownloadLink != null)
            {
                string? filename = string.Empty;

                // default to using the server's file name as the download file name
                if (UpdateDownloader is WebFileDownloader webFileDownloader)
                {
                    webFileDownloader.PrepareToDownloadFile(); // reset download operations
                }
                if (CheckServerFileName && UpdateDownloader != null)
                {
                    try
                    {
                        filename = await UpdateDownloader.RetrieveDestinationFileNameAsync(item);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }

                if (string.IsNullOrWhiteSpace(filename))
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

                if (!string.IsNullOrWhiteSpace(filename))
                {
                    string tmpPath = TmpDownloadFilePath == null || string.IsNullOrWhiteSpace(TmpDownloadFilePath) 
                        ? Path.GetTempPath() 
                        : TmpDownloadFilePath;

                    // Creates all directories and subdirectories in the specific path unless they already exist.
                    Directory.CreateDirectory(tmpPath);

                    return Path.Combine(tmpPath, filename);
                }
            }
            return null;
        }

        /// <summary>
        /// Starts the download process by grabbing the download path for
        /// the app cast item (asynchronous so that it can get the server's
        /// download name in case there is a redirect; cancel this by setting
        /// CheckServerFileName to false), then beginning the download
        /// process if the download file doesn't already exist
        /// </summary>
        /// <param name="item">the appcast item to download</param>
        public async Task InitAndBeginDownload(AppCastItem item)
        {
            if (UpdateDownloader?.IsDownloading ?? false)
            {
                return; // file is already downloading, don't do anything!
            }
            LogWriter?.PrintMessage("Preparing to download {0}", item.DownloadLink ?? "[No download link available]");
            _itemBeingDownloaded = item;
            _downloadTempFileName = await GetDownloadPathForAppCastItem(item);
            if (_downloadTempFileName == null)
            {
                LogWriter?.PrintMessage("Unable to generate download temp file name; was the app cast set up properly with a download link?");
                return;
            }
            // Make sure the file doesn't already exist on disk. If it's already downloaded and the
            // signature checks out, don't redownload the file!
            bool needsToDownload = true;
            if (File.Exists(_downloadTempFileName))
            {
                ValidationResult result;
                try
                {
                    result = SignatureVerifier.VerifySignatureOfFile(item.DownloadSignature ?? "", _downloadTempFileName);
                }
                catch (Exception exc)
                {
                    LogWriter?.PrintMessage("Error validating signature of file: {0}; {1}", exc.Message, exc.StackTrace ?? "[No stack trace available]");
                    result = ValidationResult.Invalid;
                }
                if (result == ValidationResult.Valid)
                {
                    LogWriter?.PrintMessage("File is already downloaded");
                    // We already have the file! Don't redownload it!
                    needsToDownload = false;
                    // Still need to set up the ProgressWindow for non-silent downloads, though,
                    // so that the user can actually perform the install
                    _actionToRunOnProgressWindowShown = () =>
                    {
                        DownloadFinished?.Invoke(_itemBeingDownloaded, _downloadTempFileName); 
                        bool shouldInstallAndRelaunch = UserInteractionMode == UserInteractionMode.DownloadAndInstall;
                        if (shouldInstallAndRelaunch)
                        {
                            ProgressWindowCompleted(this, new DownloadInstallEventArgs(true));
                        }
                    };
                    CreateAndShowProgressWindow(item, true);
                }
                else if (!_hasAttemptedFileRedownload)
                {
                    // The file exists but it either has a bad signature or SecurityMode is set to Unsafe.
                    // Redownload it!
                    _hasAttemptedFileRedownload = true;
                    LogWriter?.PrintMessage("File is corrupt or signature is Unchecked; deleting file and redownloading...");
                    try
                    {
                        File.Delete(_downloadTempFileName);
                    }
                    catch (Exception e)
                    {
                        LogWriter?.PrintMessage("Hm, seems as though we couldn't delete the temporary file even though it is apparently corrupt. {0}",
                            e.Message);
                        // we won't be able to download anyway since we couldn't delete the file :( we'll try next time the
                        // update loop goes around.
                        needsToDownload = false;
                        DownloadHadError?.Invoke(item, _downloadTempFileName,
                            new Exception(string.Format("Unable to delete old download at {0}", _downloadTempFileName)));
                    }
                }
                else
                {
                    DownloadedFileIsCorrupt?.Invoke(item, _downloadTempFileName);
                }
            }
            if (item.DownloadLink == null || string.IsNullOrWhiteSpace(item.DownloadLink))
            {
                LogWriter?.PrintMessage("No download link available for item; was your appcast set up properly?");
            }
            else
            {
                if (needsToDownload)
                {
                    // remove any old event handlers so we don't fire 2x
                    if (UpdateDownloader != null)
                    {
                        UpdateDownloader.DownloadProgressChanged -= OnDownloadProgressChanged;
                        UpdateDownloader.DownloadFileCompleted -= OnDownloadFinished;
                        UpdateDownloader.DownloadStarted -= OnDownloadStarted;
                        UpdateDownloader.DownloadProgressChanged += OnDownloadProgressChanged;
                        UpdateDownloader.DownloadFileCompleted += OnDownloadFinished;
                        UpdateDownloader.DownloadStarted += OnDownloadStarted;
                    }
                    _actionToRunOnProgressWindowShown = () =>
                    {
                        Uri url = Utilities.GetAbsoluteURL(item.DownloadLink, AppCastUrl);
                        LogWriter?.PrintMessage("Starting to download {0} to {1}", item.DownloadLink, _downloadTempFileName);
                        UpdateDownloader?.DownloadFile(url, _downloadTempFileName);
                    };
                    CreateAndShowProgressWindow(item, false);
                }
            }
        }

        private void OnDownloadStarted(object sender, string from, string to)
        {
            if (_itemBeingDownloaded != null && to != null)
            {
                DownloadStarted?.Invoke(_itemBeingDownloaded, to);
            }
        }

        private void OnDownloadProgressChanged(object sender, ItemDownloadProgressEventArgs args)
        {
            if (_itemBeingDownloaded != null) // just a null safety check, shouldn't be null in this case
            {
                DownloadMadeProgress?.Invoke(sender, _itemBeingDownloaded, args);
            }
        }

        private void CleanUpUpdateDownloader()
        {
            if (_updateDownloader != null)
            {
                if (ProgressWindow != null)
                {
                    _updateDownloader.DownloadProgressChanged -= ProgressWindow.OnDownloadProgressChanged;
                }
                _updateDownloader.DownloadProgressChanged -= OnDownloadProgressChanged;
                _updateDownloader.DownloadFileCompleted -= OnDownloadFinished;
                _updateDownloader.Dispose();
                _updateDownloader = null;
            }
        }

        private void CreateAndShowProgressWindow(AppCastItem castItem, bool shouldShowAsDownloadedAlready)
        {
            if (ProgressWindow != null)
            {
                ProgressWindow.DownloadProcessCompleted -= ProgressWindowCompleted;
                if (UpdateDownloader != null)
                {
                    UpdateDownloader.DownloadProgressChanged -= ProgressWindow.OnDownloadProgressChanged;
                }
                ProgressWindow = null;
            }
            if (ProgressWindow == null && UIFactory != null && !IsDownloadingSilently())
            {
                ProgressWindow = UIFactory?.CreateProgressWindow(this, castItem);
                if (ProgressWindow != null)
                {
                    ProgressWindow.DownloadProcessCompleted += ProgressWindowCompleted;
                    if (UpdateDownloader != null)
                    {
                        UpdateDownloader.DownloadProgressChanged += ProgressWindow.OnDownloadProgressChanged;
                    }
                    if (shouldShowAsDownloadedAlready)
                    {
                        ProgressWindow?.FinishedDownloadingFile(true);
                        OnDownloadFinished(this, new AsyncCompletedEventArgs(null, false, null));
                    }
                }
                ProgressWindow?.Show();
                _actionToRunOnProgressWindowShown?.Invoke();
                _actionToRunOnProgressWindowShown = null;
            }
            else
            {
                _actionToRunOnProgressWindowShown?.Invoke();
                _actionToRunOnProgressWindowShown = null;
            }
        }

        private async void ProgressWindowCompleted(object sender, DownloadInstallEventArgs args)
        {
            if (args.ShouldInstall)
            {
                ProgressWindow?.SetDownloadAndInstallButtonEnabled(false); // disable while we ask if we can close up the software
                if (await AskApplicationToSafelyCloseUp())
                {
                    ProgressWindow?.Close();
                    if (_downloadTempFileName == null)
                    {
                        LogWriter?.PrintMessage("Before running downloaded installer, _downloadTempFileName was null. Did you clear it accidentally during the download process?");
                    }
                    else
                    {
                        await RunDownloadedInstaller(_downloadTempFileName);
                    }
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
        /// Called when the installer is downloaded
        /// </summary>
        /// <param name="sender">the object that initiated this event call</param>
        /// <param name="e">information on if the download was successful.</param>
        private void OnDownloadFinished(object? sender, AsyncCompletedEventArgs e)
        {
            bool shouldShowUIItems = !IsDownloadingSilently();

            if (_itemBeingDownloaded == null || _downloadTempFileName == null)
            {
                LogWriter?.PrintMessage("[ERROR] OnDownloadFinished called with no item being downloaded and/or no download temp file name; this is an error and should be fixed");
                return;
            }

            if (e.Cancelled)
            {
                _hasAttemptedFileRedownload = false;
                if (File.Exists(_downloadTempFileName))
                {
                    try
                    {
                        File.Delete(_downloadTempFileName);
                    }
                    catch (Exception deleteEx)
                    {
                        string cleanUpErrorMessage = "Download canceled (Cleanup error): " + deleteEx.Message;
                        if (shouldShowUIItems && ProgressWindow != null && !ProgressWindow.DisplayErrorMessage(cleanUpErrorMessage))
                        {
                            UIFactory?.ShowDownloadErrorMessage(this, cleanUpErrorMessage, AppCastUrl);
                        }
                    }
                }
                LogWriter?.PrintMessage("Download was canceled");
                string errorMessage = "Download canceled";
                if (shouldShowUIItems && ProgressWindow != null && !ProgressWindow.DisplayErrorMessage(errorMessage))
                {
                    UIFactory?.ShowDownloadErrorMessage(this, errorMessage, AppCastUrl);
                }
                DownloadCanceled?.Invoke(_itemBeingDownloaded, _downloadTempFileName);
                return;
            }
            if (e.Error != null)
            {
                // Clean temp files on error too
                if (File.Exists(_downloadTempFileName))
                {
                    try
                    {
                        File.Delete(_downloadTempFileName);
                    }
                    catch (Exception deleteEx)
                    {
                        string cleanUpErrorMessage = "Error while downloading (Cleanup error): " + deleteEx.Message;
                        if (shouldShowUIItems && ProgressWindow != null && !ProgressWindow.DisplayErrorMessage(cleanUpErrorMessage))
                        {
                            UIFactory?.ShowDownloadErrorMessage(this, cleanUpErrorMessage, AppCastUrl);
                        }
                    }
                }
                LogWriter?.PrintMessage("Error on download finished: {0}", e.Error.Message);
                if (shouldShowUIItems && ProgressWindow != null && !ProgressWindow.DisplayErrorMessage(e.Error.Message))
                {
                    UIFactory?.ShowDownloadErrorMessage(this, e.Error.Message, AppCastUrl);
                }
                DownloadHadError?.Invoke(_itemBeingDownloaded, _downloadTempFileName, e.Error);
                return;
            }
            // test the item for signature
            var validationRes = ValidationResult.Invalid;
            if (!e.Cancelled && e.Error == null)
            {
                LogWriter?.PrintMessage("Fully downloaded file exists at {0}", _downloadTempFileName);

                LogWriter?.PrintMessage("Performing signature check");

                // get the assembly
                if (File.Exists(_downloadTempFileName))
                {
                    // check if the file was downloaded successfully
                    string absolutePath = Path.GetFullPath(_downloadTempFileName);
                    if (!File.Exists(absolutePath))
                    {
                        var message = "File not found even though it was reported as downloading successfully!";
                        LogWriter?.PrintMessage(message);
                        DownloadHadError?.Invoke(_itemBeingDownloaded, _downloadTempFileName, new NetSparkleException(message));
                    }

                    // check the signature
                    try
                    {
                        validationRes = SignatureVerifier.VerifySignatureOfFile(_itemBeingDownloaded.DownloadSignature ?? "", _downloadTempFileName);
                    }
                    catch (Exception exc)
                    {
                        LogWriter?.PrintMessage("Error validating signature of file: {0}; {1}", exc.Message, exc.StackTrace ?? "[No stack trace available]");
                        validationRes = ValidationResult.Invalid;
                        DownloadedFileThrewWhileCheckingSignature?.Invoke(_itemBeingDownloaded, _downloadTempFileName);
                    }
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
                LogWriter?.PrintMessage("Invalid signature for downloaded file for app cast: {0}", _downloadTempFileName);
                string errorMessage = "Downloaded file has invalid signature!";
                // Default to showing errors in the progress window. Only go to the UIFactory to show errors if necessary.
                DownloadedFileIsCorrupt?.Invoke(_itemBeingDownloaded, _downloadTempFileName);
                if (shouldShowUIItems && ProgressWindow != null && !ProgressWindow.DisplayErrorMessage(errorMessage))
                {
                    UIFactory?.ShowDownloadErrorMessage(this, errorMessage, AppCastUrl);
                }
                DownloadHadError?.Invoke(_itemBeingDownloaded, _downloadTempFileName, new NetSparkleException(errorMessage));
            }
            else
            {
                LogWriter?.PrintMessage("Signature is valid. File successfully downloaded!");
                DownloadFinished?.Invoke(_itemBeingDownloaded, _downloadTempFileName);
                bool shouldInstallAndRelaunch = UserInteractionMode == UserInteractionMode.DownloadAndInstall;
                if (shouldInstallAndRelaunch)
                {
                    ProgressWindowCompleted(this, new DownloadInstallEventArgs(true));
                }
            }
            _itemBeingDownloaded = null; // done downloading, so we can clear the download status, basically
        }

        /// <summary>
        /// Run the provided app cast item update regardless of what else is going on.
        /// Note that a more up to date download may be taking place, so if you don't
        /// want to run a potentially out-of-date installer, don't use this. This should
        /// only be used if your user wants to update before another update has been
        /// installed AND the file is already downloaded.
        /// This function will verify that the file exists and that the 
        /// signature is valid before running. It will also utilize the
        /// PreparingToExit event to ensure that the application can close.
        /// </summary>
        /// <param name="item">AppCastItem to install</param>
        /// <param name="installPath">Install path to the executable. If not provided, will ask the server for the download path.</param>
        public async Task InstallUpdate(AppCastItem item, string? installPath = null)
        {
            ProgressWindow?.SetDownloadAndInstallButtonEnabled(false); // disable while we ask if we can close up the software
            var path = installPath != null && File.Exists(installPath) ? installPath : await GetDownloadPathForAppCastItem(item);
            if (path == null || string.IsNullOrWhiteSpace(path))
            {
                LogWriter?.PrintMessage("InstallUpdate was called, but the file to install doesn't exist and/or cannot be found");
                InstallUpdateFailed?.Invoke(InstallUpdateFailureReason.FileNotFound, path);
            }
            else if (await AskApplicationToSafelyCloseUp())
            {
                if (File.Exists(path))
                {
                    ValidationResult result;
                    try
                    {
                        result = SignatureVerifier.VerifySignatureOfFile(item.DownloadSignature ?? "", path);
                    }
                    catch (Exception exc)
                    {
                        LogWriter?.PrintMessage("Error validating signature of file: {0}; {1}", exc.Message, exc.StackTrace ?? "[No stack trace available]");
                        result = ValidationResult.Invalid;
                        DownloadedFileThrewWhileCheckingSignature?.Invoke(item, path);
                    }
                    if (result == ValidationResult.Valid || result == ValidationResult.Unchecked)
                    {
                        await RunDownloadedInstaller(path);
                    }
                    else
                    {
                        LogWriter?.PrintMessage("InstallUpdate was called, but the file validation result was {0}, so we could not install the file", result);
                        InstallUpdateFailed?.Invoke(InstallUpdateFailureReason.InvalidSignature, path);
                    }
                }
                else
                {
                    LogWriter?.PrintMessage("InstallUpdate was called, but the file to install doesn't exist on disk at path {0}", path);
                    InstallUpdateFailed?.Invoke(InstallUpdateFailureReason.FileNotFound, path);
                }
            }
            ProgressWindow?.SetDownloadAndInstallButtonEnabled(true);
        }

        /// <summary>
        /// Checks to see if the item passed into this function is the one being downloaded
        /// </summary>
        /// <param name="item"><seealso cref="AppCastItem"/> to check</param>
        /// <returns>true if this item is currently being downloaded; false otherwise</returns>
        public bool IsDownloadingItem(AppCastItem item)
        {
            return _itemBeingDownloaded?.DownloadSignature == item.DownloadSignature;
        }

        /// <summary>
        /// True if the user has silent updates enabled; false otherwise.
        /// </summary>
        private bool IsDownloadingSilently()
        {
            return UserInteractionMode != UserInteractionMode.NotSilent;
        }

        /// <summary>
        /// Checks to see if two extensions match (this is basically just a 
        /// convenient string comparison). Both extensions should include the
        /// initial . (full-stop/period) in the extension.
        /// </summary>
        /// <param name="extension">first extension to check</param>
        /// <param name="otherExtension">other extension to check</param>
        /// <returns>true if the extensions match; false otherwise</returns>
        protected bool DoExtensionsMatch(string extension, string otherExtension)
        {
            return extension.Equals(otherExtension, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Get the install command for the file at the given path. Figures out which
        /// command to use based on the download file path's file extension.
        /// Currently supports .exe, .msi, and .msp.
        /// </summary>
        /// <param name="downloadFilePath">Path to the downloaded update file</param>
        /// <returns>the installer command if the file has one of the given 
        /// extensions; the initial downloadFilePath if not.</returns>
        protected virtual string GetWindowsInstallerCommand(string downloadFilePath)
        {
            string installerExt = Path.GetExtension(downloadFilePath);
            if (DoExtensionsMatch(installerExt, ".exe"))
            {
                return "\"" + downloadFilePath + "\"";
            }
            if (DoExtensionsMatch(installerExt, ".msi"))
            {
                return "msiexec /i \"" + downloadFilePath + "\"";
            }
            if (DoExtensionsMatch(installerExt, ".msp"))
            {
                return "msiexec /p \"" + downloadFilePath + "\"";
            }
            return downloadFilePath;
        }

        /// <summary>
        /// Get the install command for the file at the given path. Figures out which
        /// command to use based on the download file path's file extension.
        /// <para>Windows: currently supports .exe, .msi, and .msp.</para>
        /// <para>macOS: currently supports .tar, .tar.gz, .pkg, .dmg, and .zip.</para>
        /// <para>Linux: currently supports .tar.gz, .deb, and .rpm.</para>
        /// </summary>
        /// <param name="downloadFilePath">Path to the downloaded update file</param>
        /// <returns>the installer command if the file has one of the given 
        /// extensions; the initial downloadFilePath if not.</returns>
        protected virtual string GetInstallerCommand(string downloadFilePath)
        {
            // get the file type
#if NETFRAMEWORK
            return GetWindowsInstallerCommand(downloadFilePath);
#else
            string installerExt = Path.GetExtension(downloadFilePath);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsInstallerCommand(downloadFilePath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (DoExtensionsMatch(installerExt, ".pkg") ||
                    DoExtensionsMatch(installerExt, ".dmg"))
                {
                    return "open \"" + downloadFilePath + "\"";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (DoExtensionsMatch(installerExt, ".deb"))
                {
                    return "sudo dpkg -i \"" + downloadFilePath + "\"";
                }
                if (DoExtensionsMatch(installerExt, ".rpm"))
                {
                    return "sudo rpm -i \"" + downloadFilePath + "\"";
                }
            }
            return downloadFilePath;
#endif
        }



        /// <summary>
        /// Checks to see if the file at the path is a zip download.
        /// If user is on macOS and extension is a .tar/.tar.gz/.zip, returns true.
        /// If user is on Linux and extension is a .tar.gz, returns true.
        /// Otherwise returns false. Always returns false on .NET Framework.
        /// </summary>
        /// <param name="downloadFilePath">Path to the downloaded update file</param>
        /// <returns>True if on macOS and path is a .zip, true of on Linux and path is a 
        /// .tar.gz. False otherwise.</returns>
        protected bool IsZipDownload(string downloadFilePath)
        {
#if NETCORE
            string installerExt = Path.GetExtension(downloadFilePath);
            bool isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            if ((isMacOS && (
                    DoExtensionsMatch(installerExt, ".zip") ||
                    DoExtensionsMatch(installerExt, ".tar") ||
                    downloadFilePath.EndsWith(".tar.gz"))) ||
                (isLinux && downloadFilePath.EndsWith(".tar.gz")))
            {
                return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Updates the application via the file at the given path. Figures out which command needs
        /// to be run, sets up the application so that it will start the downloaded file once the
        /// main application stops, and then waits to start the downloaded update.
        /// </summary>
        /// <param name="downloadFilePath">path to the downloaded installer/updater</param>
        /// <returns>the awaitable <see cref="Task"/> for the application quitting</returns>
        protected virtual async Task RunDownloadedInstaller(string downloadFilePath)
        {
            LogWriter?.PrintMessage("Running downloaded installer");
            // get the options for restarting the application
            string executableName = RestartExecutableName;
            string workingDir = RestartExecutablePath;

            // generate the batch file path
#if NETFRAMEWORK
            bool isWindows = true;
            bool isMacOS = false;
#else
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#endif
            var extension = isWindows ? ".cmd" : ".sh";
            string batchFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
            string installerCmd;
            try
            {
                installerCmd = GetInstallerCommand(downloadFilePath);
                if (!string.IsNullOrWhiteSpace(CustomInstallerArguments))
                {
                    installerCmd += " " + CustomInstallerArguments;
                }
            }
            catch (InvalidDataException)
            {
                LogWriter?.PrintMessage("Unknown installer format at {0}", downloadFilePath);
                UIFactory?.ShowUnknownInstallerFormatMessage(this, downloadFilePath);
                InstallUpdateFailed?.Invoke(InstallUpdateFailureReason.CouldNotBuildInstallerCommand, downloadFilePath);
                return;
            }

            // generate the batch file                
            LogWriter?.PrintMessage("Generating batch in {0}", Path.GetFullPath(batchFilePath));

            string processID = ProcessIDToKillBeforeInstallerRuns;
            string relaunchAfterUpdate = "";
            if (RelaunchAfterUpdate)
            {
                var relaunchLine = $@"{RelaunchAfterUpdateCommandPrefix ?? ""}""{executableName}""";
                relaunchAfterUpdate = $@"
                    cd ""{workingDir}""
                    {relaunchLine.Trim()}";
            }

            using (FileStream stream = new FileStream(batchFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, true))
            using (StreamWriter write = new StreamWriter(stream, new UTF8Encoding(false))/*new StreamWriter(batchFilePath, false, new UTF8Encoding(false))*/)
            {
                // We should wait until the host process has died before starting the installer.
                // This way, any DLLs or other items can be replaced properly.
                // Code from: http://stackoverflow.com/a/22559462/3938401
                // This only applies if !ShouldKillParentProcessWhenStartingInstaller

                if (isWindows)
                {
                    var killProcessScript = ShouldKillParentProcessWhenStartingInstaller
                        ? $@"
                            set /A counter=0                       
                            setlocal ENABLEDELAYEDEXPANSION
                            :loop
                            set /A counter=!counter!+1
                            if !counter! == 90 (
                                exit /b 1
                            )
                            tasklist | findstr ""\<{processID}\>"" > nul
                            if not errorlevel 1 (
                                timeout /t 1 > nul
                                goto :loop
                            )"
                        : "";
                    string output = $@"
                        @echo off
                        chcp 65001 > nul
                        {killProcessScript}
                        :install
                        {installerCmd}
                        :afterinstall
                        {relaunchAfterUpdate.Trim()}
                        endlocal";
                    await write.WriteAsync(output);
                    write.Close();
                }
                else
                {
                    // We should wait until the host process has died before starting the installer.
                    var killProcessScript = ShouldKillParentProcessWhenStartingInstaller
                        ? $@"
                        COUNTER=0;
                        while ps -p {processID} > /dev/null;
                            do sleep 1;
                            COUNTER=$((++COUNTER));
                            if [ $COUNTER -eq 90 ] 
                            then
                                exit -1;
                            fi;
                        done;"
                        : "";
                    if (IsZipDownload(downloadFilePath)) // .zip on macOS or .tar.gz on Linux
                    {
                        // waiting for finish based on http://blog.joncairns.com/2013/03/wait-for-a-unix-process-to-finish/
                        // use tar to extract
                        var tarCommand = isMacOS ? $"tar -x -f \"{downloadFilePath}\" -C \"{workingDir}\""
                            : $"tar -xf \"{downloadFilePath}\" -C \"{workingDir}\" --overwrite ";
                        var output = $@"
                            {killProcessScript}
                            {tarCommand}
                            {relaunchAfterUpdate}";
                        await write.WriteAsync(output.Replace("\r\n", "\n"));
                    }
                    else
                    {
                        string installerExt = Path.GetExtension(downloadFilePath);
                        if (DoExtensionsMatch(installerExt, ".pkg") ||
                            DoExtensionsMatch(installerExt, ".dmg"))
                        {
                            relaunchAfterUpdate = ""; // relaunching not supported for pkg or dmg downloads
                        }
                        var output = $@"
                            {killProcessScript}
                            {installerCmd}
                            {relaunchAfterUpdate}";
                        await write.WriteAsync(output.Replace("\r\n", "\n"));
                    }
                    write.Close();
                    // if we're on unix, we need to make the script executable!
                    Exec($"chmod +x {batchFilePath}"); // this could probably be made async at some point
                }
            }

            // report
            LogWriter?.PrintMessage("Going to execute script at path: {0}", batchFilePath);

            // init the installer helper
            var didStartInstaller = true;
            if (isWindows)
            {
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
                var shouldContinue = InstallerProcessAboutToStart?.Invoke(_installerProcess, downloadFilePath) ?? true;
                if (shouldContinue)
                {
                    LogWriter?.PrintMessage("Starting the installer process at {0}", batchFilePath);
                    _installerProcess.Start();
                }
                else
                {
                    LogWriter?.PrintMessage("Starting installer was canceled by user via InstallerProcessAboutToStart");
                    didStartInstaller = false;
                    InstallUpdateFailed?.Invoke(InstallUpdateFailureReason.CanceledByUserViaEvent, downloadFilePath);
                }
            }
            else
            {
                // on macOS need to use bash to execute the shell script
                LogWriter?.PrintMessage("Starting the installer script process at {0} via shell exec", batchFilePath);
                didStartInstaller = Exec(batchFilePath, false); // _installerProcess will be set up in `Exec`
                if (!didStartInstaller)
                {
                    LogWriter?.PrintMessage("Starting installer was canceled by user via InstallerProcessAboutToStart while in Exec()");
                    InstallUpdateFailed?.Invoke(InstallUpdateFailureReason.CanceledByUserViaEvent, downloadFilePath);
                }
            }
            if (didStartInstaller && ShouldKillParentProcessWhenStartingInstaller)
            {
                await QuitApplication();
            }
        }

        // Exec grabbed from https://stackoverflow.com/a/47918132/3938401
        // for easy shell commands

        /// <summary>
        /// Execute a shell script.
        /// <para>https://stackoverflow.com/a/47918132/3938401</para>
        /// </summary>
        /// <param name="cmd">Path to script to run via a shell</param>
        /// <param name="waitForExit">True for the calling process to wait for the command to finish before exiting; false otherwise</param>
        /// <param name="downloadFilePath">Optional param for download file that is being executed via installer</param>
        /// <returns>true if process started, false if user canceled installer via InstallerProcessAboutToStart</returns>
        protected bool Exec(string cmd, bool waitForExit = true, string downloadFilePath = "")
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            var shell = "";
            try
            {
                // leave nothing up to chance :)
                shell = System.Environment.GetEnvironmentVariable("SHELL");
            }
            catch { }
            if (string.IsNullOrWhiteSpace(shell))
            {
                shell = "/bin/sh";
            }
            LogWriter?.PrintMessage("Shell is {0}", shell);

            _installerProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = shell,
                    Arguments = $"-c \"{escapedArgs}\""
                }
            };
            var shouldContinue = InstallerProcessAboutToStart?.Invoke(_installerProcess, downloadFilePath) ?? true;
            if (shouldContinue)
            {
                LogWriter?.PrintMessage("Starting the process via {1} -c \"{0}\"", escapedArgs, shell);
                _installerProcess.Start();
                if (waitForExit)
                {
                    LogWriter?.PrintMessage("Waiting for exit...");
                    _installerProcess.WaitForExit();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Quits the application (host application) that is using/started SparkleUpdater
        /// </summary>
        /// <returns>Asynchronous task that can be awaited to call code after the application 
        /// is quit (which may or may not be worth it based on your program setup)</returns>
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
                    LogWriter?.PrintMessage("Shutting down application via CloseApplicationAsync...");
                    await CloseApplicationAsync.Invoke();
                }
                else if (CloseApplication != null)
                {
                    LogWriter?.PrintMessage("Shutting down application via CloseApplication...");
                    CloseApplication.Invoke();
                }
                else
                {
                    // Because the download/install window is usually on a separate thread,
                    // send dual shutdown messages via both the sync context (kills "main" app)
                    // and the current thread (kills current thread)
                    LogWriter?.PrintMessage("Shutting down application via UIFactory...");
                    UIFactory?.Shutdown(this);
                }
            }
            catch (Exception e)
            {
                LogWriter?.PrintMessage(e.Message);
            }
        }

        /// <summary>
        /// Ask the application to close their current work items. 
        /// Apps may need, for example, to let the user save their work
        /// </summary>
        /// <returns>true if it's OK to run the installer and close the software; false otherwise</returns>
        private async Task<bool> AskApplicationToSafelyCloseUp()
        {
            try
            {
                // In case the user has shut the window that started this Sparkle window/instance, don't crash and burn.
                // If you have better ideas on how to figure out if they've shut all other windows, let me know...
                if (PreparingToExitAsync != null)
                {
                    var args = new CancelEventArgs();
                    await PreparingToExitAsync(this, args);
                    return !args.Cancel;
                }
                else if (PreparingToExit != null)
                {
                    var args = new CancelEventArgs();
                    PreparingToExit(this, args);
                    return !args.Cancel;
                }
            }
            catch (Exception e)
            {
                LogWriter?.PrintMessage(e.Message);
            }
            return true;
        }


        /// <summary>
        /// Check for updates, using UI interaction appropriate for if the user initiated the update request
        /// <param name="ignoreSkippedVersions">true to ignore skipped versions, false otherwise</param>
        /// </summary>
        public async Task<UpdateInfo> CheckForUpdatesAtUserRequest(bool ignoreSkippedVersions = false)
        {
            if (CheckingForUpdatesWindow == null)
            {
                CheckingForUpdatesWindow = UIFactory?.ShowCheckingForUpdates(this);
                if (CheckingForUpdatesWindow != null)
                {
                    CheckingForUpdatesWindow.UpdatesUIClosing += CheckingForUpdatesWindow_Closing; // to detect canceling
                }
            }
            CheckingForUpdatesWindow?.Show();
            // artificial delay -- if internet is super fast and the update check is super fast, the flash (fast show/hide) of the
            // 'Checking for Updates...' window is very disorienting, so we add an artificial delay
            await Task.Delay(250);
            UpdateInfo updateData = await CheckForUpdates(true, ignoreSkippedVersions); // handles UpdateStatus.UpdateAvailable (in terms of UI)
            if (CheckingForUpdatesWindow != null) // if null, user closed 'Checking for Updates...' window or the UIFactory was null
            {
                CheckingForUpdatesWindow?.Close();
                switch (updateData.Status)
                {
                    case UpdateStatus.UpdateNotAvailable:
                        UIFactory?.ShowVersionIsUpToDate(this);
                        break;
                    case UpdateStatus.UserSkipped:
                        UIFactory?.ShowVersionIsSkippedByUserRequest(this); // they can get skipped version from Configuration
                        break;
                    case UpdateStatus.CouldNotDetermine:
                        UIFactory?.ShowCannotDownloadAppcast(this, AppCastUrl);
                        break;
                }
            }

            return updateData; // in this case, we've already shown UI talking about the new version
        }

        private void CheckingForUpdatesWindow_Closing(object? sender, EventArgs e)
        {
            if (CheckingForUpdatesWindow != null)
            {
                // make sure event is removed just in case
                CheckingForUpdatesWindow.UpdatesUIClosing -= CheckingForUpdatesWindow_Closing;
            }
            CheckingForUpdatesWindow = null;
        }

        /// <summary>
        /// Check for updates, using interaction appropriate for where the user doesn't know you're doing it, so be polite.
        /// Basically, this checks for updates without showing a UI. NO UI WILL BE SHOWN. You must handle any showing
        /// of the UI yourself -- see the "HandleEventsYourself" sample!
        /// <param name="ignoreSkippedVersions">true to ignore skipped versions, false otherwise</param>
        /// </summary>
        public async Task<UpdateInfo> CheckForUpdatesQuietly(bool ignoreSkippedVersions = false)
        {
            return await CheckForUpdates(false, ignoreSkippedVersions);
        }

        /// <summary>
        /// Perform a one-time check for updates
        /// <param name="isUserManuallyCheckingForUpdates">true if user triggered the update check (so show UI), false otherwise (no UI)</param>
        /// <param name="ignoreSkippedVersions">true to ignore skipped versions, false otherwise</param>
        /// </summary>
        private async Task<UpdateInfo> CheckForUpdates(bool isUserManuallyCheckingForUpdates, bool ignoreSkippedVersions = false)
        {
            UpdateCheckStarted?.Invoke(this);
            Configuration config = Configuration;

            // check if update is required
            _latestDownloadedUpdateInfo = await GetUpdateStatus(config, ignoreSkippedVersions);
            List<AppCastItem> updates = _latestDownloadedUpdateInfo.Updates;
            if (_latestDownloadedUpdateInfo.Status == UpdateStatus.UpdateAvailable)
            {
                // there's an update available!
                LogWriter?.PrintMessage("Update needed from version {0} to version {1}", config.InstalledVersion, updates[0].Version ?? "[unknown version]");

                UpdateDetectedEventArgs ev = new UpdateDetectedEventArgs(
                    NextUpdateAction.ShowStandardUserInterface,
                    config,
                    updates[0],
                    updates
                );

                // UpdateDetected allows for catching and overriding the update handling,
                // so if the user has implemented it, tell them there is an update and stop
                // handling everything.
                if (UpdateDetected != null)
                {
                    UpdateDetected(this, ev); // event's next action can change, here
                    switch (ev.NextAction)
                    {
                        case NextUpdateAction.PerformUpdateUnattended:
                            {
                                LogWriter?.PrintMessage("Unattended update desired from consumer");
                                UserInteractionMode = UserInteractionMode.DownloadAndInstall;
                                UpdatesHaveBeenDownloaded(updates);
                                break;
                            }
                        case NextUpdateAction.ProhibitUpdate:
                            {
                                LogWriter?.PrintMessage("Update prohibited from consumer");
                                break;
                            }
                        case NextUpdateAction.ShowStandardUserInterface:
                            {
                                LogWriter?.PrintMessage("Showing standard update UI");
                                // don't show UI if we are quietly checking for updates with no UI
                                if (!isUserManuallyCheckingForUpdates)
                                {
                                    UpdatesHaveBeenDownloaded(updates);
                                }
                                break;
                            }
                    }
                }
                else if (isUserManuallyCheckingForUpdates)
                {
                    // user checked for updates, so show the UI
                    ShowUpdateNeededUI(updates);
                }
            }
            UpdateCheckFinished?.Invoke(this, _latestDownloadedUpdateInfo.Status);
            return _latestDownloadedUpdateInfo;
        }

        /// <summary>
        /// Cancels an in-progress download of an app cast file and deletes the temporary file.
        /// </summary>
        public void CancelFileDownload()
        {
            LogWriter?.PrintMessage("Canceling download...");
            if (UpdateDownloader != null && UpdateDownloader.IsDownloading)
            {
                UpdateDownloader.CancelDownload();
            }
        }

        private async void OnUpdateWindowUserResponded(object sender, UpdateResponseEventArgs args)
        {
            LogWriter?.PrintMessage("Update window response: {0}", args.Result);
            var currentItem = args.UpdateItem;
            var result = args.Result;
            if (result == UpdateAvailableResult.SkipUpdate)
            {
                if (currentItem.Version != null)
                {
                    Configuration.SetVersionToSkip(currentItem.Version);
                }
                UserRespondedToUpdate?.Invoke(this, new UpdateResponseEventArgs(result, currentItem));
            }
            else if (result == UpdateAvailableResult.InstallUpdate)
            {
                if (UserInteractionMode == UserInteractionMode.DownloadNoInstall && string.IsNullOrWhiteSpace(_downloadTempFileName))
                {
                    // we need the download file name in order to run the installer
                    _downloadTempFileName = await GetDownloadPathForAppCastItem(currentItem);
                }
                UserRespondedToUpdate?.Invoke(this, new UpdateResponseEventArgs(result, currentItem));
                if (UserInteractionMode == UserInteractionMode.DownloadNoInstall && File.Exists(_downloadTempFileName))
                {
                    // Binary should already be downloaded. Run it!
                    ProgressWindowCompleted(this, new DownloadInstallEventArgs(true));
                }
                else
                {
                    // download the binaries
                    await InitAndBeginDownload(currentItem);
                }
            }
            else
            {
                UserRespondedToUpdate?.Invoke(this, new UpdateResponseEventArgs(result, currentItem));
            }

            if (result != UpdateAvailableResult.None)
            {
                // if result is None, then user closed the window some other way to ignore things so we don't need
                // to close it
                UpdateAvailableWindow?.Close();
                UpdateAvailableWindow = null; // done using the window so don't hold onto reference
            }
            CheckingForUpdatesWindow?.Close();
            CheckingForUpdatesWindow = null;
        }

        /// <summary>
        /// Loop that occasionally checks for updates for the running application
        /// </summary>
        private async void OnWorkerDoWork()
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
                bool isUpdateAvailable = false;

                // notify
                LoopStarted?.Invoke(this);

                // report status
                if (doInitialCheck)
                {
                    // report status
                    LogWriter?.PrintMessage("Starting update loop...");

                    // read the config
                    LogWriter?.PrintMessage("Reading config...");
                    Configuration config = Configuration;

                    // calc CheckTasp
                    bool checkTSPInternal = checkTSP;

                    if (isInitialCheck && checkTSPInternal)
                    {
                        checkTSPInternal = !_forceInitialCheck;
                    }

                    // check if it's ok the recheck to software state
                    TimeSpan csp = DateTime.Now - config.LastCheckTime;

                    LogWriter?.PrintMessage("Done reading config. !CheckTSPInternal = {0}; csp >= _checkFrequency = {1}", 
                        !checkTSPInternal, csp >= _checkFrequency);
                    if (!checkTSPInternal || csp >= _checkFrequency)
                    {
                        checkTSP = true;
                        // when sparkle will be deactivated wait another cycle
                        LogWriter?.PrintMessage("Config has check for update true? {0}", config.CheckForUpdate == true);
                        if (config.CheckForUpdate == true)
                        {
                            // update the runonce feature
                            goIntoLoop = !config.IsFirstRun;

                            // check if update is required
                            if (_cancelToken.IsCancellationRequested || !goIntoLoop)
                            {
                                LogWriter?.PrintMessage("Cancellation token had cancellation requested and/or goIntoLoop was false");
                                break;
                            }
                            LogWriter?.PrintMessage("In worker thread loop, getting update status...");
                            _latestDownloadedUpdateInfo = await GetUpdateStatus(config);
                            if (_cancelToken.IsCancellationRequested)
                            {
                                break;
                            }
                            isUpdateAvailable = _latestDownloadedUpdateInfo.Status == UpdateStatus.UpdateAvailable;
                            LogWriter?.PrintMessage("In worker thread loop, is update available? {0}", isUpdateAvailable);
                            if (isUpdateAvailable)
                            {
                                List<AppCastItem> updates = _latestDownloadedUpdateInfo.Updates;
                                // show the update window
                                LogWriter?.PrintMessage("Update needed from version {0} to version {1}", config.InstalledVersion, updates[0].Version ?? "[Unknown version]");

                                // send notification if needed
                                UpdateDetectedEventArgs ev = new UpdateDetectedEventArgs(
                                    NextUpdateAction.ShowStandardUserInterface,
                                    config,
                                    updates[0],
                                    updates
                                );
                                UpdateDetected?.Invoke(this, ev);
                                if (_cancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                // check results
                                switch (ev.NextAction)
                                {
                                    case NextUpdateAction.PerformUpdateUnattended:
                                        {
                                            LogWriter?.PrintMessage("Unattended update desired from consumer");
                                            UserInteractionMode = UserInteractionMode.DownloadAndInstall;
                                            UpdatesHaveBeenDownloaded(updates);
                                            break;
                                        }
                                    case NextUpdateAction.ProhibitUpdate:
                                        {
                                            LogWriter?.PrintMessage("Update prohibited from consumer");
                                            break;
                                        }
                                    default:
                                        {
                                            LogWriter?.PrintMessage("Preparing to show standard update UI");
                                            UpdatesHaveBeenDownloaded(updates);
                                            break;
                                        }
                                }
                            }
                        }
                        else
                        {
                            LogWriter?.PrintMessage("Check for updates disabled");
                        }
                    }
                    else
                    {
                        LogWriter?.PrintMessage("Update check performed within the last {0} minutes!", _checkFrequency.TotalMinutes);
                    }
                }
                else
                {
                    LogWriter?.PrintMessage("Initial check prohibited, going to wait");
                    doInitialCheck = true;
                }

                // checking is done; this is now the "let's wait a while" section

                // reset initial check
                isInitialCheck = false;

                // notify
                LoopFinished?.Invoke(this, isUpdateAvailable);

                // report wait statement
                LogWriter?.PrintMessage("Sleeping for another {0} minutes, exit event or force update check event", _checkFrequency.TotalMinutes);

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
                    LogWriter?.PrintMessage("{0} minutes are over", _checkFrequency.TotalMinutes);
                    continue;
                }

                // check the exit handle
                if (i == 0)
                {
                    LogWriter?.PrintMessage("Got exit signal");
                    break;
                }

                // check another check needed
                if (i == 1)
                {
                    LogWriter?.PrintMessage("Got force update check signal");
                    checkTSP = false;
                }
                if (_cancelToken.IsCancellationRequested)
                {
                    break;
                }
            } while (goIntoLoop);

            // reset the islooping handle
            if (!_disposed)
            {
                _loopingHandle?.Reset();
            }
        }

        /// <summary>
        /// This method will be notified by the SparkleUpdater loop when
        /// some update info has been downloaded. If the info has been 
        /// downloaded fully (e.ProgressPercentage == 1), the UI
        /// for downloading updates will be shown (if not downloading silently)
        /// or the download will be performed (if downloading silently).
        /// </summary>
        private void OnWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 1:
                    UpdatesHaveBeenDownloaded(e.UserState as List<AppCastItem>);
                    break;
                case 0:
                    var state = e.UserState?.ToString();
                    if (state != null)
                    {
                        LogWriter?.PrintMessage(state);
                    }
                    break;
            }
        }

        /// <summary>
        /// Updates from appcast have been downloaded from the server.
        /// If the user is downloading silently, the download will begin.
        /// If the user is not downloading silently, the update UI will be shown.
        /// </summary>
        /// <param name="updates">Updates to be installed. If null, nothing will happen.</param>
        private async void UpdatesHaveBeenDownloaded(List<AppCastItem>? updates)
        {
            if (updates != null)
            {
                if (IsDownloadingSilently())
                {
                    await InitAndBeginDownload(updates[0]); // install only latest
                }
                else
                {
                    // show the update UI
                    ShowUpdateNeededUI(updates);
                }
            }
        }
    }
}
