using System;
using System.Drawing;
using System.Windows.Forms;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.Properties;
using NetSparkleUpdater.Enums;
using System.Threading;
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Threading.Tasks;

namespace NetSparkleUpdater.UI.WinForms
{
    /// <summary>
    /// UI factory for WinForms .NET Core interface.
    /// Note that it expects to be created on your main UI thread.
    /// </summary>
    public class UIFactory : IUIFactory
    {
        /// <summary>
        /// Icon used on various windows shown by NetSparkleUpdater
        /// </summary>
        protected Icon? _applicationIcon = null;
        private SynchronizationContext _syncContext;

        /// <inheritdoc/>
        public UIFactory()
        {
            HideReleaseNotes = false;
            HideRemindMeLaterButton = false;
            HideSkipButton = false;
            _syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            // enable visual style to ensure that we have XP style or higher
            Application.EnableVisualStyles();
        }

        /// <inheritdoc/>
        public UIFactory(Icon? applicationIcon) : this()
        {
            _applicationIcon = applicationIcon;
        }

        /// <inheritdoc/>
        public bool HideReleaseNotes { get; set; }

        /// <inheritdoc/>
        public bool HideSkipButton { get; set; }

        /// <inheritdoc/>
        public bool HideRemindMeLaterButton { get; set; }

        /// <inheritdoc/>
        public string? ReleaseNotesHTMLTemplate { get; set; }

        /// <inheritdoc/>
        public string? AdditionalReleaseNotesHeaderHTML { get; set; }

        /// <summary>
        /// The DateTime.ToString() format used when formatting dates to show in the release notes
        /// header. NetSparkle is not responsible for what happens if you send a bad format! :)
        /// </summary>
        public string ReleaseNotesDateTimeFormat { get; set; } = "D";

        /// <summary>
        /// <para>
        /// Easily set / override the ReleaseNotesGrabber used by the <see cref="UpdateAvailableWindow"/>.
        /// Note that this will NOT automatically use the <see cref="UIFactory"/> ReleaseNotesHTMLTemplate,
        /// AdditionalReleaseNotesHeaderHTML, and ReleaseNotesDateTimeFormat that you may have set on 
        /// the UIFactory - you must set these on this manual override yourself!
        /// </para>
        /// <para>
        /// Use this if you want to easily override the ReleaseNotesGrabber with your own subclass.
        /// </para>
        /// </summary>
        public ReleaseNotesGrabber? ReleaseNotesGrabberOverride { get; set; } = null;

        /// <summary>
        /// A delegate for handling forms that are created by a <see cref="UIFactory"/>
        /// </summary>
        /// <param name="form"><see cref="Form"/> that has been created and initialized</param>
        /// <param name="factory"><see cref="UIFactory"/> that created the given <see cref="Form"/></param>
        public delegate void FormHandler(Form form, UIFactory factory);

        /// <summary>
        /// Set this property to manually do any other setup on a <see cref="Form"/> after it has been created by the <see cref="UIFactory"/>.
        /// Can be used to tweak styles or perform other operations on the <see cref="Form"/>, etc.
        /// </summary>
        public FormHandler? ProcessFormAfterInit { get; set; }

        /// <inheritdoc/>
        public virtual IUpdateAvailable CreateUpdateAvailableWindow(List<AppCastItem> updates, ISignatureVerifier? signatureVerifier,
            string currentVersion = "", string appName = "the application", bool isUpdateAlreadyDownloaded = false)
        {
            var window = new UpdateAvailableWindow(updates, signatureVerifier, isUpdateAlreadyDownloaded, ReleaseNotesHTMLTemplate ?? "",
                AdditionalReleaseNotesHeaderHTML ?? "", ReleaseNotesDateTimeFormat, appName, currentVersion, _applicationIcon);
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
                window.ReleaseNotesGrabber = ReleaseNotesGrabberOverride;
            }
            window.Initialize();
            ProcessFormAfterInit?.Invoke(window, this);
            return window;
        }

        /// <inheritdoc/>
        public virtual IDownloadProgress CreateProgressWindow(string downloadTitle, string actionButtonTitleAfterDownload)
        {
            var window = new DownloadProgressWindow(downloadTitle, actionButtonTitleAfterDownload, _applicationIcon)
            {
            };
            ProcessFormAfterInit?.Invoke(window, this);
            return window;
        }

