using System;
using System.Drawing;
using System.Windows.Forms;
using NetSparkle.Interfaces;
using NetSparkle.Properties;

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
            MessageBox.Show(string.Format(Resources.DefaultNetSparkleUIFactory_ShowUnknownInstallerFormatMessageText, downloadFileName), Resources.DefaultNetSparkleUIFactory_ErrorTitle, 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Show user that current installed version is up-to-date
        /// </summary>
        public void ShowVersionIsUpToDate()
        {
            MessageBox.Show(Resources.DefaultNetSparkleUIFactory_ShowVersionIsUpToDateMessage, Resources.DefaultNetSparkleUIFactory_MessageTitle);
        }

        /// <summary>
        /// Show message that latest update was skipped by user
        /// </summary>
        public void ShowVersionIsSkippedByUserRequest()
        {
            MessageBox.Show(Resources.DefaultNetSparkleUIFactory_ShowVersionIsSkippedByUserRequestMessage, Resources.DefaultNetSparkleUIFactory_MessageTitle);//review: I'm not crystal clear on this one
        }

        /// <summary>
        /// Show message that appcast is not available
        /// </summary>
        /// <param name="appcastUrl"></param>
        public void ShowCannotDownloadAppcast(string appcastUrl)
        {
            MessageBox.Show(Resources.DefaultNetSparkleUIFactory_ShowCannotDownloadAppcastMessage, Resources.DefaultNetSparkleUIFactory_ErrorTitle);
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
            toast.Show(Resources.DefaultNetSparkleUIFactory_ToastMessage, Resources.DefaultNetSparkleUIFactory_ToastCallToAction, 5);
        }
    }
}
