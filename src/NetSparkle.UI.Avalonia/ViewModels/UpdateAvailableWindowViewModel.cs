using NetSparkle.Enums;
using NetSparkle.UI.Avalonia.Helpers;
using NetSparkle.UI.Avalonia.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace NetSparkle.UI.Avalonia.ViewModels
{
    public class UpdateAvailableWindowViewModel : ChangeNotifier
    {
        private Sparkle _sparkle;
        private List<AppCastItem> _updates;
        private ReleaseNotesGrabber _releaseNotesGrabber;

        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        private string _titleHeaderText;
        private string _infoText;

        private bool _isRemindMeLaterEnabled;
        private bool _isSkipEnabled;

        private bool _areReleaseNotesVisible;
        private bool _isRemindMeLaterVisible;
        private bool _isSkipVisible;

        private UpdateAvailableResult _userResponse;

        public UpdateAvailableWindowViewModel()
        {
            _updates = new List<AppCastItem>();
            _areReleaseNotesVisible = true;
            _isRemindMeLaterVisible = true;
            _isSkipVisible = true;
            _userResponse = UpdateAvailableResult.None;
        }

        public IReleaseNotesUpdater ReleaseNotesUpdater { get; set; }
        public IUserRespondedToUpdateCheck UserRespondedHandler { get; set; }

        public string TitleHeaderText
        {
            get => _titleHeaderText;
            set { _titleHeaderText = value; NotifyPropertyChanged(); }
        }

        public string InfoText
        {
            get => _infoText;
            set { _infoText = value; NotifyPropertyChanged(); }
        }

        public bool IsRemindMeLaterEnabled
        {
            get => _isRemindMeLaterEnabled;
            set { _isRemindMeLaterEnabled = value; NotifyPropertyChanged(); }
        }

        public bool IsSkipEnabled
        {
            get => _isSkipEnabled;
            set { _isSkipEnabled = value; NotifyPropertyChanged(); }
        }

        public List<AppCastItem> Updates
        {
            get => _updates;
        }

        public bool AreReleaseNotesVisible
        {
            get => _areReleaseNotesVisible;
            set { _areReleaseNotesVisible = value; NotifyPropertyChanged(); }
        }

        public bool IsRemindMeLaterVisible
        {
            get => _isRemindMeLaterVisible;
            set { _isRemindMeLaterVisible = value; NotifyPropertyChanged(); }
        }

        public bool IsSkipVisible
        {
            get => _isSkipVisible;
            set { _isSkipVisible = value; NotifyPropertyChanged(); }
        }

        public UpdateAvailableResult UserResponse
        {
            get => _userResponse;
        }

        public ICommand Skip => new RelayCommand(PerformSkip);
        public ICommand RemindMeLater => new RelayCommand(PerformRemindMeLater);
        public ICommand DownloadInstall => new RelayCommand(PerformDownloadInstall);

        public void PerformSkip()
        {
            SendResponse(UpdateAvailableResult.SkipUpdate);
        }
        public void PerformRemindMeLater()
        {
            SendResponse(UpdateAvailableResult.RemindMeLater);
        }
        public void PerformDownloadInstall()
        {
            SendResponse(UpdateAvailableResult.InstallUpdate);
        }

        private void SendResponse(UpdateAvailableResult response)
        {
            _userResponse = response;
            UserRespondedHandler?.UserRespondedToUpdateCheck(response);
            Cancel();
        }

        public void Initialize(Sparkle sparkle, List<AppCastItem> items, bool isUpdateAlreadyDownloaded = false,
            string separatorTemplate = "", string headAddition = "")
        {
            _sparkle = sparkle;
            _updates = items;

            _releaseNotesGrabber = new ReleaseNotesGrabber(separatorTemplate, headAddition, sparkle);

            AppCastItem item = items.FirstOrDefault();

            // TODO: string translations
            TitleHeaderText = string.Format("A new version of {0} is available.", item?.AppName ?? "the application");
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
                InfoText = string.Format("{0} {3} is now available (you have {1}). Would you like to {2} it now?", item.AppName, versionString,
                    downloadInstallText, item.Version);
            }
            else
            {
                InfoText = string.Format("Would you like to {0} it now?", downloadInstallText);
            }

            bool isUserMissingCriticalUpdate = items.Any(x => x.IsCriticalUpdate);
            IsRemindMeLaterEnabled = isUserMissingCriticalUpdate == false;
            IsSkipEnabled = isUserMissingCriticalUpdate == false;

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            ReleaseNotesUpdater?.ShowReleaseNotes(_releaseNotesGrabber.GetLoadingText());
            LoadReleaseNotes(items);
        }

        private async void LoadReleaseNotes(List<AppCastItem> items)
        {
            AppCastItem latestVersion = items.OrderByDescending(p => p.Version).FirstOrDefault();
            string releaseNotes = await _releaseNotesGrabber.DownloadAllReleaseNotesAsHTML(items, latestVersion, _cancellationToken);
            ReleaseNotesUpdater?.ShowReleaseNotes(releaseNotes);
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
