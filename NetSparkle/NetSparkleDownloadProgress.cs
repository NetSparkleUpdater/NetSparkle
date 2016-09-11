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
    public partial class NetSparkleDownloadProgress : Form, INetSparkleDownloadProgress
    {
        /// <summary>
        /// event to fire when the form asks the application to be relaunched
        /// </summary>
        public event EventHandler InstallAndRelaunch;

        private bool _wasClosedDuringDownload;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="applicationIcon">Your application Icon</param>
        public NetSparkleDownloadProgress(NetSparkleAppCastItem item, Icon applicationIcon)
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
            _wasClosedDuringDownload = false;

            FormClosing += NetSparkleDownloadProgress_FormClosing;
        }

        private void NetSparkleDownloadProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            _wasClosedDuringDownload = true;
        }

        /// <summary>
        /// Show the UI and waits
        /// </summary>
        DialogResult INetSparkleDownloadProgress.ShowDialog()
        {
            return base.ShowDialog();
        }

        /// <summary>
        /// Update UI to show file is downloaded and signature check result
        /// </summary>
        /// <param name="signatureValid"></param>
        public void ChangeDownloadState()
        {
            progressDownload.Visible = false;
            buttonCancel.Visible = false;
            downloadProgressLbl.Visible = false;
            btnInstallAndReLaunch.Visible = true;
            FormClosing -= NetSparkleDownloadProgress_FormClosing;
        }

        /// <summary>
        /// Force window close
        /// </summary>
        public void ForceClose()
        {
            DialogResult = DialogResult.Abort;
            Close();
            _wasClosedDuringDownload = true;
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
        /// <param name="sender"></param>
        /// <param name="bytesReceived"></param>
        /// <param name="totalBytesToReceive"></param>
        /// <param name="percentage"></param>
        private void OnDownloadProgressChanged(object sender, long bytesReceived, long totalBytesToReceive, int percentage)
        {
            progressDownload.Value = percentage;
            downloadProgressLbl.Text = " (" + numBytesToUserReadableString(bytesReceived) + " / " + 
                numBytesToUserReadableString(totalBytesToReceive) + ")";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            InstallAndRelaunch?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="shouldBeEnabled"></param>
        public void SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
        {
            btnInstallAndReLaunch.Enabled = shouldBeEnabled;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            _wasClosedDuringDownload = true;
            Close();
        }
    }
}
