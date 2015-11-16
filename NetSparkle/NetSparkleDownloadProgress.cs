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

            // show the right 
            Size = new Size(Size.Width, 107);
            lblSecurityHint.Visible = false;
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
        public void ChangeDownloadState(bool signatureValid)
        {
            progressDownload.Visible = false;
            btnInstallAndReLaunch.Visible = true;

            UpdateDownloadValid(signatureValid);
        }

        /// <summary>
        /// Force window close
        /// </summary>
        public void ForceClose()
        {
            DialogResult = DialogResult.Abort;
            Close();
        }

        /// <summary>
        /// Updates the UI to indicate if the download is valid
        /// </summary>
        private void UpdateDownloadValid(bool signatureValid)
        {
            if (!signatureValid)
            {
                Size = new Size(Size.Width, 137);
                lblSecurityHint.Visible = true;
                BackColor = Color.Tomato;
            }
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
                        return Math.Round(numBytesDecimal, 2).ToString() + " GB";
                    }
                    return Math.Round(numBytesDecimal, 2).ToString() + " MB";
                }
                return Math.Round(numBytesDecimal, 2).ToString() + " KB";
            }
            return numBytes.ToString();
        }
               
        /// <summary>
        /// Event called when the client download progress changes
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        public void OnClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressDownload.Value = e.ProgressPercentage;
            long bytesReceived = e.BytesReceived;
            long bytesTotal = e.TotalBytesToReceive;
            //Console.WriteLine("{0} / {1}", bytesReceived, bytesTotal);
            downloadProgressLbl.Text = " (" + numBytesToUserReadableString(bytesReceived) + " / " + 
                numBytesToUserReadableString(bytesTotal) + ")";
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
        }

        private void NetSparkleDownloadProgress_Load(object sender, EventArgs e)
        {

        }
    }
}
