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
    /// UI factory for WPF UI interface
    /// </summary>
    public class UIFactory : IUIFactory
    {
        private ImageSource _applicationIcon = null;

        /// <summary>
        /// Create a new UIFactory for WPF applications
        /// </summary>
        public UIFactory()
        {
            HideReleaseNotes = false;
            HideRemindMeLaterButton = false;
            HideSkipButton = false;
            ReleaseNotesHTMLTemplate = "";
            AdditionalReleaseNotesHeaderHTML = "";
        }

        /// <summary>
        /// Create a new UIFactory for WPF applications with the given
        /// application icon to show in all update windows
        /// </summary>
        /// <param name="applicationIcon">the <see cref="ImageSource"/> to show in all windows</param>
        public UIFactory(ImageSource applicationIcon) : this()
        {
            _applicationIcon = applicationIcon;
            _applicationIcon?.Freeze();
        }

        /// <inheritdoc/>
        public bool HideReleaseNotes { get; set; }

        /// <inheritdoc/>
        public bool HideSkipButton { get; set; }

        /// <inheritdoc/>
        public bool HideRemindMeLaterButton { get; set; }

        /// <inheritdoc/>
        public string ReleaseNotesHTMLTemplate { get; set; }

        /// <inheritdoc/>
        public string AdditionalReleaseNotesHeaderHTML { get; set; }

        /// <inheritdoc/>
        public virtual IUpdateAvailable CreateUpdateAvailableWindow(SparkleUpdater sparkle, List<AppCastItem> updates, bool isUpdateAlreadyDownloaded = false)
        {
            var viewModel = new UpdateAvailableWindowViewModel();
            var window = new UpdateAvailableWindow(viewModel)
            {
                Icon = _applicationIcon
            };
            if (HideReleaseNotes)
            {
                (window as IUpdateAvailable).HideReleaseNotes();
            }
            if (HideSkipButton)
            {
                (window as IUpdateAvailable).HideSkipButton();
            }
            if (HideRemindMeLaterButton)
            {
                (window as IUpdateAvailable).HideRemindMeLaterButton();
            }
            viewModel.Initialize(sparkle, updates, isUpdateAlreadyDownloaded, ReleaseNotesHTMLTemplate, AdditionalReleaseNotesHeaderHTML);
            return window;
        }

        /// <inheritdoc/>
        public virtual IDownloadProgress CreateProgressWindow(SparkleUpdater sparkle, AppCastItem item)
        {
            var viewModel = new DownloadProgressWindowViewModel()
            {
                ItemToDownload = item,
                SoftwareWillRelaunchAfterUpdateInstalled = sparkle.RelaunchAfterUpdate
            };
            return new DownloadProgressWindow(viewModel)
            {
                Icon = _applicationIcon
            };
        }

        /// <inheritdoc/>
        public virtual ICheckingForUpdates ShowCheckingForUpdates(SparkleUpdater sparkle)
        {
            return new CheckingForUpdatesWindow { Icon = _applicationIcon };
        }

        /// <inheritdoc/>
        public virtual void Init(SparkleUpdater sparkle)
        {
        }

        /// <inheritdoc/>
        public virtual void ShowUnknownInstallerFormatMessage(SparkleUpdater sparkle, string downloadFileName)
        {
            ShowMessage(Resources.DefaultUIFactory_MessageTitle, 
                string.Format(Resources.DefaultUIFactory_ShowUnknownInstallerFormatMessageText, downloadFileName));
        }

        /// <inheritdoc/>
        public virtual void ShowVersionIsUpToDate(SparkleUpdater sparkle)
        {
            ShowMessage(Resources.DefaultUIFactory_MessageTitle, Resources.DefaultUIFactory_ShowVersionIsUpToDateMessage);
        }

        /// <inheritdoc/>
        public virtual void ShowVersionIsSkippedByUserRequest(SparkleUpdater sparkle)
        {
            ShowMessage(Resources.DefaultUIFactory_MessageTitle, Resources.DefaultUIFactory_ShowVersionIsSkippedByUserRequestMessage);
        }

        /// <inheritdoc/>
        public virtual void ShowCannotDownloadAppcast(SparkleUpdater sparkle, string appcastUrl)
        {
            ShowMessage(Resources.DefaultUIFactory_ErrorTitle, Resources.DefaultUIFactory_ShowCannotDownloadAppcastMessage);
        }

        /// <inheritdoc/>
        public virtual bool CanShowToastMessages(SparkleUpdater sparkle)
        {
            return true;
        }

        /// <inheritdoc/>
        public virtual void ShowToast(SparkleUpdater sparkle, List<AppCastItem> updates, Action<List<AppCastItem>> clickHandler)
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

        /// <inheritdoc/>
        public virtual void ShowDownloadErrorMessage(SparkleUpdater sparkle, string message, string appcastUrl)
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

        /// <inheritdoc/>
        public void Shutdown(SparkleUpdater sparkle)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
