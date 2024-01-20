using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Drawing;
using System.Threading;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.SignatureVerifiers;

namespace NetSparkleUpdater.Samples.HandleEventsYourself
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IAppCastFilter
    {
        private SparkleUpdater _sparkle;
        private UpdateInfo _updateInfo;
        private string _downloadPath = null;
        private bool _hasDownloadFinished = false;
        
        /// <summary>
        /// this isn't hooked up to anything - imagine though that this flag indicates the apps desire to switch
        /// from a beta channel to a stable channel.
        /// <seealso cref="GetFilteredAppCastItems"/>
        /// </summary>
        private bool filterOutBetaItems = true;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(
                    "Software\\Microsoft\\NetSparkle.TestAppNetCoreWPF");
            }
            catch
            {
            }

            // get sparkle ready
            DownloadUpdateButton.IsEnabled = false;
            InstallUpdateButton.IsEnabled = false;

            _sparkle = new SparkleUpdater("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/appcast.xml",
                new DSAChecker(Enums.SecurityMode.Strict))
            {
                UIFactory = null
            };
            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkle.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
        }

        private async void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            InstallUpdateButton.IsEnabled = false;
            UpdateInfo.Content = "Checking for updates...";
            _updateInfo = await _sparkle.CheckForUpdatesQuietly();
            // use _sparkle.CheckForUpdatesQuietly() if you don't want the user to know you are checking for updates!
            // if you use CheckForUpdatesAtUserRequest() and are using a UI, then handling things yourself is rather silly
            // as it will show a UI for everything
            if (_updateInfo != null)
            {
                switch (_updateInfo.Status)
                {
                    case Enums.UpdateStatus.UpdateAvailable:
                        UpdateInfo.Content = "There's an update available!";
                        DownloadUpdateButton.IsEnabled = true;
                        break;
                    case Enums.UpdateStatus.UpdateNotAvailable:
                        UpdateInfo.Content = "There's no update available :(";
                        DownloadUpdateButton.IsEnabled = false;
                        break;
                    case Enums.UpdateStatus.UserSkipped:
                        UpdateInfo.Content = "The user skipped this update!";
                        DownloadUpdateButton.IsEnabled = false;
                        break;
                    case Enums.UpdateStatus.CouldNotDetermine:
                        UpdateInfo.Content = "We couldn't tell if there was an update...";
                        DownloadUpdateButton.IsEnabled = false;
                        break;
                }
            }
        }

        private async void DownloadUpdate_Click(object sender, RoutedEventArgs e)
        {
            // this is async so that it can grab the download file name from the server
            _sparkle.DownloadStarted -= _sparkle_StartedDownloading;
            _sparkle.DownloadStarted += _sparkle_StartedDownloading;

            _sparkle.DownloadFinished -= _sparkle_FinishedDownloading;
            _sparkle.DownloadFinished += _sparkle_FinishedDownloading;

            _sparkle.DownloadHadError -= _sparkle_DownloadError;
            _sparkle.DownloadHadError += _sparkle_DownloadError;

            _sparkle.DownloadMadeProgress += _sparkle_DownloadMadeProgress;

            _hasDownloadFinished = false;
            await _sparkle.InitAndBeginDownload(_updateInfo.Updates.First());
            // ok, the file is downloading now
        }

        private void _sparkle_DownloadMadeProgress(object sender, AppCastItem item, ItemDownloadProgressEventArgs e)
        {
            if (!_hasDownloadFinished)
            {
                DownloadInfo.Text = string.Format("The download made some progress! {0}% done.", e.ProgressPercentage);
            }
        }

        private void _sparkle_DownloadError(AppCastItem item, string path, Exception exception)
        {
            DownloadInfo.Text = "We had an error during the download process :( -- " + exception.Message;
            InstallUpdateButton.IsEnabled = false;
        }

        private void _sparkle_StartedDownloading(AppCastItem item, string path)
        {
            if (!_hasDownloadFinished)
            {
                DownloadInfo.Text = "Started downloading...";
                InstallUpdateButton.IsEnabled = false;
            }
        }

        private void _sparkle_FinishedDownloading(AppCastItem item, string path)
        {
            _hasDownloadFinished = true;
            DownloadInfo.Text = "Done downloading!";
            InstallUpdateButton.IsEnabled = true;
            _downloadPath = path;
        }

        private void InstallUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            _sparkle.CloseApplication += _sparkle_CloseApplication;
            _sparkle.InstallUpdate(_updateInfo.Updates.First(), _downloadPath);
        }

        private void _sparkle_CloseApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }

        private async void UpdateAutomaticallyButton_Click(object sender, RoutedEventArgs e)
        {
            _sparkle.UserInteractionMode = Enums.UserInteractionMode.DownloadAndInstall;
            RunFullUpdateUpdateStatusLabel.Text = "Checking for update...";
            _sparkle.UpdateDetected += _sparkle_FullUpdate_UpdateDetected;
            _sparkle.DownloadStarted += _sparkle_FullUpdate_StartedDownloading;
            _sparkle.DownloadFinished += _sparkle_FullUpdate_DownloadFileIsReady;
            _sparkle.CloseApplication += _sparkle_FullUpdate_CloseApplication;
            await _sparkle.CheckForUpdatesQuietly();
        }

        private void _sparkle_FullUpdate_UpdateDetected(object sender, Events.UpdateDetectedEventArgs e)
        {
            RunFullUpdateUpdateStatusLabel.Text = "Found update...";
        }

        private void _sparkle_FullUpdate_StartedDownloading(AppCastItem item, string path)
        {
            RunFullUpdateUpdateStatusLabel.Text = "Started downloading update...";
        }

        private void _sparkle_FullUpdate_DownloadFileIsReady(AppCastItem item, string downloadPath)
        {
            RunFullUpdateUpdateStatusLabel.Text = "Update is ready...";
        }

        private async void _sparkle_FullUpdate_CloseApplication()
        {
            RunFullUpdateUpdateStatusLabel.Text = "Closing application...";
            await Task.Delay(2000);
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// Filters out appcast items that are not relevant to the current update loop, and returns the appropriate runtime
        /// Version value for this update.
        /// The logic to decide whether the app is downgrading from beta to a stable channel is not part of NetSparkle.
        /// </summary>
        /// <param name="installed">Currently installed version of the app that's running</param>
        /// <param name="items">The list of appcast items retrieved from whatever URL the SparkleUpdater is using</param>
        /// <returns>A Version object representing the highest version number that the update process should consider
        /// and a list of appcast items that have potentially been filtered to remove those that don't apply to the current
        /// requirements, specifically when downgrading from beta to a stable channel we do not want to present the NetSparkle
        /// system with appcast items that are part of the beta channel</returns>
        public IEnumerable<AppCastItem> GetFilteredAppCastItems(SemVerLike installed, IEnumerable<AppCastItem> items)
        {
            // if we don't need to filter, then this is essentially a no-op.
            if (!filterOutBetaItems)
                return new List<AppCastItem>();

            // here we need to remove any items that are beta - since the appcast object does not expose channel
            // information as a first class citizen (yet); I'll just check the URL to see if it contains the word beta.
            // note: this is a per-app decision, and the code here just demonstrates how to detect and remove beta
            // app cast elements - you will need to have your own logic to handle this for your appcast situation. 
            var itemsWithoutBeta = items.Where((item) =>
            {
                if (item.DownloadLink.Contains("/beta/"))
                    return false;
                return true;
            });
            
            return itemsWithoutBeta;
        }
    }
}