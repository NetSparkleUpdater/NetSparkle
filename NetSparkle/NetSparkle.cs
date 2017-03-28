using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;
using NetSparkle.Interfaces;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Threading;

namespace NetSparkle
{
    /// <summary>
    /// The states of availability
    /// </summary>
    /// <paramater>UpdateAvailable</paramater>
#pragma warning disable 1591
    public enum UpdateStatus { UpdateAvailable, UpdateNotAvailable, UserSkipped, CouldNotDetermine }
#pragma warning restore 1591

    /// <summary>
    /// A simple class to hold information on potential updates to a software product.
    /// </summary>
    public class SparkleUpdateInfo
    {
        /// <summary>
        /// Update availability.
        /// </summary>
        public UpdateStatus Status { get; set; }
        /// <summary>
        /// Any available updates for the product.
        /// </summary>
        public NetSparkleAppCastItem[] Updates { get; set; }
        /// <summary>
        /// Constructor for SparkleUpdate when there are some updates available.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="updates"></param>
        public SparkleUpdateInfo(UpdateStatus status, NetSparkleAppCastItem[] updates)
        {
            Status = status;
            Updates = updates;
        }
        /// <summary>
        /// Constructor for SparkleUpdate for when there aren't any updates available. Updates are automatically set to null.
        /// </summary>
        /// <param name="status"></param>
        public SparkleUpdateInfo(UpdateStatus status)
        {
            Status = status;
            Updates = null;
        }
    }

    /// <summary>
    /// The operation has started
    /// </summary>
    /// <param name="sender">the sender</param>
    public delegate void LoopStartedOperation(Object sender);
    /// <summary>
    /// The operation has ended
    /// </summary>
    /// <param name="sender">the sender</param>
    /// <param name="updateRequired"><c>true</c> if an update is required</param>
    public delegate void LoopFinishedOperation(Object sender, Boolean updateRequired);

    /// <summary>
    /// This delegate will be used when an update was detected to allow library 
    /// consumer to add own user interface capabilities.    
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void UpdateDetected(Object sender, UpdateDetectedEventArgs e);

    /// <summary>
    /// Update check has started.
    /// </summary>
    /// <param name="sender">Sparkle updater that is checking for an update.</param>
    public delegate void UpdateCheckStarted(Object sender);

    /// <summary>
    /// Update check has finished.
    /// </summary>
    /// <param name="sender">Sparkle updater that finished checking for an update.</param>
    /// <param name="status">Update status</param>
    public delegate void UpdateCheckFinished(Object sender, UpdateStatus status);

    /// <summary>
    /// An asynchronous cancel event handler.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A System.ComponentModel.CancelEventArgs that contains the event data.</param>
    /// <returns></returns>
    public delegate Task CancelEventHandlerAsync(object sender, CancelEventArgs e);

    /// <summary>
    /// Handler for when a downloaded file is ready. Useful when using 
    /// SilentModeTypes.DownloadNoInstall so you can let your user know when the downloaded
    /// update is ready.
    /// </summary>
    /// <param name="item">App cast details of the downloaded item</param>
    /// <param name="downloadPath">Path of the downloaded software in case you want to start it yourself</param>
    public delegate void DownloadedFileReady(NetSparkleAppCastItem item, string downloadPath);

    /// <summary>
    /// Called when the file is fully downloaded, but the DSA can't be verified.
    /// This could allow you to tell the user what happened if updates are silent.
    /// Note that silent updates will not delete the corrupted file until the next download loop.
    /// </summary>
    /// <param name="item">App cast details of the downloaded item</param>
    /// <param name="downloadPath">Path of the invalid software download</param>
    public delegate void DownloadedFileIsCorrupt(NetSparkleAppCastItem item, string downloadPath);

    /// <summary>
    /// Due to weird WPF issues that I don't have time to debug (sorry), delegate for
    /// knowing when the window needs to close
    /// </summary>
    public delegate void CloseWPFSoftware();

    /// <summary>
    /// Async version of CloseWPFSoftware().
    /// Due to weird WPF issues that I don't have time to debug (sorry), delegate for
    /// knowing when the window needs to close
    /// </summary>
    public delegate Task CloseWPFSoftwareAsync();

    /// <summary>
    /// Class to communicate with a sparkle-based appcast
    /// </summary>
    public class Sparkle : IDisposable
    {
        /// <summary>
        /// Subscribe to this to get a chance to shut down gracefully before quitting
        /// </summary>
        public event CancelEventHandler AboutToExitForInstallerRun;

        /// <summary>
        /// Subscribe to this to get a chance to asynchronously shut down gracefully before quitting
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
        /// Event called for closing a WPF window.
        /// If CloseWPFWindowAsync is non-null, CloseWPFWindow is never called.
        /// </summary>
        public event CloseWPFSoftware CloseWPFWindow;

        /// <summary>
        /// Event called for closing a WPF window asynchronously.
        /// </summary>
        public event CloseWPFSoftwareAsync CloseWPFWindowAsync;

        /// <summary>
        /// Called when update check has just started
        /// </summary>
        public event UpdateCheckStarted UpdateCheckStarted;

        /// <summary>
        /// Called when update check is all done. May or may not have called UpdateDetected in the middle.
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

        //private BackgroundWorker _worker;
        private Task _taskWorker;
        private CancellationToken _cancelToken;
        private CancellationTokenSource _cancelTokenSource;
        private SynchronizationContext _syncContext;
        private string _appCastUrl;
        private readonly string _appReferenceAssembly;

        private bool _doInitialCheck;
        private bool _forceInitialCheck;

        private readonly EventWaitHandle _exitHandle;
        private readonly EventWaitHandle _loopingHandle;
        private readonly Icon _applicationIcon;       
        private TimeSpan _checkFrequency;
        private bool _useNotificationToast;

        private string _downloadTempFileName;
        private WebClient _webDownloadClient;
        private Process _installerProcess;
        private NetSparkleAppCastItem _itemBeingDownloaded;
        private bool _hasAttemptedFileRedownload;

