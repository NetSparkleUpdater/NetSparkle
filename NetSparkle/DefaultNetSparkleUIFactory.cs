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
        /// <param name="updates">Sorted array of updates from latest to previous</param>
        /// <param name="applicationIcon">Icon</param>
        /// <returns></returns>
        public virtual INetSparkleForm CreateSparkleForm(Sparkle sparkle, NetSparkleAppCastItem[] updates, Icon applicationIcon)
        {
            return new NetSparkleForm(sparkle, updates, applicationIcon);
        }

        /// <summary>
        /// Create download progress window
        /// </summary>
        /// <param name="item">Appcast item to download</param>
        /// <param name="applicationIcon">Application icon to use</param>
        /// <returns></returns>
        public virtual INetSparkleDownloadProgress CreateProgressWindow(NetSparkleAppCastItem item, Icon applicationIcon)
        {
            return new NetSparkleDownloadProgress(item, applicationIcon);
        }

        /// <summary>
        /// Initialize UI. Called when Sparkle is constructed.
        /// </summary>
        public virtual void Init()
        {
            // enable visual style to ensure that we have XP style or higher
            // also in WPF applications
            Application.EnableVisualStyles();
        }

        /// <summary>
        /// Show user a message saying downloaded update format is unknown
        /// </summary>
        /// <param name="downloadFileName"></param>
        public virtual void ShowUnknownInstallerFormatMessage(string downloadFileName, Icon applicationIcon = null)
        {
            ShowMessage(Resources.DefaultNetSparkleUIFactory_MessageTitle, 
                string.Format(Resources.DefaultNetSparkleUIFactory_ShowUnknownInstallerFormatMessageText, downloadFileName), applicationIcon);
        }

        /// <summary>
        /// Show user that current installed version is up-to-date
        /// </summary>
        public virtual void ShowVersionIsUpToDate(Icon applicationIcon = null)
        {
            ShowMessage(Resources.DefaultNetSparkleUIFactory_MessageTitle, Resources.DefaultNetSparkleUIFactory_ShowVersionIsUpToDateMessage, applicationIcon);
        }

        /// <summary>
        /// Show message that latest update was skipped by user
        /// </summary>
        public virtual void ShowVersionIsSkippedByUserRequest(Icon applicationIcon = null)
        {
            ShowMessage(Resources.DefaultNetSparkleUIFactory_MessageTitle, Resources.DefaultNetSparkleUIFactory_ShowVersionIsSkippedByUserRequestMessage, applicationIcon);
        }

        /// <summary>
        /// Show message that appcast is not available
        /// </summary>
        /// <param name="appcastUrl"></param>
        public virtual void ShowCannotDownloadAppcast(string appcastUrl, Icon applicationIcon = null)
        {
            ShowMessage(Resources.DefaultNetSparkleUIFactory_ErrorTitle, Resources.DefaultNetSparkleUIFactory_ShowCannotDownloadAppcastMessage, applicationIcon);
        }

        /// <summary>
        /// Show 'toast' window to notify new version is available
        /// </summary>
        /// <param name="updates">Appcast updates</param>
        /// <param name="applicationIcon">Icon to use in window</param>
        /// <param name="clickHandler">handler for click</param>
        public virtual void ShowToast(NetSparkleAppCastItem[] updates, Icon applicationIcon, Action<NetSparkleAppCastItem[]> clickHandler)
        {
            var toast = new ToastNotifier
                {
                    Image =
                        {
                            Image = applicationIcon != null ? applicationIcon.ToBitmap() : Resources.software_update_available1
                        }
                };
            toast.ToastClicked += (sender, args) => clickHandler(updates); // TODO: this is leak
            toast.Show(Resources.DefaultNetSparkleUIFactory_ToastMessage, Resources.DefaultNetSparkleUIFactory_ToastCallToAction, 5);
        }

        /// <summary>
        /// Show message on download error
        /// </summary>
        /// <param name="message">Error message from exception</param>
        /// <param name="appCastUrl"></param>
        public virtual void ShowDownloadErrorMessage(string message, string appCastUrl, Icon applicationIcon = null)
        {
            ShowMessage(Resources.DefaultNetSparkleUIFactory_ErrorTitle, string.Format(Resources.DefaultNetSparkleUIFactory_ShowDownloadErrorMessage, message), applicationIcon);
        }

        private void ShowMessage(string title, string message, Icon applicationIcon = null)
        {
            var messageWindow = new MessageNotificationWindow(title, message, applicationIcon);
            messageWindow.StartPosition = FormStartPosition.CenterScreen;
            messageWindow.ShowDialog();
        }
    }
}