        /// <inheritdoc/>
        public virtual ICheckingForUpdates ShowCheckingForUpdates()
        {
            var window = new CheckingForUpdatesWindow(_applicationIcon);
            ProcessFormAfterInit?.Invoke(window, this);
            return window;
        }

        /// <inheritdoc/>
        public virtual void ShowUnknownInstallerFormatMessage(string downloadFileName)
        {
            ShowMessage(Resources.DefaultUIFactory_MessageTitle, 
                string.Format(Resources.DefaultUIFactory_ShowUnknownInstallerFormatMessageText, downloadFileName));
        }

        /// <inheritdoc/>
        public virtual void ShowVersionIsUpToDate()
        {
            ShowMessage(Resources.DefaultUIFactory_MessageTitle, Resources.DefaultUIFactory_ShowVersionIsUpToDateMessage);
        }

        /// <inheritdoc/>
        public virtual void ShowVersionIsSkippedByUserRequest()
        {
            ShowMessage(Resources.DefaultUIFactory_MessageTitle, Resources.DefaultUIFactory_ShowVersionIsSkippedByUserRequestMessage);
        }

        /// <inheritdoc/>
        public virtual void ShowCannotDownloadAppcast(string? appcastUrl)
        {
            ShowMessage(Resources.DefaultUIFactory_ErrorTitle, Resources.DefaultUIFactory_ShowCannotDownloadAppcastMessage);
        }

        /// <inheritdoc/>
        public virtual bool CanShowToastMessages()
        {
            return true;
        }

        /// <inheritdoc/>
        public virtual void ShowToast(Action clickHandler)
        {
            Thread thread = new Thread(() =>
            {
                var toast = new ToastNotifier(_applicationIcon)
                {
                    ClickAction = clickHandler,
                };
                ProcessFormAfterInit?.Invoke(toast, this);
                toast.Show(Resources.DefaultUIFactory_ToastMessage, Resources.DefaultUIFactory_ToastCallToAction, 5);
                Application.Run(toast);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <inheritdoc/>
        public virtual void ShowDownloadErrorMessage(string message, string? appcastUrl)
        {
            ShowMessage(Resources.DefaultUIFactory_ErrorTitle, string.Format(Resources.DefaultUIFactory_ShowDownloadErrorMessage, message));
        }

        private void ShowMessage(string title, string message)
        {
            var messageWindow = new MessageNotificationWindow(title, message, _applicationIcon);
            messageWindow.StartPosition = FormStartPosition.CenterScreen;
            ProcessFormAfterInit?.Invoke(messageWindow, this);
            messageWindow.ShowDialog();
        }

        /// <inheritdoc/>
        public void Shutdown()
        {
            Application.Exit();
        }

        #region --- Windows Forms Result Converters ---

        /// <summary>
        /// Method performs simple conversion of DialogResult to boolean.
        /// This method is a convenience when upgrading legacy code.
        /// </summary>
        /// <param name="dialogResult">WinForms DialogResult instance</param>
        /// <returns>Boolean based on dialog result</returns>
        public static bool ConvertDialogResultToDownloadProgressResult(DialogResult dialogResult)
        {
            return (dialogResult != DialogResult.Abort) && (dialogResult != DialogResult.Cancel);
        }

        /// <summary>
        /// Method performs simple conversion of DialogResult to UpdateAvailableResult.
        /// This method is a convenience when upgrading legacy code.
        /// </summary>
        /// <param name="dialogResult">WinForms DialogResult instance</param>
        /// <returns>Enumeration value based on dialog result</returns>
        public static UpdateAvailableResult ConvertDialogResultToUpdateAvailableResult(DialogResult dialogResult)
        {
            switch (dialogResult)
            {
                case DialogResult.Yes:
                    return UpdateAvailableResult.InstallUpdate;
                case DialogResult.No:
                    return UpdateAvailableResult.SkipUpdate;
                case DialogResult.Retry:
                case DialogResult.Cancel:
                    return UpdateAvailableResult.RemindMeLater;
            }

            return UpdateAvailableResult.None;
        }

        #endregion
    }
}
