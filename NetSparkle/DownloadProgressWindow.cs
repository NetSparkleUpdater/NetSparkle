using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using NetSparkle.Interfaces;

namespace NetSparkle
{
    /// <summary>
    /// A progress bar
    /// </summary>
    public partial class DownloadProgressWindow : Form, IDownloadProgress
    {
        /// <summary>
        /// event to fire when the form asks the application to be relaunched
        /// </summary>
        public event EventHandler InstallAndRelaunch;

        private bool _shouldLaunchInstallFileOnClose = false;

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
            if (_shouldLaunchInstallFileOnClose)
            {
                InstallAndRelaunch?.Invoke(this, new EventArgs());
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        /// <summary>
        /// Show the UI and waits
        /// </summary>
        bool IDownloadProgress.ShowDialog()
        {
            return DefaultUIFactory.ConvertDialogResultToDownloadProgressResult(ShowDialog());
        }

        /// <summary>
        /// Update UI to show file is downloaded and signature check result
        /// </summary>
        public void FinishedDownloadingFile(bool isDownloadedFileValid)
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

        /// <summary>
        /// Display an error message
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        public bool DisplayErrorMessage(string errorMessage)
        {
            downloadProgressLbl.Visible = true;
            progressDownload.Visible = false;
            btnInstallAndReLaunch.Visible = false;
            buttonCancel.Text = "Close";
            downloadProgressLbl.Text = errorMessage;
            return true;
        }

        /// <summary>
        /// Force window close
        /// </summary>
        public void ForceClose()
        {
            DialogResult = DialogResult.Abort;
            Close();
        }

        private string numBytesToUserReadableString(long numBytes)
        {
            if (numBytes > 1024)
            {
                double numBytesDecimal = numBytes;
                // Put in KB
                numBytesDecimal /= 1024;
                if (numBytesDecimal > 1024)
                {
                    // Put in MB
                    numBytesDecimal /= 1024;
                    if (numBytesDecimal > 1024)
                    {
                        // Put in GB
                        numBytesDecimal /= 1024;
                        return numBytesDecimal.ToString("F2") + " GB";
                    }
                    return numBytesDecimal.ToString("F2") + " MB";
                }
                return numBytesDecimal.ToString("F2") + " KB";
            }
            return numBytes.ToString();
        }

        /// <summary>
        /// Event called when the client download progress changes
        /// </summary>
        private void OnDownloadProgressChanged(object sender, long bytesReceived, long totalBytesToReceive, int percentage)
        {
            progressDownload.Value = percentage;
            downloadProgressLbl.Text = " (" + numBytesToUserReadableString(bytesReceived) + " / " + 
                numBytesToUserReadableString(totalBytesToReceive) + ")";
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
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
            DialogResult = DialogResult.OK;
            _shouldLaunchInstallFileOnClose = true;
            Close();
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
        {
            btnInstallAndReLaunch.Enabled = shouldBeEnabled;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
