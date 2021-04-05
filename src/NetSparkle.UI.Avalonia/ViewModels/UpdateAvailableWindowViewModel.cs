using NetSparkleUpdater.Enums;
using NetSparkleUpdater.UI.Avalonia.Helpers;
using NetSparkleUpdater.UI.Avalonia.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace NetSparkleUpdater.UI.Avalonia.ViewModels
{
    /// <summary>
    /// View model for views that detail updates that are available for a given user
    /// </summary>
    public class UpdateAvailableWindowViewModel : ChangeNotifier
    {
        private SparkleUpdater _sparkle;
        private List<AppCastItem> _updates;

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

        /// <summary>
        /// Initialize the view model with release notes, remind me later,
        /// and skip button being visible by default.
        /// </summary>
        public UpdateAvailableWindowViewModel()
        {
            _updates = new List<AppCastItem>();
            _areReleaseNotesVisible = true;
            _isRemindMeLaterVisible = true;
            _isSkipVisible = true;
            _userResponse = UpdateAvailableResult.None;
        }

        /// <summary>
        /// Interface object for the object that will be displaying the release notes
        /// </summary>
        public IReleaseNotesDisplayer ReleaseNotesDisplayer { get; set; }

        /// <summary>
        /// Interface object for the object that can handle user responses to the update that
        /// is being shown to the user
        /// </summary>
        public IUserRespondedToUpdateCheck UserRespondedHandler { get; set; }

        /// <summary>
        /// Object responsible for downloading and formatting markdown release notes for display in HTML
        /// </summary>
        public ReleaseNotesGrabber ReleaseNotesGrabber { get; set; }

        /// <summary>
        /// Header text to show to the user. Usually something along the lines of
        /// "A new version of {software} is available!"
        /// </summary>
        public string TitleHeaderText
        {
            get => _titleHeaderText;
            set { _titleHeaderText = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Some additional text to show to the user underneath the header. This is text like
        /// "Version {version} is now available. Do you want to download it?"
        /// </summary>
        public string InfoText
        {
            get => _infoText;
            set { _infoText = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Whether or not the "Remind Me Later" button is enabled
        /// </summary>
        public bool IsRemindMeLaterEnabled
        {
            get => _isRemindMeLaterEnabled;
            set { _isRemindMeLaterEnabled = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Whether or not the "Skip this Update" button is enabled
        /// </summary>
        public bool IsSkipEnabled
        {
            get => _isSkipEnabled;
            set { _isSkipEnabled = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// The list of updates that are available for downloading by the user
        /// </summary>
        public List<AppCastItem> Updates
        {
            get => _updates;
        }

        /// <summary>
        /// Whether or not the release notes for updates are shown to the user
        /// </summary>
        public bool AreReleaseNotesVisible
        {
            get => _areReleaseNotesVisible;
            set { _areReleaseNotesVisible = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Whether or not the "Remind Me Later" button is visible
        /// </summary>
        public bool IsRemindMeLaterVisible
        {
            get => _isRemindMeLaterVisible;
            set { _isRemindMeLaterVisible = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Whether or not the "Skip this Update" button is visible
        /// </summary>
        public bool IsSkipVisible
        {
            get => _isSkipVisible;
            set { _isSkipVisible = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// The response that the user gave to the update
        /// </summary>
        public UpdateAvailableResult UserResponse
        {
            get => _userResponse;
        }

        /// <summary>
        /// Command to execute when the user wants to skip this update
        /// </summary>
        public ICommand Skip => new RelayCommand(PerformSkip);
        /// <summary>
        /// Command to execute when the user wants to be reminded about this update later
        /// </summary>
        public ICommand RemindMeLater => new RelayCommand(PerformRemindMeLater);
        /// <summary>
        /// Command to execute when the user wants to download and install the update right away
        /// </summary>
        public ICommand DownloadInstall => new RelayCommand(PerformDownloadInstall);

        /// <summary>
        /// Skip updating the software
        /// </summary>
        public void PerformSkip()
        {
            SendResponse(UpdateAvailableResult.SkipUpdate);
        }

        /// <summary>
        /// Hide the update window and show it to the user at another time
        /// </summary>
        public void PerformRemindMeLater()
        {
            SendResponse(UpdateAvailableResult.RemindMeLater);
        }

        /// <summary>
        /// Start downloading the update so that the user can install it
        /// </summary>
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

        /// <summary>
        /// Initialize this view model with its necessary items so that it can display things
        /// to the user properly
        /// </summary>
        /// <param name="sparkle">The <seealso cref="SparkleUpdater"/> item that is currently checking for updates</param>
        /// <param name="items">The list of <seealso cref="AppCastItem"/> updates that are available for the user</param>
        /// <param name="isUpdateAlreadyDownloaded">Whether or not the update is already downloaded ot the user's computer</param>
        /// <param name="releaseNotesHTMLTemplate">The HTML string template to show in the release notes</param>
        /// <param name="additionalReleaseNotesHeaderHTML">The HTML string to add into the head element of the HTML for the release notes</param>
        public void Initialize(SparkleUpdater sparkle, List<AppCastItem> items, bool isUpdateAlreadyDownloaded = false,
            string releaseNotesHTMLTemplate = "", string additionalReleaseNotesHeaderHTML = "")
        {
            _sparkle = sparkle;
            _updates = items;

            ReleaseNotesGrabber = new ReleaseNotesGrabber(releaseNotesHTMLTemplate, additionalReleaseNotesHeaderHTML, sparkle);

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
                    versionString = NetSparkleUpdater.Utilities.GetVersionString(versionObj);
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

            ReleaseNotesDisplayer?.ShowReleaseNotes(ReleaseNotesGrabber.GetLoadingText());
            LoadReleaseNotes(items);
        }

        private async void LoadReleaseNotes(List<AppCastItem> items)
        {
            AppCastItem latestVersion = items.OrderByDescending(p => p.Version).FirstOrDefault();
            string releaseNotes = await ReleaseNotesGrabber.DownloadAllReleaseNotes(items, latestVersion, _cancellationToken);
            ReleaseNotesDisplayer?.ShowReleaseNotes(releaseNotes);
        }

        /// <summary>
        /// Cancel the download of release notes or other asynchronous operations taking place
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
