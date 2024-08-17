using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using NetSparkleUpdater.UI.WinForms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NetSparkle.Samples.Forms.Multithread
{
    public partial class Form1 : Form
    {
        private SparkleUpdater _sparkleUpdateDetector;
        private UIFactory _factory;
        private UpdateInfo? _updateInfo = null;
        private Icon? _icon;
        private string? _downloadPath = null;
        private bool _hasDownloadFinished = false;

        public Form1()
        {
            InitializeComponent();

            var appcastUrl = "https://netsparkleupdater.github.io/NetSparkle/files/sample-app/appcast.xml";
            // set icon in project properties - <ApplicationIcon>
            string manifestModuleName = System.Reflection.Assembly.GetEntryAssembly()?.ManifestModule.FullyQualifiedName ?? "";
            _icon = System.Drawing.Icon.ExtractAssociatedIcon(manifestModuleName);
            _factory = new UIFactory(_icon);
            // you can, of course, use your own UIFactory, your own UI objects, etc. This is just a sample!
            _sparkleUpdateDetector = new SparkleUpdater(appcastUrl, new DSAChecker(SecurityMode.Strict))
            {
                UIFactory = null // so we can handle threads, which is outside the context of SparkleUpdater's use of UIFactory objects
            };
            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkleUpdateDetector.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
            //_sparkleUpdateDetector.CloseApplication += _sparkleUpdateDetector_CloseApplication;
        }

        private void _sparkleUpdateDetector_CloseApplication()
        {
            Application.Exit();
        }

        private async void AppBackgroundCheckButton_Click(object sender, EventArgs e)
        {
            // Manually check for updates, this will not show a ui
            var result = await _sparkleUpdateDetector.CheckForUpdatesQuietly();
            if (result.Status == UpdateStatus.UpdateAvailable)
            {
                // if update(s) are found, then we have to trigger the UI to show it gracefully
                ShowUpdateWindow();
            }
        }

        private CheckingForUpdatesWindow? _checkingForUpdatesWindow;
        private UpdateAvailableWindow? _updateAvailableWindow;
        private DownloadProgressWindow? _downloadProgressWindow;

        private void ShowCheckingWindow()
        {
            if (_checkingForUpdatesWindow == null)
            {
                //var re = new AutoResetEvent(false); // can use this to make an async thread start sync, basically
                // overall func won't return until .Set() called.
                var t = new Thread(() =>
                {
                    var window = _factory.ShowCheckingForUpdates(_sparkleUpdateDetector) as CheckingForUpdatesWindow;
                    window!.FormClosed += (a, b) => Application.ExitThread();
                    window!.Shown += CheckingWindowShown;
                    _checkingForUpdatesWindow = window;
                    //re.Set();
                    Application.Run(window);
                });
                t.SetApartmentState(ApartmentState.STA); // only supported on Windows
                t.Start();
                //re.WaitOne();
            }
        }

        private void ShowMessage(string title, string message)
        {
            var t = new Thread(() =>
            {
                var messageWindow = new MessageNotificationWindow(title, message, _icon);
                messageWindow.StartPosition = FormStartPosition.CenterScreen;
                messageWindow.ShowDialog();
                messageWindow.FormClosed += (a, b) => Application.ExitThread();
                //re.Set();
                Application.Run(messageWindow);
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private async void CheckingWindowShown(object? sender, EventArgs e)
        {
            _updateInfo = await _sparkleUpdateDetector.CheckForUpdatesAtUserRequest();
            if (_updateInfo != null)
            {
                switch (_updateInfo.Status)
                {
                    case UpdateStatus.UpdateAvailable:
                        ShowUpdateWindow();
                        break;
                    case UpdateStatus.UpdateNotAvailable:
                        ShowMessage("Info", "No update available");
                        CloseCheckingForUpdatesWindow(); // could be done once message shown but that's OK for this sample
                        break;
                    case UpdateStatus.UserSkipped:
                        ShowMessage("Info", "User skipped update");
                        CloseCheckingForUpdatesWindow();
                        break;
                    case UpdateStatus.CouldNotDetermine:
                        ShowMessage("Info", "We couldn't tell if there was an update...");
                        CloseCheckingForUpdatesWindow();
                        break;
                }
            }
        }

        private void CloseCheckingForUpdatesWindow()
        {
            if (_checkingForUpdatesWindow?.InvokeRequired ?? false)
            {
                _checkingForUpdatesWindow.Invoke(CloseCheckingForUpdatesWindow);
            }
            else
            {
                _checkingForUpdatesWindow?.Close();
                _checkingForUpdatesWindow = null;
            }
        }

        private void UpdateAvailableWindowShown(object? sender, EventArgs e)
        {
            CloseCheckingForUpdatesWindow();
        }

        private void ExplicitUserRequestCheckButton_Click(object sender, EventArgs e)
        {
            ShowCheckingWindow();
        }

        private void ShowUpdateWindow()
        {
            if (_updateInfo != null && _updateAvailableWindow == null)
            {
                var t = new Thread(() =>
                {
                    var window = _factory.CreateUpdateAvailableWindow(_sparkleUpdateDetector, _updateInfo.Updates, false) as UpdateAvailableWindow;
                    window!.FormClosed += (a, b) => Application.ExitThread();
                    window!.Shown += UpdateAvailableWindowShown;
                    window.UserResponded += UpdateWindowUserResponded;
                    _updateAvailableWindow = window;
                    Application.Run(window);
                });
                t.SetApartmentState(ApartmentState.STA); // only supported on Windows
                t.Start();
            }
        }

        private void ShowDownloadingWindow()
        {
            if (_updateInfo != null)
            {
                var t = new Thread(() =>
                {
                    var window = _factory.CreateProgressWindow(_sparkleUpdateDetector, _updateInfo.Updates[0]) as DownloadProgressWindow;
                    window!.FormClosed += (a, b) => Application.ExitThread();
                    window!.Shown += ProgressWindowShown;
                    _downloadProgressWindow = window;
                    Application.Run(window);
                });
                t.SetApartmentState(ApartmentState.STA); // only supported on Windows
                t.Start();
            }
        }

        private void UpdateWindowUserResponded(object sender, NetSparkleUpdater.Events.UpdateResponseEventArgs e)
        {
            if (_updateAvailableWindow != null && _updateInfo != null)
            {
                if (e.Result == UpdateAvailableResult.InstallUpdate)
                {
                    ShowDownloadingWindow();
                }
            }
        }

        private void CloseUpdateAvailableWindow()
        {
            if (_updateAvailableWindow?.InvokeRequired ?? false)
            {
                _updateAvailableWindow.Invoke(CloseUpdateAvailableWindow);
            }
            else
            {
                _updateAvailableWindow?.Close();
                _updateAvailableWindow = null;
            }
        }

        private async void ProgressWindowShown(object? sender, EventArgs e)
        {
            if (_updateInfo != null && _downloadProgressWindow != null)
            {
                // we want to download the item
                _sparkleUpdateDetector.DownloadFinished -= _sparkle_FinishedDownloading;
                _sparkleUpdateDetector.DownloadFinished += _sparkle_FinishedDownloading;

                _sparkleUpdateDetector.DownloadHadError -= _sparkle_DownloadError;
                _sparkleUpdateDetector.DownloadHadError += _sparkle_DownloadError;

                _sparkleUpdateDetector.DownloadMadeProgress += _sparkle_DownloadMadeProgress;
                _downloadProgressWindow.DownloadProcessCompleted += DownloadProgressWindow_DownloadProcessCompleted;

                _hasDownloadFinished = false;
                await _sparkleUpdateDetector.InitAndBeginDownload(_updateInfo.Updates.First());
            }
            CloseUpdateAvailableWindow();
        }

        private void CloseDownloadProgressWindow()
        {
            if (_downloadProgressWindow?.InvokeRequired ?? false)
            {
                _downloadProgressWindow.Invoke(CloseDownloadProgressWindow);
            }
            else
            {
                _downloadProgressWindow?.Close();
                _downloadProgressWindow = null;
            }
        }

        private void _sparkle_DownloadMadeProgress(object sender, AppCastItem item, NetSparkleUpdater.Events.ItemDownloadProgressEventArgs e)
        {
            if (_downloadProgressWindow?.InvokeRequired ?? false)
            {
                _downloadProgressWindow.Invoke(() => _sparkle_DownloadMadeProgress(sender, item, e));
            }
            else
            {
                if (!_hasDownloadFinished)
                {
                    _downloadProgressWindow?.OnDownloadProgressChanged(sender, e);
                }
            }
        }

        private void _sparkle_DownloadError(AppCastItem item, string? path, Exception exception)
        {
            if (_downloadProgressWindow?.InvokeRequired ?? false)
            {
                _downloadProgressWindow.Invoke(() => _sparkle_DownloadError(item, path, exception));
            }
            else
            {
                _downloadProgressWindow?.DisplayErrorMessage("We had an error during the download process :( -- " + exception.Message);
            }
        }

        private void _sparkle_FinishedDownloading(AppCastItem item, string path)
        {
            if (_downloadProgressWindow?.InvokeRequired ?? false)
            {
                _downloadProgressWindow.Invoke(() => _sparkle_FinishedDownloading(item, path));
            }
            else
            {
                if (_updateInfo != null)
                {
                    _hasDownloadFinished = true;
                    var updateSize = _updateInfo.Updates.First().UpdateSize;
                    var validationRes = _sparkleUpdateDetector.SignatureVerifier.VerifySignatureOfFile(
                        _updateInfo.Updates.First().DownloadSignature ?? "", path);
                    bool isSignatureInvalid = validationRes == ValidationResult.Invalid;
                    _downloadProgressWindow?.FinishedDownloadingFile(isDownloadedFileValid: !isSignatureInvalid);
                    _downloadPath = path;
                }
            }
        }

        private async void DownloadProgressWindow_DownloadProcessCompleted(object sender, NetSparkleUpdater.Events.DownloadInstallEventArgs args)
        {
            if (args.ShouldInstall && _updateInfo != null)
            {
                _sparkleUpdateDetector.CloseApplication += _sparkle_CloseApplication;
                await _sparkleUpdateDetector.InstallUpdate(_updateInfo.Updates.First(), _downloadPath);
            }
            else
            {
                CloseDownloadProgressWindow();
            }
        }

        private void _sparkle_CloseApplication()
        {
            Application.Exit();
        }
    }
}
