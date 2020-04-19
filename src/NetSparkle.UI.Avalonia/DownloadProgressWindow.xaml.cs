using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.UI.Avalonia.Controls;
using NetSparkleUpdater.UI.Avalonia.ViewModels;
using System.Net;

namespace NetSparkleUpdater.UI.Avalonia
{
    public class DownloadProgressWindow : BaseWindow, IDownloadProgress
    {
        private bool _didCallDownloadProcessCompletedHandler = false;

        private DownloadProgressWindowViewModel _dataContext;

        private Button _actionButton;

        public DownloadProgressWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Closing += DownloadProgressWindow_Closing;
        }

        public DownloadProgressWindow(DownloadProgressWindowViewModel viewModel, IBitmap iconBitmap)
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Closing += DownloadProgressWindow_Closing;
            DataContext = _dataContext = viewModel;
            var imageControl = this.FindControl<Image>("AppIcon");
            if (imageControl != null)
            {
                imageControl.Source = iconBitmap;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _actionButton = this.FindControl<Button>("ActionButton");
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
                _cancellationTokenSource.Cancel();
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
                Dispatcher.UIThread.Post(() =>
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
            _actionButton.IsEnabled = shouldBeEnabled;
        }

        void IDownloadProgress.Show(bool isOnMainThread)
        {
            ShowWindow(isOnMainThread);
        }

        public void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            _didCallDownloadProcessCompletedHandler = true;
            DownloadProcessCompleted?.Invoke(this, new DownloadInstallArgs(!(_dataContext?.IsDownloading ?? true)));
        }
    }
}
