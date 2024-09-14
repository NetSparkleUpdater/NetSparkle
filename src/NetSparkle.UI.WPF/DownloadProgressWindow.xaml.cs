using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.UI.WPF.Controls;
using NetSparkleUpdater.UI.WPF.ViewModels;
using System.Windows;
using System.Windows.Media;

namespace NetSparkleUpdater.UI.WPF
{
    /// <summary>
    /// Interaction logic for DownloadProgressWindow.xaml.
    /// 
    /// Window that shows while SparkleUpdater is downloading the update
    /// for the user.
    /// </summary>
    public partial class DownloadProgressWindow : BaseWindow, IDownloadProgress
    {
        private bool _didCallDownloadProcessCompletedHandler = false;

        private DownloadProgressWindowViewModel? _dataContext;

        /// <summary>
        /// Base constructor for DownloadProgressWindow. Initializes the window
        /// and sets up the Closing event.
        /// </summary>
        public DownloadProgressWindow() : base(false)
        {
            InitializeComponent();
            Closing += DownloadProgressWindow_Closing;
        }

        /// <summary>
        /// Constructor for DownloadProgressWindow that takes an initialized
        /// <see cref="DownloadProgressWindowViewModel"/> view model for use
        /// </summary>
        /// <param name="viewModel"><see cref="DownloadProgressWindowViewModel"/> view model that
        /// this window will bind to as its DataContext</param>
        public DownloadProgressWindow(DownloadProgressWindowViewModel viewModel)
        {
            InitializeComponent();
            Closing += DownloadProgressWindow_Closing;
            DataContext = _dataContext = viewModel;
        }

        private void DownloadProgressWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_didCallDownloadProcessCompletedHandler)
            {
                _didCallDownloadProcessCompletedHandler = true;
                DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(false));
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
        public event DownloadInstallEventHandler? DownloadProcessCompleted;

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

        void IDownloadProgress.OnDownloadProgressChanged(object sender, ItemDownloadProgressEventArgs e)
        {
            OnDownloadProgressChanged(sender, e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
        }

        void IDownloadProgress.SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
        {
            ActionButton.IsEnabled = shouldBeEnabled;
        }

        void IDownloadProgress.Show()
        {
            ShowWindow(true);
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            _didCallDownloadProcessCompletedHandler = true;
            DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(
                (_dataContext?.DidDownloadFail ?? false)
                ? false
                : !(_dataContext?.IsDownloading ?? true)));
        }
    }
}
