using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.Events;

namespace NetSparkleUpdater.UI.WinForms
{
    /// <summary>
    /// A progress bar
    /// </summary>
    public partial class DownloadProgressWindow : Form, IDownloadProgress
    {
        /// <summary>
        /// Event to fire when the download UI is complete; tells you 
        /// if the install process should happen or not
        /// </summary>
        public event DownloadInstallEventHandler DownloadProcessCompleted;

        private bool _shouldLaunchInstallFileOnClose = false;
        private bool _didCallDownloadProcessCompletedHandler = false;

        /// <summary>
        /// Whether or not the software will relaunch after the update has been installed
        /// </summary>
        public bool SoftwareWillRelaunchAfterUpdateInstalled
        {
            set
            {
                btnInstallAndReLaunch.Text = value ? "Install and Relaunch" : "Install";
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="item">The appcast item to use</param>
        /// <param name="applicationIcon">Your application Icon</param>
        public DownloadProgressWindow(AppCastItem item, Icon applicationIcon)
        {
            InitializeComponent();

            imgAppIcon.Image = applicationIcon.ToBitmap();
            Icon = applicationIcon;

            // init ui
            btnInstallAndReLaunch.Visible = false;
            btnInstallAndReLaunch.Text = "Install and Relaunch";
            lblHeader.Text = lblHeader.Text.Replace("APP", item.AppName + " " + item.Version);
            downloadProgressLbl.Text = "";
            progressDownload.Maximum = 100;
            progressDownload.Minimum = 0;
            progressDownload.Step = 1;

            FormClosing += DownloadProgressWindow_FormClosing;
        }

        private void DownloadProgressWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormClosing -= DownloadProgressWindow_FormClosing;
            if (!_didCallDownloadProcessCompletedHandler)
            {
                _didCallDownloadProcessCompletedHandler = true;
                DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(_shouldLaunchInstallFileOnClose));
            }
        }

        /// <summary>
        /// Show the UI and waits
        /// </summary>
        void IDownloadProgress.Show(bool isOnMainThread)
        {
            Show();
            if (!isOnMainThread)
            {
                Application.Run(this);
            }
        }

        /// <summary>
        /// Update UI to show file is downloaded and signature check result
        /// </summary>
        public void FinishedDownloadingFile(bool isDownloadedFileValid)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate () { FinishedDownloadingFile(isDownloadedFileValid); });
            }
            else
            {
                progressDownload.Visible = false;
                buttonCancel.Visible = false;
                downloadProgressLbl.Visible = false;
                if (isDownloadedFileValid)
                {
                    btnInstallAndReLaunch.Visible = true;
                    BackColor = Color.FromArgb(240, 240, 240);
                }
                else
                {
                    btnInstallAndReLaunch.Visible = false;
                    BackColor = Color.Tomato;
                }
            }
        }

        /// <summary>
        /// Display an error message
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        public bool DisplayErrorMessage(string errorMessage)
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate () { DisplayErrorMessage(errorMessage); });
            }
            else
            {
                downloadProgressLbl.Visible = true;
                progressDownload.Visible = false;
                btnInstallAndReLaunch.Visible = false;
                buttonCancel.Text = "Close";
                downloadProgressLbl.Text = errorMessage;
            }
            return true;
        }

        /// <summary>
        /// Close UI
        /// </summary>
        void IDownloadProgress.Close()
        {
            DialogResult = DialogResult.Abort;
            CloseForm();
        }

        private void CloseForm()
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate () { Close(); });
            }
            else
            {
                Close();
            }
        }

        /// <summary>
        /// Event called when the client download progress changes
        /// </summary>
        private void OnDownloadProgressChanged(object sender, long bytesReceived, long totalBytesToReceive, int percentage)
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate () { OnDownloadProgressChanged(sender, bytesReceived, totalBytesToReceive, percentage); });
            }
            else
            {
                progressDownload.Value = percentage;
                downloadProgressLbl.Text = "(" + Utilities.ConvertNumBytesToUserReadableString(bytesReceived) + " / " +
                    Utilities.ConvertNumBytesToUserReadableString(totalBytesToReceive) + ")";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnDownloadProgressChanged(object sender, ItemDownloadProgressEventArgs e)
        {
            OnDownloadProgressChanged(sender, e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
        }

        /// <summary>
        /// Event called when the "Install and relaunch" button is clicked
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private void OnInstallAndReLaunchClick(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate () { OnInstallAndReLaunchClick(sender, e); });
            }
            else
            {
                DialogResult = DialogResult.OK;
                _shouldLaunchInstallFileOnClose = true;
                _didCallDownloadProcessCompletedHandler = true;
                DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(true));
            }
        }

        /// <summary>
        /// Enables or disables the "Install and Relaunch" button
        /// </summary>
        public void SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate () { SetDownloadAndInstallButtonEnabled(shouldBeEnabled); });
            }
            else
            {
                btnInstallAndReLaunch.Enabled = shouldBeEnabled;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            _didCallDownloadProcessCompletedHandler = true;
            DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(false));
        }
    }
}
