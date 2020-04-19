using System;
using System.Collections.Generic;
using System.Text;
using NetSparkleUpdater.UI.Avalonia.Helpers;
using NetSparkleUpdater.UI.Avalonia.Interfaces;

namespace NetSparkleUpdater.UI.Avalonia.ViewModels
{
    public class DownloadProgressWindowViewModel : ChangeNotifier
    {
        private AppCastItem _itemToDownload;

        private bool _isDownloading;
        private bool _didDownloadAnything;

        private string _errorMessageText;
        private bool _isErrorMessageVisible;

        private string _downloadingTitle;
        private double _downloadProgressValue;
        private string _userReadableDownloadProgress;

        private string _actionButtonTitle;
        private bool _isActionButtonVisible;

        public DownloadProgressWindowViewModel()
        {
            IsDownloading = true;
            DidDownloadAnything = false;
            ErrorMessageText = "";
            IsErrorMessageVisible = false;
            _downloadingTitle = "";
            _downloadProgressValue = 0.0;
            IsActionButtonVisible = false;
        }

        public AppCastItem ItemToDownload
        {
            get => _itemToDownload;
            set 
            { 
                _itemToDownload = value;
                NotifyPropertyChanged();

                if (value != null)
                {
                    DownloadingTitle = string.Format("Downloading {0}", _itemToDownload.AppName + " " + _itemToDownload.Version);
                }
                else
                {
                    DownloadingTitle = "Downloading...";
                }
            }
        }

        public bool IsDownloading
        {
            get => _isDownloading;
            set { _isDownloading = value; NotifyPropertyChanged(); }
        }

        public bool DidDownloadAnything
        {
            get => _didDownloadAnything;
            set { _didDownloadAnything = value; NotifyPropertyChanged(); }
        }

        public string ErrorMessageText
        {
            get => _errorMessageText;
            set { _errorMessageText = value; NotifyPropertyChanged(); }
        }

        public bool IsErrorMessageVisible
        {
            get => _isErrorMessageVisible;
            set { _isErrorMessageVisible = value; NotifyPropertyChanged(); }
        }

        public string DownloadingTitle
        {
            get => _downloadingTitle;
            set { _downloadingTitle = value; NotifyPropertyChanged(); }
        }

        public double DownloadProgress
        {
            get => _downloadProgressValue;
            set { _downloadProgressValue = value; NotifyPropertyChanged(); }
        }

        public string UserReadableDownloadProgress
        {
            get => _userReadableDownloadProgress;
            set { _userReadableDownloadProgress = value; NotifyPropertyChanged(); }
        }

        public string ActionButtonTitle
        {
            get => _actionButtonTitle;
            set { _actionButtonTitle = value; NotifyPropertyChanged(); }
        }

        public bool IsActionButtonVisible
        {
            get => _isActionButtonVisible;
            set { _isActionButtonVisible = value; NotifyPropertyChanged(); }
        }

        public void SetFinishedDownloading(bool isInstallFileValid)
        {
            IsDownloading = false;

            DownloadProgress = 100;
            if (!_didDownloadAnything)
            {
                UserReadableDownloadProgress = string.Format("");
            }
            if (isInstallFileValid)
            {
                IsActionButtonVisible = true;
                ActionButtonTitle = "Install and Relaunch";
            }
            else
            {
                IsActionButtonVisible = false;
            }
        }

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
