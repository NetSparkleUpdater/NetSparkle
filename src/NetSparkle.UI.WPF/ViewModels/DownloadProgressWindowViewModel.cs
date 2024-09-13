using NetSparkleUpdater.UI.WPF.Helpers;
using System.Windows.Media;

namespace NetSparkleUpdater.UI.WPF.ViewModels
{
    /// <summary>
    /// View model for the DownloadProgressWindow (which displays
    /// downloading progress for an app update along with any 
    /// error message that occurs).
    /// This view model does not actually control the download of the
    /// NetSparkleUpdater <see cref="AppCastItem"/>.
    /// </summary>
    public class DownloadProgressWindowViewModel : ChangeNotifier
    {
        private bool _isDownloading;
        private bool _didDownloadAnything;
        private bool _didDownloadFail;
        private string _downloadingTitle;

        private string _errorMessageText;
        private bool _isErrorMessageVisible;

        private double _downloadProgressValue;
        private string _userReadableDownloadProgress;

        private string _actionButtonTitle;
        private bool _isActionButtonVisible;

        private Brush _background;

        /// <summary>
        /// Default constructor for the view model that sets
        /// everything up for use
        /// </summary>
        public DownloadProgressWindowViewModel()
        {
            IsDownloading = true;
            DidDownloadAnything = false;
            _errorMessageText = "";
            IsErrorMessageVisible = false;
            _userReadableDownloadProgress = "";
            _downloadingTitle = "Downloading...";
            _actionButtonTitle = "Cancel";
            _downloadProgressValue = 0.0;
            IsActionButtonVisible = true;
            _background = new SolidColorBrush(Colors.Transparent);
        }

        /// <summary>
        /// Whether or not the app update is downloading right now or not
        /// </summary>
        public bool IsDownloading
        {
            get => _isDownloading;
            set { _isDownloading = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(ActionButtonTitle)); }
        }

        /// <summary>
        /// Whether or not the app updater was able to download anything at all
        /// (even if the download ultimately failed)
        /// </summary>
        public bool DidDownloadAnything
        {
            get => _didDownloadAnything;
            set { _didDownloadAnything = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Whether or not the download failed (e.g. due to signature validation)
        /// </summary>
        public bool DidDownloadFail
        {
            get => _didDownloadFail;
        }

        /// <summary>
        /// Error message text to show to the user. Shown or not shown to the
        /// user depending on the value of <see cref="IsErrorMessageVisible"/>
        /// </summary>
        public string ErrorMessageText
        {
            get => _errorMessageText;
            set { _errorMessageText = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Whether or not to show the <see cref="ErrorMessageText"/> to the
        /// user or not
        /// </summary>
        public bool IsErrorMessageVisible
        {
            get => _isErrorMessageVisible;
            set { _isErrorMessageVisible = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Title to show to the user (e.g. "Downloading MyApp 1.0...").
        /// </summary>
        public string DownloadingTitle
        {
            get => _downloadingTitle;
            set { _downloadingTitle = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Progress for the download as a number between 0 and 100, inclusive
        /// </summary>
        public double DownloadProgress
        {
            get => _downloadProgressValue;
            set { _downloadProgressValue = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// A user-readable string that describes the download progress for
        /// showing to the user. This is updated via <see cref="UpdateProgress(long, long, int)"/>
        /// and shows the number of bytes downloaded out of the total number of bytes to download.
        /// </summary>
        public string UserReadableDownloadProgress
        {
            get => _userReadableDownloadProgress;
            set { _userReadableDownloadProgress = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Title to show on the single action button (e.g. "Cancel")
        /// </summary>
        public string ActionButtonTitle
        {
            get => _isDownloading || _didDownloadFail ? "Cancel" : _actionButtonTitle;
            set { _actionButtonTitle = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Whether or not the action button is visible to the user
        /// </summary>
        public bool IsActionButtonVisible
        {
            get => _isActionButtonVisible;
            set { _isActionButtonVisible = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Background color of download window
        /// </summary>
        public Brush BackgroundColor
        {
            get => _background;
            set { _background = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Change whether or not the <see cref="AppCastItem"/> download file has finished downloading
        /// </summary>
        /// <param name="isInstallFileValid">true if the download file has finished downloading;
        /// false otherwise</param>
        public void SetFinishedDownloading(bool isInstallFileValid)
        {
            IsDownloading = false;

            DownloadProgress = 100;
            if (!_didDownloadAnything)
            {
                UserReadableDownloadProgress = string.Format("");
            }
            IsActionButtonVisible = isInstallFileValid;
            if (!isInstallFileValid)
            {
                BackgroundColor = new SolidColorBrush(Colors.Tomato);
                _didDownloadFail = true;
                NotifyPropertyChanged(nameof(ActionButtonTitle));
            }
        }

        /// <summary>
        /// Update the progress held in this view model based on the bytes that have been downloaded
        /// and the total number of bytes that need to be downloaded
        /// </summary>
        /// <param name="bytesReceived">Number of bytes that have been downloaded</param>
        /// <param name="totalBytesToReceive">Number of bytes that have to be downloaded</param>
        /// <param name="percentage">Number between 0-100 that represents how much of the download has been completed</param>
        public void UpdateProgress(long bytesReceived, long totalBytesToReceive, int percentage)
        {
            DidDownloadAnything = true;
            DownloadProgress = percentage;
            UserReadableDownloadProgress = string.Format("({0} / {1})",
                Utilities.ConvertNumBytesToUserReadableString(bytesReceived),
                Utilities.ConvertNumBytesToUserReadableString(totalBytesToReceive));
        }

    }
}
