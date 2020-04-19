using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.UI.WPF.Controls;
using NetSparkleUpdater.UI.WPF.ViewModels;
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

namespace NetSparkleUpdater.UI.WPF
{
    /// <summary>
    /// Interaction logic for DownloadProgressWindow.xaml
    /// </summary>
    public partial class DownloadProgressWindow : BaseWindow, IDownloadProgress
    {
        private bool _didCallDownloadProcessCompletedHandler = false;

        private DownloadProgressWindowViewModel _dataContext;

        public DownloadProgressWindow() : base(false)
        {
            InitializeComponent();
            Closing += DownloadProgressWindow_Closing;
        }

        public DownloadProgressWindow(DownloadProgressWindowViewModel viewModel)
        {
            InitializeComponent();
            Closing += DownloadProgressWindow_Closing;
            DataContext = _dataContext = viewModel;
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

        /// <summary>
        /// Event to fire when the download UI is complete; tells you 
        /// if the install process should happen or not
        /// </summary>
        public event DownloadInstallEventHandler DownloadProcessCompleted;

        bool IDownloadProgress.DisplayErrorMessage(string errorMessage)
        {
            if (_dataContext != null)
            {
                _dataContext.ErrorMessageText = errorMessage;
                _dataContext.IsErrorMessageVisible = true;
            }
            return true;
        }

        void IDownloadProgress.FinishedDownloadingFile(bool isDownloadedFileValid)
        {
            _dataContext?.SetFinishedDownloading(isDownloadedFileValid);
            if (!isDownloadedFileValid)
            {
                Dispatcher.Invoke(() =>
                {
                    this.Background = new SolidColorBrush(Colors.Tomato);
                });
            }
        }

        void IDownloadProgress.Close()
        {
            CloseWindow();
        }

        /// <summary>
        /// Event called when the client download progress changes
        /// </summary>
        private void OnDownloadProgressChanged(object sender, long bytesReceived, long totalBytesToReceive, int percentage)
        {
            _dataContext?.UpdateProgress(bytesReceived, totalBytesToReceive, percentage);
        }

        void IDownloadProgress.OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnDownloadProgressChanged(sender, e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
        }

        void IDownloadProgress.SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
        {
            ActionButton.IsEnabled = shouldBeEnabled;
        }

        void IDownloadProgress.Show(bool isOnMainThread)
        {
            ShowWindow(isOnMainThread);
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            _didCallDownloadProcessCompletedHandler = true;
            DownloadProcessCompleted?.Invoke(this, new DownloadInstallArgs(!(_dataContext?.IsDownloading ?? true)));
        }
    }
}
