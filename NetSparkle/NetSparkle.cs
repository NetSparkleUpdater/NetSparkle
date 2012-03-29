using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;
using AppLimit.NetSparkle.Interfaces;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace AppLimit.NetSparkle
{
    /// <summary>
    /// The operation has started
    /// </summary>
    /// <param name="sender">the sender</param>
    public delegate void LoopStartedOperation(Object sender);
    /// <summary>
    /// The operation has ended
    /// </summary>
    /// <param name="sender">the sender</param>
    /// <param name="UpdateRequired"><c>true</c> if an update is required</param>
    public delegate void LoopFinishedOperation(Object sender, Boolean UpdateRequired);

    /// <summary>
    /// This delegate will be used when an update was detected to allow library 
    /// consumer to add own user interface capabilities.    
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void UpdateDetected(Object sender, UpdateDetectedEventArgs e);

    /// <summary>
    /// Class to communicate with a sparkle-based appcast
    /// </summary>
    public class Sparkle : IDisposable
    {
        /// <summary>
        /// Event to fire if this instance needs to exit the appliction.
        /// </summary>
        public event EventHandler ApplicationExitRequested;

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

        private BackgroundWorker _worker = new BackgroundWorker();
        private String _AppCastUrl;
        private String _AppReferenceAssembly;

        private Boolean _DoInitialCheck;
        private Boolean _ForceInitialCheck;

        private EventWaitHandle _exitHandle;
        private EventWaitHandle _loopingHandle;
       
        private TimeSpan _CheckFrequency;

        private string _downloadTempFileName;
        private WebClient _webDownloadClient;
        private NetSparkleDiagnostic _diagnostic;

        /// <summary>
        /// ctor which needs the appcast url
        /// </summary>
        /// <param name="appcastUrl">the URL for the appcast file</param>
        public Sparkle(String appcastUrl)
            : this(appcastUrl, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url and a referenceassembly
        /// </summary>
        /// <param name="appcastUrl">the URL for the appcast file</param>
        /// <param name="referenceAssembly">the reference assembly</param>
        public Sparkle(String appcastUrl, String referenceAssembly)
            : this(appcastUrl, referenceAssembly, false)
        { }

        /// <summary>
        /// ctor which needs the appcast url and a referenceassembly
        /// </summary>        
        /// <param name="appcastUrl">the URL for the appcast file</param>
        /// <param name="referenceAssembly">the reference assembly</param>
        /// <param name="showDiagnostic">show the diagnostics window.</param>
        public Sparkle(String appcastUrl, String referenceAssembly, bool showDiagnostic)
        {
            // preconfige ssl trust
            TrustEverySSLConnection = false;

            // configure ssl cert link
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidation;

            // enable visual style to ensure that we have XP style or higher
            // also in WPF applications
            System.Windows.Forms.Application.EnableVisualStyles();

            // reset vars
            ApplicationIcon = null;
            _AppReferenceAssembly = null;            

            // set var
            this.ShowDiagnosticWindow = showDiagnostic;
            _diagnostic = new NetSparkleDiagnostic(showDiagnostic);

            // set the reference assembly
            if (referenceAssembly != null)
            {
                _AppReferenceAssembly = referenceAssembly;
                _diagnostic.Report("Checking the following file: " + _AppReferenceAssembly);
            }

            // show if needed
            ShowDiagnosticWindowIfNeeded();            

            // adjust the delegates
            _worker.WorkerReportsProgress = true;
            _worker.DoWork += new DoWorkEventHandler(OnWorkerDoWork);
            _worker.ProgressChanged += new ProgressChangedEventHandler(OnWorkerProgressChanged);

            // build the wait handle
            _exitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            _loopingHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            // set the url
            _AppCastUrl = appcastUrl;
            _diagnostic.Report("Using the following url: " + _AppCastUrl);            
        }

        #region Properties
        /// <summary>
        /// Enables system profiling against a profile server
        /// </summary>
        public Boolean EnableSystemProfiling { get; private set; }

        /// <summary>
        /// Hides the release notes view when an update was found. This 
        /// mode is switched on automatically when no sparkle:releaseNotesLink
        /// tag was found in the app cast         
        /// </summary>
        public Boolean HideReleaseNotes { get; private set; }

        /// <summary>
        /// Contains the profile url for System profiling
        /// </summary>
        public Uri SystemProfileUrl { get; private set; }

        /// <summary>
        /// This property holds an optional application icon
        /// which will be displayed in the software update dialog. The icon has
        /// to be 48x48 pixels.
        /// </summary>
        public Image ApplicationIcon { get; set; }

        /// <summary>
        /// This property returns an optional application icon 
        /// which will displayed in the windows as self
        /// </summary>
        public Icon ApplicationWindowIcon { get; set; }

        /// <summary>
        /// This property enables a diagnostic window for debug reasons
        /// </summary>
        public bool ShowDiagnosticWindow { get; set; }

        /// <summary>
        /// This property enables the silent mode, this means 
        /// the application will be updated without user interaction
        /// </summary>
        public bool EnableSilentMode { get; set; }

        /// <summary>
        /// This property returns true when the upadete loop is running
        /// and files when the loop is not running
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
        /// The configuration.
        /// </summary>
        public NetSparkleConfiguration Configuration { get; set; }
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

            // show if needed
            ShowDiagnosticWindowIfNeeded();

            // store infos
            _DoInitialCheck = doInitialCheck;
            _ForceInitialCheck = forceInitialCheck;
            _CheckFrequency = checkFrequency;

            // create and configure the worker
            _diagnostic.Report("Starting background worker");

            // start the work
            _worker.RunWorkerAsync();
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
        /// Unregisters events so that we don't have multiple items updating
        /// </summary>
        private void UnregisterEvents()
        {
            if ( this.ProgressWindow != null )
            {
                this.ProgressWindow.InstallAndRelaunch -= new EventHandler(OnProgressWindowInstallAndRelaunch);
            }

            if (_webDownloadClient != null)
            {
                _webDownloadClient.DownloadProgressChanged -= new DownloadProgressChangedEventHandler(this.ProgressWindow.OnClientDownloadProgressChanged);
                _webDownloadClient.DownloadFileCompleted -= new AsyncCompletedEventHandler(OnWebDownloadClientDownloadFileCompleted);
                _webDownloadClient = null;
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
                    String requestUrl = inv.BuildRequestUrl(SystemProfileUrl.ToString() + "?");

                    // perform the webrequest
                    HttpWebRequest request = HttpWebRequest.Create(requestUrl) as HttpWebRequest;
                    if (request != null)
                    {
                        request.UseDefaultCredentials = true;
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
        /// the calling process has access to the internet and read access to the 
        /// reference assembly. This method is also called from the background loops.
        /// </summary>
        /// <param name="config">the configuration</param>
        /// <param name="latestVersion">returns the latest version</param>
        /// <returns><c>true</c> if an update is required</returns>
        public bool IsUpdateRequired(NetSparkleConfiguration config, out NetSparkleAppCastItem latestVersion)
        {
            // report
            ReportDiagnosticMessage("Downloading and checking appcast");

            // init the appcast
            NetSparkleAppCast cast = new NetSparkleAppCast(_AppCastUrl, config);

            // check if any updates are available
            try
            {
                latestVersion = cast.GetLatestVersion();
            }
            catch (Exception e)
            {
                // show the exeception message 
                ReportDiagnosticMessage("Error during app cast download: " + e.Message);

                // just null the version info
                latestVersion = null;
            }

            if (latestVersion == null)
            {
                ReportDiagnosticMessage("No version information in app cast found");
                return false;
            }
            else
            {
                ReportDiagnosticMessage("Lastest version on the server is " + latestVersion.Version);
            }

            // set the last check time
            ReportDiagnosticMessage("Touch the last check timestamp");
            config.TouchCheckTime();

            // check if the available update has to be skipped
            if (latestVersion.Version.Equals(config.SkipThisVersion))
            {
                ReportDiagnosticMessage("Latest update has to be skipped (user decided to skip version " + config.SkipThisVersion + ")");
                return false;
            }

            // check if the version will be the same then the installed version
            Version v1 = new Version(config.InstalledVersion);
            Version v2 = new Version(latestVersion.Version);

            if (v2 <= v1)
            {
                ReportDiagnosticMessage("Installed version is valid, no update needed (" + config.InstalledVersion + ")");
                return false;
            }

            // ok we need an update
            return true;
        }

        /// <summary>
        /// This method reads the local sparkle configuration for the given
        /// reference assembly
        /// </summary>
        /// <returns>the configuration</returns>
        public NetSparkleConfiguration GetApplicationConfig()
        {
            if (this.Configuration == null)
            {
                this.Configuration = new NetSparkleRegistryConfiguration(_AppReferenceAssembly);
            }
            this.Configuration.Reload();
            return this.Configuration;
        }

        /// <summary>
        /// This method shows the update ui and allows to perform the 
        /// update process
        /// </summary>
        /// <param name="currentItem">the item to show the UI for</param>
        public void ShowUpdateNeededUI(NetSparkleAppCastItem currentItem)
        {
            if (this.UserWindow == null)
            {
                // create the form
                this.UserWindow = new NetSparkleForm(currentItem, ApplicationIcon, ApplicationWindowIcon);
            }
            this.UserWindow.CurrentItem = currentItem;
            if (this.HideReleaseNotes)
            {
                this.UserWindow.HideReleaseNotes();
            }
            // clear if already set.
            this.UserWindow.UserResponded -= new EventHandler(OnUserWindowUserResponded);
            this.UserWindow.UserResponded += new EventHandler(OnUserWindowUserResponded);
            this.UserWindow.Show();
        }

        /// <summary>
        /// This method reports a message in the diagnostic window
        /// </summary>
        /// <param name="message"></param>
        public void ReportDiagnosticMessage(String message)
        {
            _diagnostic.Report(message);
        }

        /// <summary>
        /// Starts the download process
        /// </summary>
        /// <param name="item">the appcast item to download</param>
        private void InitDownloadAndInstallProcess(NetSparkleAppCastItem item)
        {
            // get the filename of the download lin
            string[] segments = item.DownloadLink.Split('/');
            string fileName = segments[segments.Length - 1];

            // get temp path
            _downloadTempFileName = Environment.ExpandEnvironmentVariables("%temp%\\" + fileName);
            if (this.ProgressWindow == null)
            {
                this.ProgressWindow = new NetSparkleDownloadProgress(this, item, ApplicationIcon, ApplicationWindowIcon, EnableSilentMode);
            }
            else
            {
                this.ProgressWindow.InstallAndRelaunch -= new EventHandler(OnProgressWindowInstallAndRelaunch);
            }

            this.ProgressWindow.TempFileName = _downloadTempFileName;
            this.ProgressWindow.InstallAndRelaunch += new EventHandler(OnProgressWindowInstallAndRelaunch);
            
            // set up the download client
            // start async download
            if (_webDownloadClient != null)
            {
                _webDownloadClient.DownloadProgressChanged -= new DownloadProgressChangedEventHandler(this.ProgressWindow.OnClientDownloadProgressChanged);
                _webDownloadClient.DownloadFileCompleted -= new AsyncCompletedEventHandler(OnWebDownloadClientDownloadFileCompleted);
                _webDownloadClient = null;
            }

            _webDownloadClient = new WebClient();
            _webDownloadClient.UseDefaultCredentials = true;
            _webDownloadClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.ProgressWindow.OnClientDownloadProgressChanged);
            _webDownloadClient.DownloadFileCompleted += new AsyncCompletedEventHandler(OnWebDownloadClientDownloadFileCompleted);

            Uri url = new Uri(item.DownloadLink);
            _webDownloadClient.DownloadFileAsync(url, _downloadTempFileName);

            this.ProgressWindow.ShowDialog();
        }

        /// <summary>
        /// Runs the downloaded installer
        /// </summary>
        private void RunDownloadedInstaller()
        {
            // get the commandline 
            string cmdLine = Environment.CommandLine;
            string workingDir = Environment.CurrentDirectory;

            // generate the batch file path
            string cmd = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cmd");
            string installerCMD;

            // get the file type
            if (Path.GetExtension(_downloadTempFileName).Equals(".exe", StringComparison.CurrentCultureIgnoreCase))
            {
                // build the command line 
                installerCMD = _downloadTempFileName;
            }
            else if (Path.GetExtension(_downloadTempFileName).Equals(".msi", StringComparison.CurrentCultureIgnoreCase))
            {
                // buid the command line
                installerCMD = "msiexec /i \"" + _downloadTempFileName + "\"";
            }
            else
            {
                MessageBox.Show("Updater not supported, please execute " + _downloadTempFileName + " manually", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Exit(-1);
                return;
            }

            // generate the batch file                
            ReportDiagnosticMessage("Generating MSI batch in " + Path.GetFullPath(cmd));

            using (StreamWriter write = new StreamWriter(cmd))
            {
                write.WriteLine(installerCMD);
                write.WriteLine("cd " + workingDir);
                write.WriteLine(cmdLine);
                write.Close();
            }

            // report
            ReportDiagnosticMessage("Going to execute batch: " + cmd);

            // start the installer helper
            Process process = new Process();
            process.StartInfo.FileName = cmd;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();

            // quit the app
            Exit(0);
        }

        /// <summary>
        /// Attempts to exit the application
        /// </summary>
        /// <param name="returnCode">the return code.</param>
        private void Exit( int returnCode)
        {
            if (ApplicationExitRequested != null)
            {
                ApplicationExitRequested(this, new EventArgs());
            }
            else
            {
                Environment.Exit(returnCode);
            }
        }

        /// <summary>
        /// Shows the diagnostics window
        /// </summary>
        private void ShowDiagnosticWindowIfNeeded()
        {
            bool isShown = false;
            if (this.Configuration != null)
            {
                isShown = this.Configuration.ShowDiagnosticWindow;
            }
            _diagnostic.ShowDiagnosticWindowIfNeeded(isShown);
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
            if (TrustEverySSLConnection)
            {
                // verify if we talk about our app cast dll 
                HttpWebRequest req = sender as HttpWebRequest;
                if (req == null)
                    return (certificate is X509Certificate2) ? ((X509Certificate2)certificate).Verify() : false;

                // if so just return our trust 
                if (req.RequestUri.Equals(new Uri(_AppCastUrl)))
                    return true;
                else
                    return (certificate is X509Certificate2) ? ((X509Certificate2)certificate).Verify() : false;
            }
            else
            {
                // check our cert                 
                return (certificate is X509Certificate2) ? ((X509Certificate2)certificate).Verify() : false;
            }
        }

        /// <summary>
        /// Does a one-off check for updates
        /// </summary>
        /// <param name="isUserInterfaceShown"><c>true</c> if the user interface is to be shown.</param>
        public bool CheckForUpdates(bool isUserInterfaceShown)
        {
            NetSparkleConfiguration config = GetApplicationConfig();
            // update profile information is needed
            UpdateSystemProfileInformation(config);

            // check if update is required
            NetSparkleAppCastItem latestVersion = null;
            if (IsUpdateRequired(config, out latestVersion))
            {
                // show the update window
                ReportDiagnosticMessage("Update needed from version " + config.InstalledVersion + " to version " + latestVersion.Version);

                // send notification if needed
                UpdateDetectedEventArgs ev = new UpdateDetectedEventArgs() { NextAction = NextUpdateAction.ShowStandardUserInterface, ApplicationConfig = config, LatestVersion = latestVersion };
                if (UpdateDetected != null)
                    UpdateDetected(this, ev);

                // check results
                if (isUserInterfaceShown)
                {
                    switch (ev.NextAction)
                    {
                        case NextUpdateAction.PerformUpdateUnattended:
                            EnableSilentMode = true;
                            Update(latestVersion);
                            break;
                        case NextUpdateAction.ProhibitUpdate:
                            break;
                        case NextUpdateAction.ShowStandardUserInterface:
                        default:
                            Update(latestVersion);
                            break;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates from appcast
        /// </summary>
        /// <param name="currentItem">the current (top-most) item in the app-cast</param>
        private void Update(NetSparkleAppCastItem currentItem)
        {
            if (currentItem != null)
            {
                // show the update ui
                if (EnableSilentMode)
                {
                    InitDownloadAndInstallProcess(currentItem);
                }
                else
                {
                    ShowUpdateNeededUI(currentItem);
                }
            }
        }

        /// <summary>
        /// Cancels the install
        /// </summary>
        public void CancelInstall()
        {
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
        void OnUserWindowUserResponded(object sender, EventArgs e)
        {
            if (this.UserWindow.Result == DialogResult.No)
            {
                // skip this version
                NetSparkleConfiguration config = GetApplicationConfig();
                config.SetVersionToSkip(this.UserWindow.CurrentItem.Version);
            }
            else if (this.UserWindow.Result == DialogResult.Yes)
            {
                // download the binaries
                InitDownloadAndInstallProcess(this.UserWindow.CurrentItem);
            }
        }

        /// <summary>
        /// Called when the progress bar fires the update event
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        void OnProgressWindowInstallAndRelaunch(object sender, EventArgs e)
        {
            RunDownloadedInstaller();
        }

        /// <summary>
        /// This method will be executed as worker thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            // store the did run once feature
            bool goIntoLoop = true;
            bool checkTSP = true;
            bool doInitialCheck = _DoInitialCheck;
            bool isInitialCheck = true;

            // start our lifecycles
            do
            {
                // set state
                Boolean bUpdateRequired = false;

                // notify
                if (CheckLoopStarted != null)
                    CheckLoopStarted(this);

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
                Boolean checkTSPInternal = checkTSP;

                if (isInitialCheck && checkTSPInternal)
                    checkTSPInternal = !_ForceInitialCheck;

                // check if it's ok the recheck to software state
                if (checkTSPInternal)
                {
                    TimeSpan csp = DateTime.Now - config.LastCheckTime;
                    if (csp < _CheckFrequency)
                    {
                        ReportDiagnosticMessage(String.Format("Update check performed within the last {0} minutes!", _CheckFrequency.TotalMinutes));
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
                NetSparkleAppCastItem latestVersion = null;
                bUpdateRequired = IsUpdateRequired(config, out latestVersion);
                if (!bUpdateRequired)
                    goto WaitSection;

                // show the update window
                ReportDiagnosticMessage("Update needed from version " + config.InstalledVersion + " to version " + latestVersion.Version);

                // send notification if needed
                UpdateDetectedEventArgs ev = new UpdateDetectedEventArgs() { NextAction = NextUpdateAction.ShowStandardUserInterface, ApplicationConfig = config, LatestVersion = latestVersion };
                if (UpdateDetected != null)
                    UpdateDetected(this, ev);

                // check results
                switch (ev.NextAction)
                {
                    case NextUpdateAction.PerformUpdateUnattended:
                        {
                            ReportDiagnosticMessage("Unattended update whished from consumer");
                            EnableSilentMode = true;
                            _worker.ReportProgress(1, latestVersion);
                            break;
                        }
                    case NextUpdateAction.ProhibitUpdate:
                        {
                            ReportDiagnosticMessage("Update prohibited from consumer");
                            break;
                        }
                    case NextUpdateAction.ShowStandardUserInterface:
                    default:
                        {
                            ReportDiagnosticMessage("Standard UI update whished from consumer");
                            _worker.ReportProgress(1, latestVersion);
                            break;
                        }
                }

            WaitSection:
                // reset initialcheck
                isInitialCheck = false;

                // notify
                if (CheckLoopFinished != null)
                    CheckLoopFinished(this, bUpdateRequired);

                // report wait statement
                ReportDiagnosticMessage(String.Format("Sleeping for an other {0} minutes, exit event or force update check event", _CheckFrequency.TotalMinutes));

                // wait for
                if (!goIntoLoop)
                    break;
                else
                {
                    // build the event array
                    WaitHandle[] handles = new WaitHandle[1];
                    handles[0] = _exitHandle;

                    // wait for any
                    int i = WaitHandle.WaitAny(handles, _CheckFrequency);
                    if (WaitHandle.WaitTimeout == i)
                    {
                        ReportDiagnosticMessage(String.Format("{0} minutes are over", _CheckFrequency.TotalMinutes));
                        continue;
                    }

                    // check the exit hadnle
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
                        continue;
                    }
                }
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
                    Update(e.UserState as NetSparkleAppCastItem);
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
        void OnWebDownloadClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                // test the item for DSA signature
                bool isDSAOk = false;

                // report
                ReportDiagnosticMessage("Performing DSA check");

                // get the assembly
                if (File.Exists(_downloadTempFileName))
                {
                    // check if the file was downloaded successfully
                    String absolutePath = Path.GetFullPath(_downloadTempFileName);
                    if (!File.Exists(absolutePath))
                        throw new FileNotFoundException();

                    // get the assembly reference from which we start the update progress
                    // only from this trusted assembly the public key can be used
                    Assembly refassembly = System.Reflection.Assembly.GetEntryAssembly();
                    if (refassembly != null)
                    {
                        // Check if we found the public key in our entry assembly
                        if (NetSparkleDSAVerificator.ExistsPublicKey("NetSparkle_DSA.pub"))
                        {
                            // check the DSA Code and modify the back color            
                            NetSparkleDSAVerificator dsaVerifier = new NetSparkleDSAVerificator("NetSparkle_DSA.pub");
                            isDSAOk = dsaVerifier.VerifyDSASignature(this.UserWindow.CurrentItem.DSASignature, _downloadTempFileName);
                        }
                    }
                }
                this.ProgressWindow.IsDownloadDSAValid = isDSAOk;
            }
            this.ProgressWindow.OnClientDownloadFileCompleted(sender, e);
        }


    }
}
