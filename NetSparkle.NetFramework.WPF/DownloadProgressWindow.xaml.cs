using NetSparkle.Events;
using NetSparkle.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
    /// Interaction logic for DownloadProgressWindow.xaml
    /// </summary>
    public partial class DownloadProgressWindow : Window, IDownloadProgress
    {
        private AppCastItem _itemToDownload;
        private bool _isDownloading;
        private bool _didDownloadAnything;
        private bool _didCallDownloadProcessCompletedHandler = false;

        public DownloadProgressWindow()
        {
            InitializeComponent();
            _isDownloading = true;
            _didDownloadAnything = false;
            ErrorMessage.Text = "";
            Closing += DownloadProgressWindow_Closing;
        }

        private void DownloadProgressWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_didCallDownloadProcessCompletedHandler)
            {
                DownloadProcessCompleted?.Invoke(this, new DownloadInstallArgs(false));
            }
        }

        public AppCastItem ItemToDownload 
        { 
            get { return _itemToDownload; }
            set 
            {
                _itemToDownload = value;
                if (value != null)
                {
                    DownloadingTitle.Content = string.Format("Downloading {0}", _itemToDownload.AppName + " " + _itemToDownload.Version);
                }
                else
                {
                    DownloadingTitle.Content = string.Format("Downloading...");
                }
            }
        }

        /// <summary>
        /// Event to fire when the download UI is complete; tells you 
        /// if the install process should happen or not
        /// </summary>
        public event DownloadInstallEventHandler DownloadProcessCompleted;

        bool IDownloadProgress.DisplayErrorMessage(string errorMessage)
        {
            ErrorMessage.Text = errorMessage;
            ErrorMessage.Visibility = Visibility.Visible;
            return true;
        }

        void IDownloadProgress.FinishedDownloadingFile(bool isDownloadedFileValid)
        {
            _isDownloading = false;
            ProgressBar.Value = 100;
            if (!_didDownloadAnything)
            {
                DownloadProgress.Content = string.Format("(- / -)");
            }
            ActionButton.Content = "Install and Relaunch";
        }

        void IDownloadProgress.Close()
        {
            DialogResult = false;
        }

        /// <summary>
        /// Event called when the client download progress changes
        /// </summary>
        private void OnDownloadProgressChanged(object sender, long bytesReceived, long totalBytesToReceive, int percentage)
        {
            ProgressBar.Value = percentage;
            DownloadProgress.Content = string.Format("({0} / {1})",
                Utilities.NumBytesToUserReadableString(bytesReceived),
                Utilities.NumBytesToUserReadableString(totalBytesToReceive));
        }

        void IDownloadProgress.OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _didDownloadAnything = true;
            OnDownloadProgressChanged(sender, e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
        }

        void IDownloadProgress.SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
        {
            ActionButton.IsEnabled = shouldBeEnabled;
        }

        void IDownloadProgress.Show()
        {
            Show();
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = !_isDownloading;
            _didCallDownloadProcessCompletedHandler = true;
            DownloadProcessCompleted?.Invoke(this, new DownloadInstallArgs(!_isDownloading));
        }
    }
}
