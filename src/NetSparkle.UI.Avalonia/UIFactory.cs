using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.Properties;
using NetSparkleUpdater.UI.Avalonia.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace NetSparkleUpdater.UI.Avalonia
{
    /// <summary>
    /// UI factory for Avalonia UI interface
    /// </summary>
    public class UIFactory : IUIFactory
    {
        private WindowIcon _applicationIcon = null;

        private Bitmap _iconBitmap;

        /// <inheritdoc/>
        public UIFactory()
        {
            HideReleaseNotes = false;
            HideRemindMeLaterButton = false;
            HideSkipButton = false;
        }

        /// <inheritdoc/>
        public UIFactory(WindowIcon applicationIcon, string releaseNotesSeparatorTemplate = "", string releaseNotesHeadAddition = "") : this()
        {
            _applicationIcon = applicationIcon;
            if (applicationIcon != null)
            {
                using (var stream = new MemoryStream())
                {
                    applicationIcon?.Save(stream);
                    stream.Position = 0;
                    _iconBitmap = new Bitmap(stream);
                }
            }
            ReleaseNotesHTMLTemplate = releaseNotesSeparatorTemplate;
            AdditionalReleaseNotesHeaderHTML = releaseNotesHeadAddition;
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
            var window = new UpdateAvailableWindow(viewModel, _iconBitmap)
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
                ItemToDownload = item
            };
            return new DownloadProgressWindow(viewModel, _iconBitmap)
            {
                Icon = _applicationIcon
            };
        }

        /// <inheritdoc/>
        public virtual ICheckingForUpdates ShowCheckingForUpdates(SparkleUpdater sparkle)
        {
            return new CheckingForUpdatesWindow(_iconBitmap)
            { 
                Icon = _applicationIcon
            };
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
            return false;
        }

        /// <inheritdoc/>
        public virtual void ShowToast(SparkleUpdater sparkle, List<AppCastItem> updates, Action<List<AppCastItem>> clickHandler)
        {
        }

        /// <inheritdoc/>
        public virtual void ShowDownloadErrorMessage(SparkleUpdater sparkle, string message, string appcastUrl)
        {
            ShowMessage(Resources.DefaultUIFactory_ErrorTitle, string.Format(Resources.DefaultUIFactory_ShowDownloadErrorMessage, message));
        }

        private void ShowMessage(string title, string message)
        {
            var messageWindow = new MessageNotificationWindow(new MessageNotificationWindowViewModel(message), _iconBitmap)
            {
                Title = title,
                Icon = _applicationIcon
            };
            messageWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            messageWindow.Show(); // TODO: This was ShowDialog; will this break anything?
        }

        /// <inheritdoc/>
        public void Shutdown(SparkleUpdater sparkle)
        {
            (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
        }
    }
}
