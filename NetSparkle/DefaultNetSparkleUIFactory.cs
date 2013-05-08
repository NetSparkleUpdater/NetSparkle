using System;
using System.Drawing;
using System.Windows.Forms;
using NetSparkle.Interfaces;

namespace NetSparkle
{
    /// <summary>
    /// UI factory for default interface
    /// </summary>
    public class DefaultNetSparkleUIFactory : INetSparkleUIFactory
    {
        /// <summary>
        /// Create sparkle form implementation
        /// </summary>
        /// <param name="currentItem">App cast item to show</param>
        /// <param name="applicationIcon">Icon</param>
        /// <returns></returns>
        public INetSparkleForm CreateSparkleForm(NetSparkleAppCastItem currentItem, Icon applicationIcon)
        {
            return new NetSparkleForm(currentItem, applicationIcon);
        }

        /// <summary>
        /// Create download progress window
        /// </summary>
        /// <param name="item">Appcast item to download</param>
        /// <param name="applicationIcon">Application icon to use</param>
        /// <returns></returns>
        public INetSparkleDownloadProgress CreateProgressWindow(NetSparkleAppCastItem item, Icon applicationIcon)
        {
            return new NetSparkleDownloadProgress(item, applicationIcon);
        }

        /// <summary>
        /// Initialize UI. Called when Sparkle is constructed.
        /// </summary>
        public void Init()
        {
            // enable visual style to ensure that we have XP style or higher
            // also in WPF applications
            Application.EnableVisualStyles();
        }

        /// <summary>
        /// Show user a message saying downloaded update format is unknown
        /// </summary>
        /// <param name="downloadFileName"></param>
        public void ShowUnknownInstallerFormatMessage(string downloadFileName)
        {
            MessageBox.Show("Updater not supported, please execute " + downloadFileName + " manually", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Show user that current installed version is up-to-date
        /// </summary>
        public void ShowVersionIsUpToDate()
        {
            MessageBox.Show("Your current version is up to date.");
        }

        /// <summary>
        /// Show message that latest update was skipped by user
        /// </summary>
        public void ShowVersionIsSkippedByUserRequest()
        {
            MessageBox.Show("Your have elected to skip this version.");//review: I'm not crystal clear on this one
        }

        /// <summary>
        /// Show message that appcast is not available
        /// </summary>
        /// <param name="appcastUrl"></param>
        public void ShowCannotDownloadAppcast(string appcastUrl)
        {
            MessageBox.Show("Sorry, either you aren't connected to the internet, or our server is having a problem.");
        }

        /// <summary>
        /// Show 'toast' window to notify new version is available
        /// </summary>
        /// <param name="item">Appcast item</param>
        /// <param name="applicationIcon">Icon to use in window</param>
        /// <param name="clickHandler">handler for click</param>
        public void ShowToast(NetSparkleAppCastItem item, Icon applicationIcon, EventHandler clickHandler)
        {
            var toast = new ToastNotifier
                {
                    Tag = item, 
                    Image =
                        {
                            Image = applicationIcon.ToBitmap()
                        }
                };
            toast.ToastClicked += clickHandler;
            toast.Show("New Version Available", "more information", 5);
        }
    }
}
