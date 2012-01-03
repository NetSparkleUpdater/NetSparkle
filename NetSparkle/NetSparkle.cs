using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Management;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace AppLimit.NetSparkle
{
    public delegate void LoopStartedOperation(Object sender);
    public delegate void LoopFinishedOperation(Object sender, Boolean UpdateRequired);

    /// <summary>
    /// Everytime when netsparkle detects an update the 
    /// consumer can decide what should happen as next with the help 
    /// of the UpdateDatected event
    /// </summary>
    public enum nNextUpdateAction
    {
        showStandardUserInterface = 1,
        performUpdateUnattended = 2,
        prohibitUpdate = 3
    }

    /// <summary>
    /// Contains all information for the update detected event
    /// </summary>
    public class UpdateDetectedEventArgs : EventArgs
    {
        public nNextUpdateAction NextAction;
        public NetSparkleConfiguration ApplicationConfig;
        public NetSparkleAppCastItem LatestVersion;        
    }

    /// <summary>
    /// This delegate will be used when an update was detected to allow library 
    /// consumer to add own user interface capabilities.    
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void UpdateDetected(Object sender, UpdateDetectedEventArgs e);

    public class Sparkle : IDisposable
    {
        private BackgroundWorker _worker = new BackgroundWorker();

        private String _AppCastUrl;
        private String _AppReferenceAssembly;

        private Boolean _DoInitialCheck;
        private Boolean _ForceInitialCheck;

        private EventWaitHandle _exitHandle;
        private EventWaitHandle _loopingHandle;
        
        private NetSparkleMainWindows _DiagnosticWindow;

        private TimeSpan _CheckFrequency;

        /// <summary>
        /// Enables system profiling against a profile server
        /// </summary>
        public Boolean EnableSystemProfiling = false;

        /// <summary>
        /// Hides the release notes view when an update was found. This 
        /// mode is switched on automatically when no sparkle:releaseNotesLink
        /// tag was found in the app cast         
        /// </summary>
        public Boolean HideReleaseNotes = false;

        /// <summary>
        /// Contains the profile url for System profiling
        /// </summary>
        public Uri SystemProfileUrl;

        /// <summary>
        /// This event will be raised when a check loop will be started
        /// </summary>
        public event LoopStartedOperation checkLoopStarted;

        /// <summary>
        /// This event will be raised when a check loop is finished
        /// </summary>
        public event LoopFinishedOperation checkLoopFinished;

        /// <summary>
        /// This event can be used to override the standard user interface
        /// process when an update is detected
        /// </summary>
        public event UpdateDetected updateDetected;

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
        public Boolean ShowDiagnosticWindow { get; set; }

        /// <summary>
        /// This property enables the silent mode, this means 
        /// the application will be updated without user interaction
        /// </summary>
        public Boolean EnableSilentMode { get; set; }

        /// <summary>
        /// This property returns true when the upadete loop is running
        /// and files when the loop is not running
        /// </summary>
        public Boolean IsUpdateLoopRunning
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
        public Boolean TrustEverySSLConnection { get; set; }      

        /// <summary>
        /// ctor which needs the appcast url
        /// </summary>
        /// <param name="appcastUrl"></param>
        public Sparkle(String appcastUrl)
            : this(appcastUrl, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url and a referenceassembly
        /// </summary>
        public Sparkle(String appcastUrl, String referenceAssembly)
            : this(appcastUrl, referenceAssembly, false)
        { }

        /// <summary>
        /// ctor which needs the appcast url and a referenceassembly
        /// </summary>        
        public Sparkle(String appcastUrl, String referenceAssembly, Boolean ShowDiagnostic)
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
            ShowDiagnosticWindow = ShowDiagnostic;

            // create the diagnotic window
            _DiagnosticWindow = new NetSparkleMainWindows();

            // set the reference assembly
            if (referenceAssembly != null)
            {
                _AppReferenceAssembly = referenceAssembly;
                _DiagnosticWindow.Report("Checking the following file: " + _AppReferenceAssembly);
            }

            // show if needed
            ShowDiagnosticWindowIfNeeded();            

            // adjust the delegates
            _worker.WorkerReportsProgress = true;
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.ProgressChanged += new ProgressChangedEventHandler(_worker_ProgressChanged);

            // build the wait handle
            _exitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            _loopingHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            // set the url
            _AppCastUrl = appcastUrl;
            _DiagnosticWindow.Report("Using the following url: " + _AppCastUrl);            
        }

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
        /// <param name="doInitialCheck"></param>
        /// <param name="checkFrequency"></param>
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
        /// <param name="doInitialCheck"></param>
        /// <param name="forceInitialCheck"></param>
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
        /// <param name="doInitialCheck"></param>
        /// <param name="forceInitialCheck"></param>
        /// <param name="checkFrequency"></param>
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
            _DiagnosticWindow.Report("Starting background worker");

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
        }

        /// <summary>
        /// This method updates the profile information which can be sended to the server if enabled    
        /// </summary>
        /// <param name="config"></param>
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
        /// <param name="obj"></param>
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
                    using (WebResponse response = request.GetResponse())
                    {
                        // close the response 
                        response.Close();
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
        /// <param name="config"></param>
        /// <returns></returns>
        public Boolean IsUpdateRequired(NetSparkleConfiguration config, out NetSparkleAppCastItem latestVersion)
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
        /// <param name="AppReferenceAssembly"></param>
        /// <returns></returns>
        public NetSparkleConfiguration GetApplicationConfig()
        {
            NetSparkleConfiguration config;
            config = new NetSparkleConfiguration(_AppReferenceAssembly);
            return config;
        }

        /// <summary>
        /// This method shows the update ui and allows to perform the 
        /// update process
        /// </summary>
        /// <param name="currentItem"></param>
        public void ShowUpdateNeededUI(NetSparkleAppCastItem currentItem)
        {
            // create the form
            NetSparkleForm frm = new NetSparkleForm(currentItem, ApplicationIcon, ApplicationWindowIcon);

            // configure the form
            frm.TopMost = true;

            if (HideReleaseNotes)
                frm.RemoveReleaseNotesControls();

            // show it
            DialogResult dlgResult = frm.ShowDialog();

            if (dlgResult == DialogResult.No)
            {
                // skip this version
                NetSparkleConfiguration config = new NetSparkleConfiguration(_AppReferenceAssembly);
                config.SetVersionToSkip(currentItem.Version);
            }
            else if (dlgResult == DialogResult.Yes)
            {
                // download the binaries
                InitDownloadAndInstallProcess(currentItem);
            }
        }

        /// <summary>
        /// This method reports a message in the diagnostic window
        /// </summary>
        /// <param name="message"></param>
        public void ReportDiagnosticMessage(String message)
        {
            if (_DiagnosticWindow.InvokeRequired)
            {
                _DiagnosticWindow.Invoke(new Action<String>(ReportDiagnosticMessage), message);                
            }
            else
            {
                _DiagnosticWindow.Report(message);
            }
        }

        /// <summary>
        /// This method will be executed as worker thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // store the did run once feature
            Boolean goIntoLoop = true;
            Boolean checkTSP = true;
            Boolean doInitialCheck = _DoInitialCheck;
            Boolean isInitialCheck = true;

            // start our lifecycles
            do
            {
                // set state
                Boolean bUpdateRequired = false;

                // notify
                if (checkLoopStarted != null)
                    checkLoopStarted(this);

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
                UpdateDetectedEventArgs ev = new UpdateDetectedEventArgs() { NextAction = nNextUpdateAction.showStandardUserInterface, ApplicationConfig = config, LatestVersion = latestVersion };
                if (updateDetected != null)
                    updateDetected(this, ev);
                
                // check results
                switch(ev.NextAction)
                {
                    case nNextUpdateAction.performUpdateUnattended:
                        {
                            ReportDiagnosticMessage("Unattended update whished from consumer");
                            EnableSilentMode = true;
                            _worker.ReportProgress(1, latestVersion);
                            break;
                        }
                    case nNextUpdateAction.prohibitUpdate:
                        {
                            ReportDiagnosticMessage("Update prohibited from consumer");
                            break;
                        }
                    case nNextUpdateAction.showStandardUserInterface:
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
                if (checkLoopFinished != null)
                    checkLoopFinished(this, bUpdateRequired);

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
        private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 1:
                    {
                        // get the current item
                        NetSparkleAppCastItem currentItem = e.UserState as NetSparkleAppCastItem;

                        // show the update ui
                        if (EnableSilentMode == true)
                            InitDownloadAndInstallProcess(currentItem);
                        else
                            ShowUpdateNeededUI(currentItem);

                        break;
                    }
                case 0:
                    {
                        ReportDiagnosticMessage(e.UserState.ToString());
                        break;
                    }
            }
        }

        private void InitDownloadAndInstallProcess(NetSparkleAppCastItem item)
        {
            NetSparkleDownloadProgress dlProgress = new NetSparkleDownloadProgress(this, item, _AppReferenceAssembly, ApplicationIcon, ApplicationWindowIcon, EnableSilentMode);
            dlProgress.ShowDialog();
        }

        private void ShowDiagnosticWindowIfNeeded()
        {
            if (_DiagnosticWindow.InvokeRequired)
            {
                _DiagnosticWindow.Invoke(new Action(ShowDiagnosticWindowIfNeeded));
            }
            else
            {
                // check the diagnotic value
                NetSparkleConfiguration config = new NetSparkleConfiguration(_AppReferenceAssembly);
                if (config.ShowDiagnosticWindow || ShowDiagnosticWindow)
                {
                    Point newLocation = new Point();

                    newLocation.X = Screen.PrimaryScreen.Bounds.Width - _DiagnosticWindow.Width;
                    newLocation.Y = 0;

                    _DiagnosticWindow.Location = newLocation;
                    _DiagnosticWindow.Show();
                }
            }
        }

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
    }
}
