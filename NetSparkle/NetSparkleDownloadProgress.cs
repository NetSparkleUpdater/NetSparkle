using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using NetSparkle.Interfaces;

namespace NetSparkle
{
    /// <summary>
    /// A progress bar
    /// </summary>
    public partial class NetSparkleDownloadProgress : Form, INetSparkleDownloadProgress
    {
        /// <summary>
        /// event to fire when the form asks the application to be relaunched
        /// </summary>
        public event EventHandler InstallAndRelaunch;

        private String _tempName;
        private NetSparkleAppCastItem _item;
        private Sparkle _sparkle;
        private bool _unattend;
        private bool _isDownloadDSAValid;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sparkle">the sparkle instance</param>
        /// <param name="item"></param>
        /// <param name="applicationIcon">Your application Icon</param>
        /// <param name="Unattend"><c>true</c> if this is an unattended install</param>
        public NetSparkleDownloadProgress(Sparkle sparkle, NetSparkleAppCastItem item, Icon applicationIcon, Boolean Unattend)
        {
            InitializeComponent();

            imgAppIcon.Image = applicationIcon.ToBitmap();
            Icon = applicationIcon;

            // store the item
            _sparkle = sparkle;
            _item = item;
            //_referencedAssembly = referencedAssembly;
            _unattend = Unattend;

            // init ui
            btnInstallAndReLaunch.Visible = false;
            lblHeader.Text = lblHeader.Text.Replace("APP", item.AppName + " " + item.Version);
            progressDownload.Maximum = 100;
            progressDownload.Minimum = 0;
            progressDownload.Step = 1;

            // show the right 
            Size = new Size(Size.Width, 107);
            lblSecurityHint.Visible = false;
        }

        /// <summary>
        /// Gets or sets the temporary file name where the new items are downloaded
        /// </summary>
        public string TempFileName
        {
            get { return _tempName; }
            set { _tempName = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating if the downloaded file matches its listed
        /// DSA hash.
        /// </summary>
        public bool IsDownloadDSAValid
        {
            get { return _isDownloadDSAValid; }
            set
            {
                _isDownloadDSAValid = value;
                UpdateDownloadValid();
            }
        }

        /// <summary>
        /// Show the UI and waits
        /// </summary>
        void INetSparkleDownloadProgress.ShowDialog()
        {
            base.ShowDialog();
        }

        /// <summary>
        /// Event called when the download of the binary is complete
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        public void OnClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null )
            {
                progressDownload.Visible = false;
                btnInstallAndReLaunch.Visible = true;

                // this should move to Sparkle itself.
                // report message            
                _sparkle.ReportDiagnosticMessage("Finished downloading file to: " + _tempName);

                // check if we have a dsa signature in appcast            
                if (_item.DSASignature == null || _item.DSASignature.Length == 0)
                {
                    _sparkle.ReportDiagnosticMessage("No DSA check needed");
                }
                else
                {
                    this.IsDownloadDSAValid = false;

                    // report
                    _sparkle.ReportDiagnosticMessage("Performing DSA check");

                    // get the assembly
                    if (File.Exists(_tempName))
                    {
                        // check if the file was downloaded successfully
                        String absolutePath = Path.GetFullPath(_tempName);
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
                                this.IsDownloadDSAValid = dsaVerifier.VerifyDSASignature(_item.DSASignature, _tempName);
                            }
                        }
                    }

                    UpdateDownloadValid();
                }

                // Check the unattended mode
                if (_unattend)
                    OnInstallAndReLaunchClick(null, null);
            }
        }

        /// <summary>
        /// Updates the UI to indicate if the download is valid
        /// </summary>
        private void UpdateDownloadValid()
        {
            if (!this.IsDownloadDSAValid)
            {
                Size = new Size(Size.Width, 137);
                lblSecurityHint.Visible = true;
                BackColor = Color.Tomato;
            }
        }
               
        /// <summary>
        /// Event called when the client download progress changes
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        public void OnClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressDownload.Value = e.ProgressPercentage;            
        }

        /// <summary>
        /// Event called when the "Install and relaunch" button is clicked
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private void OnInstallAndReLaunchClick(object sender, EventArgs e)
        {
            if (InstallAndRelaunch != null)
            {
                InstallAndRelaunch(this, new EventArgs());
            }
            //RunDownloadedInstaller();
        }

    }
}
