using NetSparkle.Enums;
using NetSparkle.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetSparkle.UI.NetFramework.WPF
{
    /// <summary>
    /// Interaction logic for UpdateAvailableWindow.xaml
    /// </summary>
    public partial class UpdateAvailableWindow : Window, IUpdateAvailable
    {
        private Sparkle _sparkle;
        private AppCastItem[] _updates;
        private ReleaseNotesGrabber _releaseNotesGrabber;

        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        private bool _isOnMainThread;
        private bool _hasInitiatedShutdown;

        private UpdateAvailableResult _userResponse;

        public UpdateAvailableWindow()
        {
            InitializeComponent();
            Closing += UpdateAvailableWindow_Closing;
            _userResponse = UpdateAvailableResult.None;
        }

        private void UpdateAvailableWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= UpdateAvailableWindow_Closing;
            if (!_isOnMainThread && !_hasInitiatedShutdown)
            {
                _hasInitiatedShutdown = true;
                Dispatcher.InvokeShutdown();
            }
        }

        public void Initialize(Sparkle sparkle, AppCastItem[] items, bool isUpdateAlreadyDownloaded = false,
            string separatorTemplate = "", string headAddition = "")
        {
            _sparkle = sparkle;
            _updates = items;

            _releaseNotesGrabber = new ReleaseNotesGrabber(separatorTemplate, headAddition, sparkle);

            ReleaseNotesBrowser.AllowDrop = false;

            AppCastItem item = items.FirstOrDefault();

            // TODO: string translations
            TitleHeader.Text = string.Format("A new version of {0} is available.", item?.AppName ?? "the application");
            var downloadInstallText = isUpdateAlreadyDownloaded ? "install" : "download";
            if (item != null)
            {
                var versionString = "";
                try
                {
                    // Use try/catch since Version constructor can throw an exception and we don't want to
                    // die just because the user has a malformed version string
                    Version versionObj = new Version(item.AppVersionInstalled);
                    versionString = NetSparkle.Utilities.GetVersionString(versionObj);
                }
                catch
                {
                    versionString = "?";
                }
                InfoText.Text = string.Format("{0} is now available (you have {1}). Would you like to {2} it now?", item.AppName, versionString, downloadInstallText);
            }
            else
            {
                InfoText.Text = string.Format("Would you like to {0} it now?", downloadInstallText);
            }

            bool isUserMissingCriticalUpdate = items.Any(x => x.IsCriticalUpdate);
            RemindMeLaterButton.IsEnabled = isUserMissingCriticalUpdate == false;
            SkipButton.IsEnabled = isUserMissingCriticalUpdate == false;

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            ReleaseNotesBrowser.NavigateToString(_releaseNotesGrabber.GetLoadingText());
            LoadReleaseNotes(items);
        }

        private async void LoadReleaseNotes(AppCastItem[] items)
        {
            AppCastItem latestVersion = items.OrderByDescending(p => p.Version).FirstOrDefault();
            string releaseNotes = await _releaseNotesGrabber.DownloadAllReleaseNotesAsHTML(items, latestVersion, _cancellationToken);
            ReleaseNotesBrowser.Dispatcher.Invoke(() =>
            {
                // see https://stackoverflow.com/a/15209861/3938401
                ReleaseNotesBrowser.NavigateToString(releaseNotes);
            });
        }

        UpdateAvailableResult IUpdateAvailable.Result => _userResponse;
        // actually the result should be sent back in the UserResponded event! that would simplify the event handling a lot)

        AppCastItem IUpdateAvailable.CurrentItem => _updates.Count() > 0 ? _updates[0] : null;

        public event EventHandler UserResponded;

        void IUpdateAvailable.BringToFront()
        {
            Topmost = true;
            Activate();
            Topmost = false;
        }

        void IUpdateAvailable.Close()
        {
            Close();
            if (!_isOnMainThread && !_hasInitiatedShutdown)
            {
                _hasInitiatedShutdown = true;
                Dispatcher.InvokeShutdown();
            }
        }

        void IUpdateAvailable.HideReleaseNotes()
        {
            ReleaseNotesBrowser.Visibility = Visibility.Collapsed;
            // TODO: resize window to account for no release notes being shown
        }

        void IUpdateAvailable.HideRemindMeLaterButton()
        {
            RemindMeLaterButton.Visibility = Visibility.Collapsed; // TODO: Binding instead of direct property setting (#70)
        }

        void IUpdateAvailable.HideSkipButton()
        {
            SkipButton.Visibility = Visibility.Collapsed; // TODO: Binding instead of direct property setting (#70)
        }

        void IUpdateAvailable.Show(bool IsOnMainThread)
        {
            try
            {
                Show();
                _isOnMainThread = IsOnMainThread;
                if (!IsOnMainThread)
                {
                    // https://stackoverflow.com/questions/1111369/how-do-i-create-and-show-wpf-windows-on-separate-threads
                    System.Windows.Threading.Dispatcher.Run();
                }
            }
            catch (ThreadAbortException)
            {
                Close();
                if (!IsOnMainThread)
                {
                    Dispatcher.InvokeShutdown();
                }
            }
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            _userResponse = UpdateAvailableResult.SkipUpdate;
            UserResponded?.Invoke(this, new EventArgs());
        }

        private void RemindMeLaterButton_Click(object sender, RoutedEventArgs e)
        {
            _userResponse = UpdateAvailableResult.RemindMeLater;
            UserResponded?.Invoke(this, new EventArgs());
        }

        private void DownloadInstallButton_Click(object sender, RoutedEventArgs e)
        {
            _userResponse = UpdateAvailableResult.InstallUpdate;
            UserResponded?.Invoke(this, new EventArgs());
        }
    }
}