        /// <summary>
        /// ctor which needs the appcast url
        /// </summary>
        /// <param name="appcastUrl">the URL for the appcast file</param>
        public Sparkle(String appcastUrl)
            : this(appcastUrl, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url
        /// </summary>
        /// <param name="appcastUrl">the URL for the appcast file</param>
        /// <param name="applicationIcon">If you're invoking this from a form, this would be this.Icon</param>
        public Sparkle(String appcastUrl, Icon applicationIcon)
            : this(appcastUrl, applicationIcon, SecurityMode.Strict, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url
        /// </summary>
        /// <param name="appcastUrl">the URL for the appcast file</param>
        /// <param name="applicationIcon">If you're invoking this from a form, this would be this.Icon</param>
        /// <param name="securityMode">Sparkle Security mode</param>
        public Sparkle(String appcastUrl, Icon applicationIcon, SecurityMode securityMode)
            : this(appcastUrl, applicationIcon, securityMode, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url
        /// </summary>
        /// <param name="appcastUrl">the URL for the appcast file</param>
        /// <param name="applicationIcon">If you're invoking this from a form, this would be this.Icon</param>
        /// <param name="dsaPublicKey">The dsa public key to verfiy the sigatures.</param>
        public Sparkle(String appcastUrl, Icon applicationIcon, SecurityMode securityMode, String dsaPublicKey)
            : this(appcastUrl, applicationIcon, securityMode, dsaPublicKey, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url and a referenceassembly
        /// </summary>        
        /// <param name="appcastUrl">the URL for the appcast file</param>
        /// <param name="applicationIcon">If you're invoking this from a form, this would be this.Icon</param>
        /// <param name="referenceAssembly">the name of the assembly to use for comparison</param>
        public Sparkle(String appcastUrl, Icon applicationIcon, SecurityMode securityMode, String dsaPublicKey, String referenceAssembly) 
            : this(appcastUrl, applicationIcon, securityMode, dsaPublicKey, referenceAssembly, new DefaultNetSparkleUIFactory())
        { }

        /// <summary>
        /// ctor which needs the appcast url and a referenceassembly
        /// </summary>        
        /// <param name="appcastUrl">the URL for the appcast file</param>
        /// <param name="applicationIcon">If you're invoking this from a form, this would be this.Icon</param>
        /// <param name="referenceAssembly">the name of the assembly to use for comparison</param>
        /// <param name="factory">UI factory to use</param>
        public Sparkle(String appcastUrl, Icon applicationIcon, SecurityMode securityMode, String dsaPublicKey, String referenceAssembly, INetSparkleUIFactory factory)
        {
            _applicationIcon = applicationIcon;
            ExtraJsonData = "";
            PrintDiagnosticToConsole = false;
            _hasAttemptedFileRedownload = false;
            UIFactory = factory;
            // DSA Verificator
            DSAVerificator = new NetSparkleDSAVerificator(securityMode, dsaPublicKey);
            // Syncronisation Context
            _syncContext = SynchronizationContext.Current;
            if (_syncContext == null)
            {
                _syncContext = new SynchronizationContext();
            }
            // preconfige ssl trust
            TrustEverySSLConnection = false;
            // configure ssl cert link
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidation;
            // init UI
            UIFactory.Init();
            _appReferenceAssembly = null;            
            // set the reference assembly
            if (referenceAssembly != null)
            {
                _appReferenceAssembly = referenceAssembly;
                Debug.WriteLine("Checking the following file: " + _appReferenceAssembly);
            }

            // TODO: change BackgroundWorker to Task
            // adjust the delegates
            _taskWorker = new Task(() =>
            {
                OnWorkerDoWork(null, null);
            });
            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;

            /*_worker = new BackgroundWorker {WorkerReportsProgress = true};
            _worker.DoWork += OnWorkerDoWork;
            _worker.ProgressChanged += OnWorkerProgressChanged;*/

            // build the wait handle
            _exitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            _loopingHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            
            // set the url
            _appCastUrl = appcastUrl;
            Debug.WriteLine("Using the following url: " + _appCastUrl);
            RunningFromWPF = false;
            SilentMode = SilentModeTypes.NotSilent;
            TmpDownloadFilePath = "";
        }

        /// <summary>
        /// The app will check once, after the app settles down.
        /// </summary>
        public void CheckOnFirstApplicationIdle()
        {
            Application.Idle += OnFirstApplicationIdle;
        }

        private async void OnFirstApplicationIdle(object sender, EventArgs e)
        {
            Application.Idle -= OnFirstApplicationIdle;
            await CheckForUpdates(true);
        }


        #region Properties
        /// <summary>
        /// Enables system profiling against a profile server
        /// </summary>
        public bool EnableSystemProfiling { get; private set; }

        /// <summary>
        /// Hides the release notes view when an update was found. This 
        /// mode is switched on automatically when no sparkle:releaseNotesLink
        /// tag was found in the app cast         
        /// </summary>
        public bool HideReleaseNotes { get; private set; }

        /// <summary>
        /// Contains the profile url for System profiling
        /// </summary>
        public Uri SystemProfileUrl { get; private set; }

        /// <summary>
        /// This property enables the silent mode, this means 
        /// the application will be updated without user interaction
        /// </summary>
        [System.Obsolete("Please use SilentModeType instead.")]
        public bool EnableSilentMode { get; set; }

        /// <summary>
        /// Allows for updating the application with or without user interaction.
        /// </summary>
        public enum SilentModeTypes
        {
            /// <summary>
            /// Shows the change log UI automatically (this is the default)
            /// </summary>
            NotSilent,
            /// <summary>
            /// Downloads the latest update file and changelog automatically, but does not
            /// show any UI until asked to show UI.
            /// </summary>
            DownloadNoInstall,
            /// <summary>
            /// Downloads the latest update file and automatically runs it as an installer file.
            /// WARNING: if you don't tell the user that the application is about to quit
            /// to update/run an installer, this setting might be quite the shock to the user!
            /// Make sure to implement AboutToExitForInstallerRun or AboutToExitForInstallerRunAsync
            /// so that you can show your users what is about to happen.
            /// </summary>
            DownloadAndInstall,
        }

        /// <summary>
        /// Set the silent mode type for Sparkle to use when there is a valid update for the 
        /// software
        /// </summary>
        public SilentModeTypes SilentMode { get; set; }

        /// <summary>
        /// If set, downloads files to this path. If the folder doesn't already exist, creates
        /// the folder. Note that this variable is a path, not a full file name.
        /// </summary>
        public string TmpDownloadFilePath { get; set; }

        /// <summary>
        /// Because of bugs with detecting that the application is closed, setting this to true
        /// offers a quick bug-fix workaround for starting the _installerProcess when updating
        /// a WPF application
        /// </summary>
        public bool RunningFromWPF { get; set; }

        /// <summary>
        /// Defines if the application needs to be relaunched after executing the downloaded installer
        /// </summary>
        public bool RelaunchAfterUpdate { get; set; }

        /// <summary>
        /// If true, prints diagnostic messages to Console.WriteLine rather than Debug.WriteLine
        /// </summary>
        public bool PrintDiagnosticToConsole { get; set; }

        /// <summary>
        /// Run the downloaded installer with these arguments
        /// </summary>
        public string CustomInstallerArguments { get; set; }

        /// <summary>
        /// This property returns true when the update loop is running
        /// and false when the loop is not running
        /// </summary>
        public bool IsUpdateLoopRunning
        {
            get
            {
                return _loopingHandle.WaitOne(0);
            }
        }

        /// <summary>
        /// This property defines if we trust every ssl connection also when 
        /// this connection has not a valid cert
        /// </summary>
        public bool TrustEverySSLConnection { get; set; }

        /// <summary>
        /// Factory for creating UI form like progress window etc.
        /// </summary>
        public INetSparkleUIFactory UIFactory { get; set; }

        /// <summary>
        /// The user interface window that shows the release notes and
        /// asks the user to skip, later or update
        /// </summary>
        public INetSparkleForm UserWindow { get; set; }

        /// <summary>
        /// The user interface window that shows a download progress bar,
        /// and then asks to install and relaunch the application
        /// </summary>
        public INetSparkleDownloadProgress ProgressWindow { get; set; }

        /// <summary>
        /// The user interface window that shows the 'Checking for Updates...'
        /// form. TODO: Make this an interface so user can config their own UI
        /// </summary>
        public CheckingForUpdatesWindow CheckingForUpdatesWindow { get; set; }

        /// <summary>
        /// The configuration.
        /// </summary>
        public NetSparkleConfiguration Configuration { get; set; }

        /// <summary>
        /// The DSA Verificator
        /// </summary>
        public NetSparkleDSAVerificator DSAVerificator { get; set; }

        /// <summary>
        /// Gets or sets the app cast URL
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

        public bool UseSyncronizedForms { get; set; }

        /// <summary>
        /// If not "", sends extra JSON via POST to server with the web request for update information.
        /// </summary>
        public string ExtraJsonData { get; set; }

        #endregion

        /// <summary>
        /// The method starts a NetSparkle background loop
        /// If NetSparkle is configured to check for updates on startup, proceeds to perform 
        /// the check. You should only call this function when your app is initialized and 
        /// shows its main window.        
        /// </summary>        
        /// <param name="doInitialCheck"></param>
        public void StartLoop(Boolean doInitialCheck)
        {
            StartLoop(doInitialCheck, false);
        }

        /// <summary>
        /// The method starts a NetSparkle background loop
        /// If NetSparkle is configured to check for updates on startup, proceeds to perform 
        /// the check. You should only call this function when your app is initialized and 
        /// shows its main window.
        /// </summary>
        /// <param name="doInitialCheck"><c>true</c> if this instance should do an initial check.</param>
        /// <param name="checkFrequency">the frequency between checks.</param>
        public void StartLoop(Boolean doInitialCheck, TimeSpan checkFrequency)
        {
            StartLoop(doInitialCheck, false, checkFrequency);
        }

        /// <summary>
        /// The method starts a NetSparkle background loop
        /// If NetSparkle is configured to check for updates on startup, proceeds to perform 
        /// the check. You should only call this function when your app is initialized and 
        /// shows its main window.
        /// </summary>
        /// <param name="doInitialCheck"><c>true</c> if this instance should do an initial check.</param>
        /// <param name="forceInitialCheck"><c>true</c> if this instance should force an initial check.</param>
        public void StartLoop(Boolean doInitialCheck, Boolean forceInitialCheck)
        {
            StartLoop(doInitialCheck, forceInitialCheck, TimeSpan.FromHours(24));
        }

        /// <summary>
        /// The method starts a NetSparkle background loop
        /// If NetSparkle is configured to check for updates on startup, proceeds to perform 
        /// the check. You should only call this function when your app is initialized and 
        /// shows its main window.
        /// </summary>
        /// <param name="doInitialCheck"><c>true</c> if this instance should do an initial check.</param>
        /// <param name="forceInitialCheck"><c>true</c> if this instance should force an initial check.</param>
        /// <param name="checkFrequency">the frequency between checks.</param>
        public void StartLoop(Boolean doInitialCheck, Boolean forceInitialCheck, TimeSpan checkFrequency)
        {
            // first set the event handle
            _loopingHandle.Set();

            // Start the helper thread as a background worker to 
            // get well ui interaction                        

            // store infos
            _doInitialCheck = doInitialCheck;
            _forceInitialCheck = forceInitialCheck;
            _checkFrequency = checkFrequency;

            ReportDiagnosticMessage("Starting background worker");

            // start the work
            //var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            //_taskWorker.Start(scheduler);
            _taskWorker.Start();
            //_worker.RunWorkerAsync();
        }

        /// <summary>
        /// This method will stop the sparkle background loop and is called
        /// through the disposable interface automatically
        /// </summary>
        public void StopLoop()
        {
            // ensure the work will finished
            _exitHandle.Set();                       
        }

        /// <summary>
        /// Is called in the using context and will stop all background activities
        /// </summary>
        public void Dispose()
        {
            StopLoop();
            UnregisterEvents();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public WebResponse GetWebContentResponse(string Url)
        {
            WebRequest request = WebRequest.Create(Url);
            if (request != null)
            {
                if (request is FileWebRequest)
                {
                    FileWebRequest fileRequest = request as FileWebRequest;
                    if (fileRequest != null)
                    {
                        return request.GetResponse();
                    }
                }

                if (request is HttpWebRequest)
                {
                    HttpWebRequest httpRequest = request as HttpWebRequest;
                    httpRequest.UseDefaultCredentials = true;
                    httpRequest.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                    if (TrustEverySSLConnection)
                        httpRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                    // http://stackoverflow.com/a/10027534/3938401
                    if (ExtraJsonData != null && ExtraJsonData != "")
                    {
                        httpRequest.ContentType = "application/json";
                        httpRequest.Method = "POST";

                        using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                        {
                            streamWriter.Write(ExtraJsonData);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }
                    }

                    // request the cast and build the stream
                    return httpRequest.GetResponse();
                }
            }
            return null;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public Stream GetWebContentStream(string Url)
        {
            var response = GetWebContentResponse(Url);
            if (response != null)
            {
                var ms = new MemoryStream();
                response.GetResponseStream().CopyTo(ms);
                response.Close();
                ms.Position = 0;
                return ms;
            }
            return null;
        }

        /// <summary>
        /// Unregisters events so that we don't have multiple items updating
        /// </summary>
        private void UnregisterEvents()
        {
            ServicePointManager.ServerCertificateValidationCallback -= RemoteCertificateValidation;
            _cancelTokenSource.Cancel();
            /* _worker.DoWork -= OnWorkerDoWork;
             _worker.ProgressChanged -= OnWorkerProgressChanged;
             _worker = null; */

            if (_webDownloadClient != null)
            {
                if (ProgressWindow != null)
                {
                    _webDownloadClient.DownloadProgressChanged -= ProgressWindow.OnDownloadProgressChanged;
                }
                _webDownloadClient.DownloadFileCompleted -= OnDownloadFinished;
                _webDownloadClient = null;
            }

            if (UserWindow != null)
            {
                UserWindow.UserResponded -= OnUserWindowUserResponded;
                UserWindow = null;
            }

            if (ProgressWindow != null)
            {
                ProgressWindow.InstallAndRelaunch -= OnProgressWindowInstallAndRelaunch;
                ProgressWindow = null;
            }

        }

        /// <summary>
        /// This method updates the profile information which can be sended to the server if enabled    
        /// </summary>
        /// <param name="config">the configuration</param>
        public void UpdateSystemProfileInformation(NetSparkleConfiguration config)
        {
            // check if profile data is enabled
            if (!EnableSystemProfiling)
                return;

            // check if we need an update
            if (DateTime.Now - config.LastProfileUpdate < new TimeSpan(7, 0, 0, 0))
                return;

            // touch the profile update time
            config.TouchProfileTime();

            // start the profile thread
            Thread t = new Thread(ProfileDataThreadStart);
            t.Start(config);
        }

        /// <summary>
        /// Profile data thread
        /// </summary>
        /// <param name="obj">the configuration object</param>
        private void ProfileDataThreadStart(object obj)
        {
            try
            {
                if (SystemProfileUrl != null)
                {
                    // get the config
                    NetSparkleConfiguration config = obj as NetSparkleConfiguration;

                    // collect data
                    NetSparkleDeviceInventory inv = new NetSparkleDeviceInventory(config);
                    inv.CollectInventory();

                    // build url
                    string requestUrl = inv.BuildRequestUrl(SystemProfileUrl + "?");

                    // perform the webrequest
                    HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                    if (request != null)
                    {
                        //request.ServerCertificateValidationCallback += 
                        request.UseDefaultCredentials = true;
                        request.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

                        using (WebResponse response = request.GetResponse())
                        {
                            // close the response 
                            response.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // No exception during data send 
                ReportDiagnosticMessage(ex.Message);
            }
        }

        /// <summary>
        /// This method checks if an update is required. During this process the appcast
        /// will be downloaded and checked against the reference assembly. Ensure that
        /// the calling process has read access to the reference assembly.
        /// This method is also called from the background loops.
        /// </summary>
        /// <param name="config">the configuration</param>
        /// <returns>SparkleUpdate with information on whether there is an update available or not.</returns>
        public async Task<SparkleUpdateInfo> GetUpdateStatus(NetSparkleConfiguration config)
        {
            NetSparkleAppCastItem[] updates = null;
            // report
            ReportDiagnosticMessage("Downloading and checking appcast");

            // init the appcast
            NetSparkleAppCast cast = new NetSparkleAppCast(_appCastUrl, this, config);
            // check if any updates are available
            try
            {
                var task = Task.Factory.StartNew(() =>
                {
                    if (cast.Read())
                        updates = cast.GetUpdates();
                });
                await task;
            }
            catch (Exception e)
            {
                ReportDiagnosticMessage("Couldn't read/parse the app cast: " + e.Message);
                updates = null;
            }


            if (updates == null)
            {
                ReportDiagnosticMessage("No version information in app cast found");
                return new SparkleUpdateInfo(UpdateStatus.CouldNotDetermine);
            }

            // set the last check time
            ReportDiagnosticMessage("Touch the last check timestamp");
            config.TouchCheckTime();

            // check if the version will be the same then the installed version
            if (updates.Length == 0)
            {
                ReportDiagnosticMessage("Installed version is valid, no update needed (" + config.InstalledVersion + ")");
                return new SparkleUpdateInfo(UpdateStatus.UpdateNotAvailable);
            }
            ReportDiagnosticMessage("Latest version on the server is " + updates[0].Version);

            // check if the available update has to be skipped
            if (updates[0].Version.Equals(config.SkipThisVersion))
            {
                ReportDiagnosticMessage("Latest update has to be skipped (user decided to skip version " + config.SkipThisVersion + ")");
                return new SparkleUpdateInfo(UpdateStatus.UserSkipped);
            }

            // ok we need an update
            return new SparkleUpdateInfo(UpdateStatus.UpdateAvailable, updates);
        }

        /// <summary>
        /// This method reads the local sparkle configuration for the given
        /// reference assembly
        /// </summary>
        /// <returns>the configuration</returns>
        public NetSparkleConfiguration GetApplicationConfig()
        {
            if (Configuration == null)
            {
                Configuration = new NetSparkleRegistryConfiguration(_appReferenceAssembly);
            }
            Configuration.Reload();
            return Configuration;
        }

        /// <summary>
        /// Converts an relative url to an absolute url based on the 
        /// </summary>
        /// <param name="url">relative or absolute url</param>
        /// <returns>the absolute url</returns>
        public Uri GetAbsoluteUrl(string url)
        {
            return new Uri(new Uri(_appCastUrl), url);
        }

        /// <summary>
        /// This method shows the update ui and allows to perform the 
        /// update process
        /// </summary>
        /// <param name="updates">updates to show UI for</param>
        public void ShowUpdateNeededUI(NetSparkleAppCastItem[] updates)
        {
            if (_useNotificationToast)
            {
                UIFactory.ShowToast(updates, _applicationIcon, OnToastClick);
            }
            else
            {
                ShowUpdateNeededUIInner(updates);
            }
        }

        private void OnToastClick(NetSparkleAppCastItem[] updates)
        {
            ShowUpdateNeededUIInner(updates);
        }

        private void ShowUpdateNeededUIInner(NetSparkleAppCastItem[] updates)
        {
            // TODO: In the future, instead of remaking the window, just send the new data to the old window
            if (UserWindow != null)
            {
                // close old window
                if (UseSyncronizedForms)
                {
                    _syncContext.Send((state) =>
                    {
                        UserWindow.Close();
                    }, null);
                }
                else
                {
                    UserWindow.Close();
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
                        UserWindow = UIFactory.CreateSparkleForm(this, updates, _applicationIcon);

                        if (HideReleaseNotes)
                        {
                            UserWindow.HideReleaseNotes();
                        }

                        // clear if already set.
                        UserWindow.UserResponded += OnUserWindowUserResponded;
                        UserWindow.Show();
                    };

                    // call action
                    if (UseSyncronizedForms)
                    {
                        _syncContext.Send((state) => showSparkleUI(state), null);
                    }
                    else
                    {
                        showSparkleUI(null);
                    }
                }  
                catch (Exception e)
                {
                    ReportDiagnosticMessage("Error showing sparkle form: " + e.Message);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// This method reports a message in the diagnostic window
        /// </summary>
        /// <param name="message"></param>
        public void ReportDiagnosticMessage(string message)
        {
            if (!PrintDiagnosticToConsole)
                Debug.WriteLine("netsparkle: " + message);
            else
                Console.WriteLine("netsparkle: " + message);
        }

        /// <summary>
        /// Starts the download process
        /// </summary>
        /// <param name="item">the appcast item to download</param>
        private void InitDownloadAndInstallProcess(NetSparkleAppCastItem item)
        {
            ReportDiagnosticMessage("Preparing to download " + item.DownloadLink);
            _itemBeingDownloaded = item;
            // get the filename of the download link
            string[] segments = item.DownloadLink.Split('/');
            string fileName = segments[segments.Length - 1];

            // get temp path
            TmpDownloadFilePath = TmpDownloadFilePath.Trim();
            bool isTmpDownloadFilePathSet = TmpDownloadFilePath != null && TmpDownloadFilePath != "";
            string tmpPath = isTmpDownloadFilePathSet ? TmpDownloadFilePath : Path.GetTempPath();
            if (isTmpDownloadFilePathSet && !File.Exists(tmpPath))
            {
                Directory.CreateDirectory(tmpPath);
            }
            _downloadTempFileName = Path.Combine(tmpPath, fileName);
            // Make sure the file doesn't already exist on disk. If it's already downloaded and the
            // DSA signature checks out, don't redownload the file!
            bool needsToDownload = true;
            if (File.Exists(_downloadTempFileName))
            {
                ValidationResult result = DSAVerificator.VerifyDSASignatureFile(item.DownloadDSASignature, _downloadTempFileName);
                if (result == ValidationResult.Valid || result == ValidationResult.Unchecked)
                {
                    ReportDiagnosticMessage("File is already downloaded");
                    // We already have the file! Don't redownload it!
                    needsToDownload = false;
                    // Still need to set up the ProgressWindow for non-silent downloads, though,
                    // so that the user can actually perform the install
                    initializeProgressWindow(item);
                    ProgressWindow?.FinishedDownloadingFile(true);
                    OnDownloadFinished(null, new AsyncCompletedEventArgs(null, false, null));
                    showProgressWindow(); // opens as a dialog, hence why we call OnDownloadFinished before showing the window
                }
                else if (!_hasAttemptedFileRedownload)
                {
                    // File is downloaded, but is corrupt or was stopped in the middle or something else happened.
                    // Redownload it!
                    _hasAttemptedFileRedownload = true;
                    ReportDiagnosticMessage("File is corrupt; deleting file and redownloading...");
                    File.Delete(_downloadTempFileName);
                }
                else
                {
                    DownloadedFileIsCorrupt?.Invoke(item, _downloadTempFileName);
                }
            }
            if (needsToDownload)
            {
                initializeProgressWindow(item);

                if (_webDownloadClient != null)
                {
                    if (ProgressWindow != null)
                    {
                        _webDownloadClient.DownloadProgressChanged -= ProgressWindow.OnDownloadProgressChanged;
                    }
                    _webDownloadClient.DownloadFileCompleted -= OnDownloadFinished;
                    _webDownloadClient = null;
                }

                _webDownloadClient = new WebClient
                {
                    UseDefaultCredentials = true,
                    Proxy = { Credentials = CredentialCache.DefaultNetworkCredentials },
                };
                if (ProgressWindow != null)
                {
                    _webDownloadClient.DownloadProgressChanged += ProgressWindow.OnDownloadProgressChanged;
                }
                _webDownloadClient.DownloadFileCompleted += OnDownloadFinished;

                Uri url = new Uri(item.DownloadLink);
                ReportDiagnosticMessage("Starting to download " + url + " to " + _downloadTempFileName);
                _webDownloadClient.DownloadFileAsync(url, _downloadTempFileName);
                showProgressWindow();
            }
        }

        private void initializeProgressWindow(NetSparkleAppCastItem castItem)
        {
            if (ProgressWindow != null)
            {
                ProgressWindow.InstallAndRelaunch -= OnProgressWindowInstallAndRelaunch;
                ProgressWindow = null;
            }
            if (ProgressWindow == null && !isDownloadingSilently())
            {
                ProgressWindow = UIFactory.CreateProgressWindow(castItem, _applicationIcon);
                ProgressWindow.InstallAndRelaunch += OnProgressWindowInstallAndRelaunch;
            }
        }

        /// <summary>
        /// Shows the progress window if not downloading silently.
        /// </summary>
        private void showProgressWindow()
        {
            if (!isDownloadingSilently() && ProgressWindow != null)
            {
                DialogResult result = ProgressWindow.ShowDialog();
                if (result == DialogResult.Abort || result == DialogResult.Cancel)
                {
                    CancelFileDownload();
                }
            }
        }

        /// <summary>
        /// True if the user has silent updates enabled; false otherwise.
        /// </summary>
        /// <returns></returns>
        private bool isDownloadingSilently()
        {
            return EnableSilentMode || SilentMode != SilentModeTypes.NotSilent;
        }

        /// <summary>
        /// Return installer runner command. May throw InvalidDataException
        /// </summary>
        /// <param name="downloadFileName"></param>
        /// <returns></returns>
        protected virtual string GetInstallerCommand(string downloadFileName)
        {
            // get the file type
            string installerExt = Path.GetExtension(downloadFileName);
            if (".exe".Equals(installerExt, StringComparison.CurrentCultureIgnoreCase))
            {
                // build the command line 
                return "\"" + downloadFileName + "\"";
            }
            if (".msi".Equals(installerExt, StringComparison.CurrentCultureIgnoreCase))
            {
                // buid the command line
                return "msiexec /i \"" + downloadFileName + "\"";
            }
            if (".msp".Equals(installerExt, StringComparison.CurrentCultureIgnoreCase))
            {
                // build the command line
                return "msiexec /p \"" + downloadFileName + "\"";
            }

            throw new InvalidDataException("Unknown installer format");
        }

        /// <summary>
        /// Runs the downloaded installer
        /// </summary>
        private async Task RunDownloadedInstaller()
        {
            // get the commandline 
            string cmdLine = Environment.CommandLine;
            string workingDir = Environment.CurrentDirectory;

            // generate the batch file path
            string cmd = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cmd");
            string installerCmd;
            try
            {
                installerCmd = GetInstallerCommand(_downloadTempFileName);

                if (!String.IsNullOrEmpty(CustomInstallerArguments))
                    installerCmd += " " + CustomInstallerArguments;
            }
            catch (InvalidDataException)
            {
                UIFactory.ShowUnknownInstallerFormatMessage(_downloadTempFileName, _applicationIcon);
                return;
            }

            // generate the batch file                
            ReportDiagnosticMessage("Generating batch in " + Path.GetFullPath(cmd));

            using (StreamWriter write = new StreamWriter(cmd))
            {
                write.WriteLine("@echo off");
                // We should wait until the host process has died before starting the installer.
                // This way, any DLLs or other items can be replaced properly.
                // Code from: http://stackoverflow.com/a/22559462/3938401
                int processID = Process.GetCurrentProcess().Id;
                write.WriteLine(":loop");
                write.WriteLine("tasklist | find \"" + processID.ToString() + "\" > nul");
                write.WriteLine("if not errorlevel 1 (");
                write.WriteLine("ECHO Waiting for application to close...");
                write.WriteLine("timeout /t 1 >nul");
                write.WriteLine("goto :loop");
                write.WriteLine(")");
                write.WriteLine(installerCmd);

                if (RelaunchAfterUpdate)
                {
                    write.WriteLine("cd " + workingDir);
                    write.WriteLine(cmdLine);
                }
                write.Close();
            }

            // report
            ReportDiagnosticMessage("Going to execute batch: " + cmd);
            
            // init the installer helper
            _installerProcess = new Process
                {
                    StartInfo =
                        {
                            FileName = cmd, 
                            WindowStyle = ProcessWindowStyle.Hidden
                        }
                };

            // listen for application exit events
            Application.ApplicationExit += OnWindowsFormsApplicationExit;
            // quit the app
            if (_exitHandle != null)
                _exitHandle.Set(); // make SURE the loop exits!
            if (RunningFromWPF == true)
            {
                // In case the user has shut the window that started this Sparkle window/instance, don't crash and burn.
                // If you have better ideas on how to figure out if they've shut all other windows, let me know...
                try
                {
                    if (CloseWPFWindowAsync != null)
                    {
                        await CloseWPFWindowAsync.Invoke();
                    }
                    else if (CloseWPFWindow != null)
                    {
                        CloseWPFWindow.Invoke();
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() => {
                            System.Windows.Application.Current.Shutdown();
                        });
                    }
                }
                catch (Exception e)
                {
                    ReportDiagnosticMessage(e.Message);
                }
            }
            _installerProcess.Start();
            Application.Exit();
        }

        /// <summary>
        /// Apps may need, for example, to let user save their work
        /// </summary>
        /// <returns>true if it's ok</returns>
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
                ReportDiagnosticMessage(e.Message);
            }
            return true;
        }

        /// <summary>
        /// Determine if the remote X509 certificate is validate
        /// </summary>
        /// <param name="sender">the web request</param>
        /// <param name="certificate">the certificate</param>
        /// <param name="chain">the chain</param>
        /// <param name="sslPolicyErrors">how to handle policy errors</param>
        /// <returns><c>true</c> if the cert is valid</returns>
        private bool RemoteCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            X509Certificate2 cert2 = certificate as X509Certificate2;
            if (TrustEverySSLConnection)
            {
                // verify if we talk about our app cast dll 
                HttpWebRequest req = sender as HttpWebRequest;
                if (req != null && req.RequestUri.Equals(new Uri(_appCastUrl)))
                    return true;
            }

            // check our cert                 
            return sslPolicyErrors == SslPolicyErrors.None && cert2 != null && cert2.Verify();
        }

        /// <summary>
        /// Check for updates, using interaction appropriate for if the user just said "check for updates".
        /// </summary>
        public async Task<SparkleUpdateInfo> CheckForUpdatesAtUserRequest()
        {
            Cursor.Current = Cursors.WaitCursor;
            CheckingForUpdatesWindow = new CheckingForUpdatesWindow(_applicationIcon);
            CheckingForUpdatesWindow.FormClosed += CheckingForUpdatesWindow_FormClosed; // to detect canceling -- TODO: there's probably a better way...
            CheckingForUpdatesWindow.Show();
            // TODO: in the future, instead of pseudo-canceling the request and only making it appear as though it was canceled, 
            // actually cancel the request using a BackgroundWorker or something
            SparkleUpdateInfo updateData = await CheckForUpdates(false /* toast not appropriate, since they just requested it */);
            if (CheckingForUpdatesWindow != null) // if null, user closed 'Checking for Updates...' window
            {
                CheckingForUpdatesWindow?.Close();
                UpdateStatus updateAvailable = updateData.Status;
                Cursor.Current = Cursors.Default;

                Action<object> UIAction = (state) =>
                {
                    switch (updateAvailable)
                    {
                        case UpdateStatus.UpdateAvailable:
                            if (_useNotificationToast)
                                UIFactory.ShowToast(updateData.Updates, _applicationIcon, OnToastClick);
                            break;
                        case UpdateStatus.UpdateNotAvailable:
                            UIFactory.ShowVersionIsUpToDate(_applicationIcon);
                            break;
                        case UpdateStatus.UserSkipped:
                            UIFactory.ShowVersionIsSkippedByUserRequest(_applicationIcon); // TODO: pass skipped version number
                            break;
                        case UpdateStatus.CouldNotDetermine:
                            UIFactory.ShowCannotDownloadAppcast(_appCastUrl, _applicationIcon);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                };

                if (UseSyncronizedForms)
                {
                    _syncContext.Send((state) => UIAction(state), null);
                }
                else
                {
                    UIAction(null);
                }
            }
            else
            {
                return null;
            }

            return updateData;// in this case, we've already shown UI talking about the new version
        }

        private void CheckingForUpdatesWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            CheckingForUpdatesWindow = null;
        }

        /// <summary>
        /// Check for updates, using interaction appropriate for where the user doesn't know you're doing it, so be polite
        /// </summary>
        public async Task<SparkleUpdateInfo> CheckForUpdatesQuietly()
        {
            SparkleUpdateInfo updateData = await CheckForUpdates(true);
            return updateData;
        }

        /// <summary>
        /// Does a one-off check for updates
        /// </summary>
        /// <param name="useNotificationToast">set false if you want the big dialog to open up, without the user having the chance to ignore the popup toast notification</param>
        private async Task<SparkleUpdateInfo> CheckForUpdates(bool useNotificationToast)
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
            NetSparkleConfiguration config = GetApplicationConfig();
            // update profile information is needed
            UpdateSystemProfileInformation(config);

            // check if update is required
            SparkleUpdateInfo updateStatus = await GetUpdateStatus(config);
            NetSparkleAppCastItem[] updates = updateStatus.Updates;
            if (updateStatus.Status == UpdateStatus.UpdateAvailable)
            {
                // show the update window
                ReportDiagnosticMessage("Update needed from version " + config.InstalledVersion + " to version " +
                                        updates[0].Version);

                UpdateDetectedEventArgs ev = new UpdateDetectedEventArgs
                                                    {
                                                        NextAction = NextUpdateAction.ShowStandardUserInterface,
                                                        ApplicationConfig = config,
                                                        LatestVersion = updates[0]
                                                    };

                // if the client wants to intercept, send an event
                if (UpdateDetected != null)
                {
                    UpdateDetected(this, ev);
                    // if the client wants the default UI then show them
                    switch (ev.NextAction)
                    {
                        case NextUpdateAction.ShowStandardUserInterface:
                            ReportDiagnosticMessage("Showing Standard Update UI");
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
            UpdateCheckFinished?.Invoke(this, updateStatus.Status);
            return updateStatus;
        }

        /// <summary>
        /// Updates from appcast
        /// </summary>
        /// <param name="updates">updates to be installed</param>
        private void Update(NetSparkleAppCastItem[] updates)
        {
            if (updates == null)
                return;

            if (isDownloadingSilently())
            {
                InitDownloadAndInstallProcess(updates[0]); // install only latest
            }
            else
            {
                // show the update ui
                ShowUpdateNeededUI(updates);
            }
        }

        /// <summary>
        /// Cancels the install
        /// </summary>
        public void CancelFileDownload()
        {
            ReportDiagnosticMessage("Canceling download...");
            if (_webDownloadClient != null && _webDownloadClient.IsBusy)
            {
                _webDownloadClient.CancelAsync();
            }
        }

        /// <summary>
        /// Called when the user responds to the "skip, later, install" question.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private void OnUserWindowUserResponded(object sender, EventArgs e)
        {
            if (UserWindow.Result == DialogResult.No)
            {
                // skip this version
                NetSparkleConfiguration config = GetApplicationConfig();
                config.SetVersionToSkip(UserWindow.CurrentItem.Version);
            }
            else if (UserWindow.Result == DialogResult.Yes)
            {
                // download the binaries
                InitDownloadAndInstallProcess(UserWindow.CurrentItem);
            }
            UserWindow = null; // done using the window so don't hold onto reference
            CheckingForUpdatesWindow?.Close();
            CheckingForUpdatesWindow = null;
        }

        /// <summary>
        /// Called when the progress bar fires the update event
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private async void OnProgressWindowInstallAndRelaunch(object sender, EventArgs e)
        {
            ProgressWindow?.SetDownloadAndInstallButtonEnabled(false); // disable while we ask if we can close up the software
            if (await AskApplicationToSafelyCloseUp())
            {
                await RunDownloadedInstaller();
            }
            else
            {
                ProgressWindow?.SetDownloadAndInstallButtonEnabled(true);
            }
        }

        /// <summary>
        /// This method will be executed as worker thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    break;
                // set state
                bool bUpdateRequired = false;

                // notify
                CheckLoopStarted?.Invoke(this);

                // report status
                if (doInitialCheck == false)
                {
                    ReportDiagnosticMessage("Initial check prohibited, going to wait");
                    doInitialCheck = true;
                    goto WaitSection;
                }

                // report status
                ReportDiagnosticMessage("Starting update loop...");

                // read the config
                ReportDiagnosticMessage("Reading config...");
                NetSparkleConfiguration config = GetApplicationConfig();

                // calc CheckTasp
                bool checkTSPInternal = checkTSP;

                if (isInitialCheck && checkTSPInternal)
                    checkTSPInternal = !_forceInitialCheck;

                // check if it's ok the recheck to software state
                if (checkTSPInternal)
                {
                    TimeSpan csp = DateTime.Now - config.LastCheckTime;
                    if (csp < _checkFrequency)
                    {
                        ReportDiagnosticMessage(String.Format("Update check performed within the last {0} minutes!", _checkFrequency.TotalMinutes));
                        goto WaitSection;
                    }
                }
                else
                    checkTSP = true;

                // when sparkle will be deactivated wait an other cycle
                if (config.CheckForUpdate == false)
                {
                    ReportDiagnosticMessage("Check for updates disabled");
                    goto WaitSection;
                }

                // update the runonce feature
                goIntoLoop = !config.DidRunOnce;

                // update profile information is needed
                UpdateSystemProfileInformation(config);

                // check if update is required
                if (_cancelToken.IsCancellationRequested)
                    break;
                SparkleUpdateInfo updateStatus = await GetUpdateStatus(config);
                if (_cancelToken.IsCancellationRequested)
                    break;
                NetSparkleAppCastItem[] updates = updateStatus.Updates;
                bUpdateRequired = updateStatus.Status == UpdateStatus.UpdateAvailable;
                if (bUpdateRequired)
                {
                    // show the update window
                    ReportDiagnosticMessage("Update needed from version " + config.InstalledVersion + " to version " + updates[0].Version);

                    // send notification if needed
                    UpdateDetectedEventArgs ev = new UpdateDetectedEventArgs { NextAction = NextUpdateAction.ShowStandardUserInterface, ApplicationConfig = config, LatestVersion = updates[0] };
                    UpdateDetected?.Invoke(this, ev);

                    // check results
                    switch (ev.NextAction)
                    {
                        case NextUpdateAction.PerformUpdateUnattended:
                            {
                                ReportDiagnosticMessage("Unattended update desired from consumer");
                                EnableSilentMode = true;
                                SilentMode = SilentModeTypes.DownloadAndInstall;
                                OnWorkerProgressChanged(_taskWorker, new ProgressChangedEventArgs(1, updates));
                                //_worker.ReportProgress(1, updates);
                                break;
                            }
                        case NextUpdateAction.ProhibitUpdate:
                            {
                                ReportDiagnosticMessage("Update prohibited from consumer");
                                break;
                            }
                        default:
                            {
                                ReportDiagnosticMessage("Showing Standard Update UI");
                                OnWorkerProgressChanged(_taskWorker, new ProgressChangedEventArgs(1, updates));
                                //_worker.ReportProgress(1, updates);
                                break;
                            }
                    }
                }

            WaitSection:
                // reset initialcheck
                isInitialCheck = false;

                // notify
                CheckLoopFinished?.Invoke(this, bUpdateRequired);

                // report wait statement
                ReportDiagnosticMessage(String.Format("Sleeping for an other {0} minutes, exit event or force update check event", _checkFrequency.TotalMinutes));

                // wait for
                if (!goIntoLoop || _cancelToken.IsCancellationRequested)
                    break;

                // build the event array
                WaitHandle[] handles = new WaitHandle[1];
                handles[0] = _exitHandle;

                // wait for any
                if (_cancelToken.IsCancellationRequested)
                    break;
                int i = WaitHandle.WaitAny(handles, _checkFrequency);
                if (_cancelToken.IsCancellationRequested)
                    break;
                if (WaitHandle.WaitTimeout == i)
                {
                    ReportDiagnosticMessage(String.Format("{0} minutes are over", _checkFrequency.TotalMinutes));
                    continue;
                }

                // check the exit handle
                if (i == 0)
                {
                    ReportDiagnosticMessage("Got exit signal");
                    break;
                }

                // check an other check needed
                if (i == 1)
                {
                    ReportDiagnosticMessage("Got force update check signal");
                    checkTSP = false;
                }
                if (_cancelToken.IsCancellationRequested)
                    break;
            } while (goIntoLoop);

            // reset the islooping handle
            _loopingHandle.Reset();
        }

        /// <summary>
        /// This method will be notified
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 1:
                    Update(e.UserState as NetSparkleAppCastItem[]);
                    break;
                case 0:
                    ReportDiagnosticMessage(e.UserState.ToString());
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
            // TODO: more delegate messages (especially for silent mode operations)
            bool shouldShowUIItems = !isDownloadingSilently();
            if (e.Error != null)
            {
                ReportDiagnosticMessage("Error on download finished: " + e.Error.Message);
                if (shouldShowUIItems && ProgressWindow != null && !ProgressWindow.DisplayErrorMessage(e.Error.Message))
                {
                    UIFactory.ShowDownloadErrorMessage(e.Error.Message, _appCastUrl, _applicationIcon);
                }
                return;
            }

            if (e.Cancelled)
            {
                ReportDiagnosticMessage("Download was canceled");
                string errorMessage = "Download cancelled";
                if (shouldShowUIItems && ProgressWindow != null && !ProgressWindow.DisplayErrorMessage(errorMessage))
                {
                    UIFactory.ShowDownloadErrorMessage(errorMessage, _appCastUrl, _applicationIcon);
                }
                return;
            }

            // test the item for DSA signature
            var validationRes = ValidationResult.Invalid;
            if (!e.Cancelled && e.Error == null)
            {
                ReportDiagnosticMessage("Fully downloaded file exists at " + _downloadTempFileName);

                ReportDiagnosticMessage("Performing DSA check");

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
                    string dsaSignature = _itemBeingDownloaded?.DownloadDSASignature;
                    if (dsaSignature != null)
                    {
                        validationRes = DSAVerificator.VerifyDSASignatureFile(dsaSignature, _downloadTempFileName);
                    }
                }
            }

            bool isSignatureInvalid = validationRes == ValidationResult.Invalid;
            if (shouldShowUIItems)
            {
                ProgressWindow?.FinishedDownloadingFile(!isSignatureInvalid);
            }
            // signature of file isn't valid so exit with error
            if (isSignatureInvalid)
            {
                ReportDiagnosticMessage("Invalid signature for downloaded file for app cast: " + _downloadTempFileName);
                string errorMessage = "Downloaded file has invalid signature!";
                DownloadedFileIsCorrupt?.Invoke(_itemBeingDownloaded, _downloadTempFileName);
                // Default to showing errors in the progress window. Only go to the UIFactory to show errors if necessary.
                if (shouldShowUIItems && ProgressWindow != null && !ProgressWindow.DisplayErrorMessage(errorMessage))
                {
                    UIFactory.ShowDownloadErrorMessage(errorMessage, _appCastUrl, _applicationIcon);
                }
                // Let the progress window handle closing itself.
            }
            else
            {
                ReportDiagnosticMessage("DSA Signature is valid. File successfully downloaded!");
                DownloadedFileReady?.Invoke(_itemBeingDownloaded, _downloadTempFileName);
                bool shouldInstallAndRelaunch = EnableSilentMode || SilentMode == SilentModeTypes.DownloadAndInstall;
                if (shouldInstallAndRelaunch)
                {
                    OnProgressWindowInstallAndRelaunch(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Called when a Windows forms application exits. Starts the installer.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private void OnWindowsFormsApplicationExit(object sender, EventArgs e)
        {
            Application.ApplicationExit -= OnWindowsFormsApplicationExit;
            if (_installerProcess != null)
            {
                _installerProcess.Start();
            }
        }
     
    }
}
