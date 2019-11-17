using NetSparkle.Events;
using NetSparkle.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// Interaction logic for DownloadProgressWindow.xaml
    /// </summary>
    public partial class DownloadProgressWindow : Window, IDownloadProgress
    {
        private AppCastItem _itemToDownload;
        private bool _isDownloading;
        private bool _didDownloadAnything;
        private bool _didCallDownloadProcessCompletedHandler = false;

        private bool _isOnMainThread;
        private bool _hasInitiatedShutdown;

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
                _didCallDownloadProcessCompletedHandler = true;
                DownloadProcessCompleted?.Invoke(this, new DownloadInstallArgs(false));
            }
            Closing -= DownloadProgressWindow_Closing;
            if (!_isOnMainThread && !_hasInitiatedShutdown)
            {
                _hasInitiatedShutdown = true;
                Dispatcher.InvokeShutdown();
            }
        }

        public AppCastItem ItemToDownload 
        { 
            get { return _itemToDownload; }
            set 
            {
                _itemToDownload = value;

                Dispatcher.InvokeAsync(() =>
                {
                    if (value != null)
                    {
                        DownloadingTitle.Content = string.Format("Downloading {0}", _itemToDownload.AppName + " " + _itemToDownload.Version);
                    }
                    else
                    {
                        DownloadingTitle.Content = string.Format("Downloading...");
                    }
                });
            }
        }

        /// <summary>
        /// Event to fire when the download UI is complete; tells you 
        /// if the install process should happen or not
        /// </summary>
        public event DownloadInstallEventHandler DownloadProcessCompleted;

        bool IDownloadProgress.DisplayErrorMessage(string errorMessage)
        {
            Dispatcher.InvokeAsync(() =>
            {
                ErrorMessage.Text = errorMessage;
                ErrorMessage.Visibility = Visibility.Visible;
            });
            return true;
        }

        void IDownloadProgress.FinishedDownloadingFile(bool isDownloadedFileValid)
        {
            _isDownloading = false;

            Dispatcher.InvokeAsync(() =>
            {
                ProgressBar.Value = 100;
                if (!_didDownloadAnything)
                {
                    DownloadProgress.Content = string.Format("(- / -)");
                }
                ActionButton.Content = "Install and Relaunch";
            });
        }

        void IDownloadProgress.Close()
        {
            Dispatcher.InvokeAsync(() =>
            {
                Close();
                if (!_isOnMainThread && !_hasInitiatedShutdown)
                {
                    _hasInitiatedShutdown = true;
                    Dispatcher.InvokeShutdown();
                }
            });
        }

        /// <summary>
        /// Event called when the client download progress changes
        /// </summary>
        private void OnDownloadProgressChanged(object sender, long bytesReceived, long totalBytesToReceive, int percentage)
        {
            Dispatcher.InvokeAsync(() =>
            {
                ProgressBar.Value = percentage;
                DownloadProgress.Content = string.Format("({0} / {1})",
                    Utilities.NumBytesToUserReadableString(bytesReceived),
                    Utilities.NumBytesToUserReadableString(totalBytesToReceive));
            });
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

        void IDownloadProgress.Show(bool isOnMainThread)
        {
            try
            {
                Show();
                _isOnMainThread = isOnMainThread;
                if (!isOnMainThread)
                {
                    // https://stackoverflow.com/questions/1111369/how-do-i-create-and-show-wpf-windows-on-separate-threads
                    System.Windows.Threading.Dispatcher.Run();
                }
            }
            catch (ThreadAbortException)
            {
                Close();
                if (!isOnMainThread)
                {
                    Dispatcher.InvokeShutdown();
                }
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            _didCallDownloadProcessCompletedHandler = true;
            DownloadProcessCompleted?.Invoke(this, new DownloadInstallArgs(!_isDownloading));
        }
    }
}
