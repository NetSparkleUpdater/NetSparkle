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

namespace NetSparkle.NetFramework.WPF
{
    /// <summary>
    /// Interaction logic for DownloadProgressWindow.xaml
    /// </summary>
    public partial class DownloadProgressWindow : Window, IDownloadProgress
    {
        private AppCastItem _itemToDownload;
        private bool _isDownloading;

        public DownloadProgressWindow()
        {
            InitializeComponent();
            _isDownloading = true;
            ErrorMessage.Text = "";
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

        public event EventHandler InstallAndRelaunch;

        bool IDownloadProgress.DisplayErrorMessage(string errorMessage)
        {
            ErrorMessage.Text = errorMessage;
            ErrorMessage.Visibility = Visibility.Visible;
            return true;
        }

        void IDownloadProgress.FinishedDownloadingFile(bool isDownloadedFileValid)
        {
            _isDownloading = false;
            ActionButton.Content = "Install and Relaunch";
        }

        void IDownloadProgress.ForceClose()
        {
            DialogResult = false;
            Close();
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
            OnDownloadProgressChanged(sender, e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
        }

        void IDownloadProgress.SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
        {
            ActionButton.IsEnabled = shouldBeEnabled;
        }

        bool IDownloadProgress.ShowDialog()
        {
            return (bool)ShowDialog();
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isDownloading)
            {
                DialogResult = false;
                Close();
            }
            else
            {
                InstallAndRelaunch?.Invoke(sender, new EventArgs());
                DialogResult = true;
                Close();
            }
        }
    }
}
