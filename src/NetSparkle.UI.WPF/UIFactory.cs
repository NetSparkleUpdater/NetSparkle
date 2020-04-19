using System;
using System.Drawing;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.Properties;
using System.Windows.Media;
using System.Windows;
using System.Threading;
using System.Collections.Generic;
using NetSparkleUpdater.UI.WPF.ViewModels;

namespace NetSparkleUpdater.UI.WPF
{
    /// <summary>
    /// UI factory for default interface
    /// </summary>
    public class UIFactory : IUIFactory
    {
        private ImageSource _applicationIcon = null;

        public UIFactory()
        {

        }

        public UIFactory(ImageSource applicationIcon)
        {
            _applicationIcon = applicationIcon;
            _applicationIcon?.Freeze();
        }

        /// <summary>
        /// Create sparkle form implementation
        /// </summary>
        /// <param name="sparkle">The <see cref="Sparkle"/> instance to use</param>
        /// <param name="updates">Sorted array of updates from latest to earliest</param>
        /// <param name="isUpdateAlreadyDownloaded">If true, make sure UI text shows that the user is about to install the file instead of download it.</param>
        public virtual IUpdateAvailable CreateUpdateAvailableWindow(Sparkle sparkle, List<AppCastItem> updates, bool isUpdateAlreadyDownloaded = false)
        {
            var viewModel = new UpdateAvailableWindowViewModel();
            var window = new UpdateAvailableWindow(viewModel)
            {
                Icon = _applicationIcon
            };
            viewModel.Initialize(sparkle, updates, isUpdateAlreadyDownloaded);
            return window;
        }

        /// <summary>
        /// Create download progress window
        /// </summary>
        /// <param name="item">Appcast item to download</param>
        public virtual IDownloadProgress CreateProgressWindow(AppCastItem item)
        {
            var viewModel = new DownloadProgressWindowViewModel()
            {
                ItemToDownload = item
            };
            return new DownloadProgressWindow(viewModel)
            {
                Icon = _applicationIcon
            };
        }

        /// <summary>
        /// Inform user in some way that NetSparkle is checking for updates
        /// </summary>
        public virtual ICheckingForUpdates ShowCheckingForUpdates()
        {
            return new CheckingForUpdatesWindow { Icon = _applicationIcon };
        }

        /// <summary>
        /// Initialize UI. Called when Sparkle is constructed and/or when the UIFactory is set.
        /// </summary>
        public virtual void Init()
        {
        }

        /// <summary>
        /// Show user a message saying downloaded update format is unknown
        /// </summary>
        /// <param name="downloadFileName">The filename to be inserted into the message text</param>
        public virtual void ShowUnknownInstallerFormatMessage(string downloadFileName)
        {
            ShowMessage(Resources.DefaultUIFactory_MessageTitle, 
                string.Format(Resources.DefaultUIFactory_ShowUnknownInstallerFormatMessageText, downloadFileName));
        }

        /// <summary>
        /// Show user that current installed version is up-to-date
        /// </summary>
        public virtual void ShowVersionIsUpToDate()
        {
            ShowMessage(Resources.DefaultUIFactory_MessageTitle, Resources.DefaultUIFactory_ShowVersionIsUpToDateMessage);
        }

        /// <summary>
        /// Show message that latest update was skipped by user
        /// </summary>
        public virtual void ShowVersionIsSkippedByUserRequest()
        {
            ShowMessage(Resources.DefaultUIFactory_MessageTitle, Resources.DefaultUIFactory_ShowVersionIsSkippedByUserRequestMessage);
        }

        /// <summary>
        /// Show message that appcast is not available
        /// </summary>
        /// <param name="appcastUrl">the URL for the appcast file</param>
        public virtual void ShowCannotDownloadAppcast(string appcastUrl)
        {
            ShowMessage(Resources.DefaultUIFactory_ErrorTitle, Resources.DefaultUIFactory_ShowCannotDownloadAppcastMessage);
        }

        public virtual bool CanShowToastMessages()
        {
            return true;
        }

        /// <summary>
        /// Show 'toast' window to notify new version is available
        /// </summary>
        /// <param name="updates">Appcast updates</param>
        /// <param name="clickHandler">handler for click</param>
        public virtual void ShowToast(List<AppCastItem> updates, Action<List<AppCastItem>> clickHandler)
        {
            Thread thread = new Thread(() =>
            {
                var toast = new ToastNotification()
                {
                    ClickAction = clickHandler,
                    Updates = updates,
                    Icon = _applicationIcon
                };
                try
                {
                    toast.Show(Resources.DefaultUIFactory_ToastMessage, Resources.DefaultUIFactory_ToastCallToAction, 5);
                    System.Windows.Threading.Dispatcher.Run();
                }
                catch (ThreadAbortException)
                {
                    toast.Dispatcher.InvokeShutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// Show message on download error
        /// </summary>
        /// <param name="message">Error message from exception</param>
        /// <param name="appcastUrl">the URL for the appcast file</param>
        public virtual void ShowDownloadErrorMessage(string message, string appcastUrl)
        {
            ShowMessage(Resources.DefaultUIFactory_ErrorTitle, string.Format(Resources.DefaultUIFactory_ShowDownloadErrorMessage, message));
        }

        private void ShowMessage(string title, string message)
        {
            var messageWindow = new MessageNotificationWindow(new MessageNotificationWindowViewModel(message))
            {
                Title = title,
                Icon = _applicationIcon
            };
            messageWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            messageWindow.ShowDialog();
        }

        /// <summary>
        /// Shut down the UI so we can run an update.
        /// If in WPF, System.Windows.Application.Current.Shutdown().
        /// If in WinForms, Application.Exit().
        /// </summary>
        public void Shutdown()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
