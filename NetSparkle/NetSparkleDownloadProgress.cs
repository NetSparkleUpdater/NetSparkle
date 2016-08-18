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
        }

        /// <summary>
        /// Show the UI and waits
        /// </summary>
        void INetSparkleDownloadProgress.ShowDialog()
        {
            base.ShowDialog();
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
                        return Math.Round(numBytesDecimal, 1).ToString() + " GB";
                    }
                    return Math.Round(numBytesDecimal, 1).ToString() + " MB";
                }
                return Math.Round(numBytesDecimal, 0).ToString() + " KB";
            }
            return numBytes.ToString();
        }
               
        /// <summary>
        /// Event called when the client download progress changes
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        public bool OnDownloadProgressChanged(object sender, long bytesReceived, long totalBytesToReceive, int percentage)
        {
            progressDownload.Value = percentage;
            downloadProgressLbl.Text = " (" + numBytesToUserReadableString(bytesReceived) + " / " + 
                numBytesToUserReadableString(totalBytesToReceive) + ")";
            
            return this.Visible;
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

        public void SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
        {
            btnInstallAndReLaunch.Enabled = shouldBeEnabled;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
