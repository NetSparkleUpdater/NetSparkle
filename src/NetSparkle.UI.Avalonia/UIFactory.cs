using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
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
        /// <summary>
        /// Icon used on various windows shown by NetSparkleUpdater
        /// </summary>
        protected WindowIcon _applicationIcon = null;

        private Bitmap _iconBitmap;

        /// <inheritdoc/>
        public UIFactory()
        {
            HideReleaseNotes = false;
            HideRemindMeLaterButton = false;
            HideSkipButton = false;
            UseStaticUpdateWindowBackgroundColor = false;
            UpdateWindowGridBackgroundBrush = (IBrush)new BrushConverter().ConvertFrom("#EEEEEE");
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

        /// <summary>
        /// Whether or not a hardcoded window background color is set on the updates window.
        /// Defaults to true.
        /// </summary>
        public bool UseStaticUpdateWindowBackgroundColor { get; set; }

        /// <summary>
        /// Brush for the background of the main grid on the update (change log) window
        /// </summary>
        public IBrush UpdateWindowGridBackgroundBrush { get; set; }

        /// <summary>
        /// The DateTime.ToString() format used when formatting dates to show in the release notes
        /// header. NetSparkle is not responsible for what happens if you send a bad format! :)
        /// </summary>
        public string ReleaseNotesDateTimeFormat { get; set; } = "D";

        /// <summary>
        /// <para>
        /// Easily set / override the ReleaseNotesGrabber used by the <see cref="UpdateAvailableWindowViewModel"/>.
        /// Note that this will NOT automatically use the <see cref="UIFactory"/> ReleaseNotesHTMLTemplate,
        /// AdditionalReleaseNotesHeaderHTML, and ReleaseNotesDateTimeFormat that you may have set on 
        /// the UIFactory - you must set these on this manual override yourself!
        /// </para>
        /// <para>
        /// Use this if you want to easily override the ReleaseNotesGrabber with your own subclass. Note
        /// that if you want the notes to be styled the same as default, you can use 
        /// <see cref="UpdateAvailableWindowViewModel.GetDefaultReleaseNotesTemplate"/> and
        /// <see cref="UpdateAvailableWindowViewModel.GetDefaultAdditionalHeaderHTML"/> and pass those
        /// in to your ReleaseNotesGrabber subclass.
        /// </para>
        /// </summary>
        public ReleaseNotesGrabber ReleaseNotesGrabberOverride { get; set; } = null;

        /// <summary>
        /// A delegate for handling windows that are created by a <see cref="UIFactory"/>
        /// </summary>
        /// <param name="window"><see cref="Window"/> that has been created and initialized (with view model, if applicable)</param>
        /// <param name="factory"><see cref="UIFactory"/> that created the given <see cref="Window"/></param>
        public delegate void WindowHandler(Window window, UIFactory factory);

        /// <summary>
        /// Set this property to manually do any other setup on a <see cref="Window"/> after it has been created by the <see cref="UIFactory"/>.
        /// Can be used to tweak view models, change styles on the <see cref="Window"/>, etc.
        /// </summary>
        public WindowHandler ProcessWindowAfterInit { get; set; }

        /// <inheritdoc/>
        public virtual IUpdateAvailable CreateUpdateAvailableWindow(SparkleUpdater sparkle, List<AppCastItem> updates, bool isUpdateAlreadyDownloaded = false)
        {
            var viewModel = new UpdateAvailableWindowViewModel();
            var window = new UpdateAvailableWindow(viewModel, _iconBitmap)
            {
                Icon = _applicationIcon
            };
            if (UseStaticUpdateWindowBackgroundColor)
            {
                window.ChangeMainGridBackgroundColor(UpdateWindowGridBackgroundBrush);
            }
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
            if (ReleaseNotesGrabberOverride != null)
            {
                viewModel.ReleaseNotesGrabber = ReleaseNotesGrabberOverride;
            }
            viewModel.Initialize(sparkle, updates, isUpdateAlreadyDownloaded, ReleaseNotesHTMLTemplate, AdditionalReleaseNotesHeaderHTML, ReleaseNotesDateTimeFormat);
            ProcessWindowAfterInit?.Invoke(window, this);
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
            var window = new DownloadProgressWindow(viewModel, _iconBitmap)
            {
                Icon = _applicationIcon
            };
            ProcessWindowAfterInit?.Invoke(window, this);
            return window;
        }

        /// <inheritdoc/>
        public virtual ICheckingForUpdates ShowCheckingForUpdates(SparkleUpdater sparkle)
        {
            var window = new CheckingForUpdatesWindow(_iconBitmap)
            { 
                Icon = _applicationIcon
            };
            ProcessWindowAfterInit?.Invoke(window, this);
            return window;
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
            ProcessWindowAfterInit?.Invoke(messageWindow, this);
            messageWindow.Show(); // TODO: This was ShowDialog; will this break anything?
        }

        /// <inheritdoc/>
        public void Shutdown(SparkleUpdater sparkle)
        {
            (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
        }
    }
}
