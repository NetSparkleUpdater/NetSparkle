using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.Enums;
using System.Threading;
using System.Collections.Generic;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.AppCastHandlers;

namespace NetSparkleUpdater.UI.WinForms
{
    /// <summary>
    /// The main form
    /// </summary>
    public partial class UpdateAvailableWindow : Form, IUpdateAvailable
    {
        private readonly List<AppCastItem> _updates;
        private System.Windows.Forms.Timer? _ensureDialogShownTimer;
        private string _releaseNotesHTMLTemplate;
        private string _additionalReleaseNotesHeaderHTML;
        private string _releaseNotesDateFormat;

        /// <summary>
        /// Template for HTML code drawing release notes separator. {0} used for version number, {1} for publication date
        /// </summary>
        private CancellationToken? _cancellationToken;
        private CancellationTokenSource? _cancellationTokenSource;

        private bool _didSendResponse = false;


        /// <summary>
        /// Event fired when the user has responded to the 
        /// skip, later, install question.
        /// </summary>
        public event UserRespondedToUpdate? UserResponded;

        /// <summary>
        /// Object responsible for downloading and formatting markdown release notes for display in HTML
        /// </summary>
        public ReleaseNotesGrabber? ReleaseNotesGrabber { get; set; }

        private ISignatureVerifier? _signatureVerifier;

        /// <summary>
        /// Form constructor for showing release notes.
        /// </summary>
        /// <param name="signatureVerifier">The <seealso cref="ISignatureVerifier"/> for verifying release note signatures</param>
        /// <param name="items">The list of <seealso cref="AppCastItem"/> updates that are available for the user</param>
        /// <param name="isUpdateAlreadyDownloaded">Whether or not the update is already downloaded ot the user's computer</param>
        /// <param name="releaseNotesHTMLTemplate">The HTML string template to show for the release notes</param>
        /// <param name="additionalReleaseNotesHeaderHTML">The HTML string to add into the head element of the HTML for the release notes</param>
        /// <param name="releaseNotesDateFormat">Date format for release notes</param>
        /// <param name="appNameTitle">Title for application</param>
        /// <param name="installedVersion">Currently installed version of application</param>
        /// <param name="applicationIcon">The icon to display</param>
        public UpdateAvailableWindow(List<AppCastItem> items, ISignatureVerifier? signatureVerifier, bool isUpdateAlreadyDownloaded = false,
            string releaseNotesHTMLTemplate = "", string additionalReleaseNotesHeaderHTML = "", string releaseNotesDateFormat = "D",
            string appNameTitle = "the application", string installedVersion = "", Icon? applicationIcon = null)
        {
            _updates = items;
            _releaseNotesHTMLTemplate = releaseNotesHTMLTemplate;
            _additionalReleaseNotesHeaderHTML = additionalReleaseNotesHeaderHTML;
            _releaseNotesDateFormat = releaseNotesDateFormat;
            _signatureVerifier = signatureVerifier;

            InitializeComponent();

            var didError = false; // while loading release notes browser; if we have an error, make sure user sees it so dev can fix whatever happened
            try
            {
                ReleaseNotesBrowser.AllowWebBrowserDrop = false;
                ReleaseNotesBrowser.AllowNavigation = false;
            }
            catch (Exception ex)
            {
                lblInfoText.Text = string.Format("Error in browser init: {0}", ex.Message);
                didError = true;
            }

            AppCastItem? item = items.FirstOrDefault();

            var downloadInstallText = isUpdateAlreadyDownloaded ? "install" : "download";
            lblHeader.Text = lblHeader.Text.Replace("APP", item != null ? appNameTitle : "the application");
            if (!didError)
            {
                if (item != null)
                {
                    lblInfoText.Text = string.Format("{0} {1} is now available (you have {2}). Would you like to {3} it now?", 
                        appNameTitle, item.Version, installedVersion, downloadInstallText);
                }
                else
                {
                    // TODO: string translations (even though I guess this window should never be called with 0 app cast items...)
                    lblInfoText.Text = string.Format("Would you like to {0} it now?", downloadInstallText);
                }
            }

            bool isUserMissingCriticalUpdate = items.Any(x => x.IsCriticalUpdate);
            buttonRemind.Enabled = isUserMissingCriticalUpdate == false;
            skipButton.Enabled = isUserMissingCriticalUpdate == false;
            //if (isUserMissingCriticalUpdate)
            //{
            //    FormClosing += UpdateAvailableWindow_FormClosing; // no closing a critical update!
            //}

            if (applicationIcon != null)
            {
                using (Icon icon = new Icon(applicationIcon, new Size(48, 48)))
                {
                    imgAppIcon.Image = icon.ToBitmap();
                }
                Icon = applicationIcon;
            }
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            FormClosing += UpdateAvailableWindow_FormClosing;
        }

        /// <summary>
        /// Setup the ReleaseNotesGrabber (if needed) and load the release notes
        /// </summary>
        public void Initialize()
        {
            if (ReleaseNotesGrabber == null)
            {
                ReleaseNotesGrabber = new ReleaseNotesGrabber(_releaseNotesHTMLTemplate, _additionalReleaseNotesHeaderHTML, _signatureVerifier)
                {
                    DateFormat = _releaseNotesDateFormat
                };
            }
            ReleaseNotesBrowser.DocumentText = ReleaseNotesGrabber.GetLoadingText();
            EnsureDialogShown();
            LoadReleaseNotes(_updates);
        }

        private async void LoadReleaseNotes(List<AppCastItem> items)
        {
            AppCastItem? latestVersion = items.OrderByDescending(p => p.Version).FirstOrDefault();
            string releaseNotes = ReleaseNotesGrabber != null
                ? await ReleaseNotesGrabber.DownloadAllReleaseNotes(
                    items,
                    latestVersion ?? new AppCastItem(),
                    _cancellationToken ?? new CancellationTokenSource().Token)
                : "";
            ReleaseNotesBrowser.Invoke((MethodInvoker)delegate
            {
                // see https://stackoverflow.com/a/15209861/3938401
                ReleaseNotesBrowser.Navigate("about:blank");
                ReleaseNotesBrowser.Document?.OpenNew(true);
                ReleaseNotesBrowser.Document?.Write(releaseNotes);
                ReleaseNotesBrowser.DocumentText = releaseNotes;
            });
        }

        private void UpdateAvailableWindow_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_didSendResponse)
            {
                // user closed form in some other way other than standard buttons
                DialogResult = DialogResult.None;
                _didSendResponse = true;
                UserResponded?.Invoke(this, new UpdateResponseEventArgs(UpdateAvailableResult.None, CurrentItem));
            }
            FormClosing -= UpdateAvailableWindow_FormClosing;
        }

        /// <summary>
        /// The current item being installed
        /// </summary>
        AppCastItem IUpdateAvailable.CurrentItem => CurrentItem;

        /// <summary>
        /// The item that is being shown as the available update
        /// </summary>
        public AppCastItem CurrentItem
        {
            get { return _updates.Count > 0 ? _updates[0] : new AppCastItem(); }
        }

        /// <summary>
        /// The result of ShowDialog()
        /// </summary>
        UpdateAvailableResult IUpdateAvailable.Result => UIFactory.ConvertDialogResultToUpdateAvailableResult(DialogResult);

        /// <summary>
        /// Hides the release notes
        /// </summary>
        void IUpdateAvailable.HideReleaseNotes()
        {
            RemoveReleaseNotesControls();
        }

        /// <summary>
        /// Shows the dialog
        /// </summary>
        void IUpdateAvailable.Show()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Show()));
            }
            else
            {
                Show();
            }
        }

        void IUpdateAvailable.BringToFront()
        {
            BringToFront();
        }

        void IUpdateAvailable.Close()
        {
            _cancellationTokenSource?.Cancel();
            CloseForm();
        }

        private void CloseForm()
        {
            if (InvokeRequired && !IsDisposed && !Disposing)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    if (!IsDisposed && !Disposing)
                    {
                        Close();
                    }
                });
            }
            else if (!IsDisposed && !Disposing)
            {
                Close();
            }
        }

        /// <summary>
        /// Removes the release notes control
        /// </summary>
        public void RemoveReleaseNotesControls()
        {
            if (label3.Parent == null)
                return;

            // calc new size
            Size newSize = new Size(Size.Width, Size.Height - label3.Height - ReleaseNotesBrowser.Height);

            // remove the no more needed controls            
            label3.Parent?.Controls.Remove(label3);
            ReleaseNotesBrowser.Parent?.Controls.Remove(ReleaseNotesBrowser);

            // resize the window
            /*this.MinimumSize = newSize;
            this.Size = this.MinimumSize;
            this.MaximumSize = this.MinimumSize;*/
            Size = newSize;
        }

        void SendResponse(UpdateAvailableResult response)
        {
            _cancellationTokenSource?.Cancel();
            _didSendResponse = true;
            UserResponded?.Invoke(this, new UpdateResponseEventArgs(response, CurrentItem));
        }

        /// <summary>
        /// Event called when the skip button is clicked
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private void OnSkipButtonClick(object sender, EventArgs e)
        {
            // set the dialog result to no
            DialogResult = DialogResult.No;

            // close the windows
            SendResponse(UpdateAvailableResult.SkipUpdate);
        }

        /// <summary>
        /// Event called when the "remind me later" button is clicked
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private void OnRemindClick(object sender, EventArgs e)
        {
            // set the dialog result ot retry
            DialogResult = DialogResult.Retry;

            // close the window
            SendResponse(UpdateAvailableResult.RemindMeLater);
        }

        /// <summary>
        /// Called when the "Update button" is clicked
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private void OnUpdateButtonClick(object sender, EventArgs e)
        {
            // set the result to yes
            DialogResult = DialogResult.Yes;

            // close the dialog
            SendResponse(UpdateAvailableResult.InstallUpdate);
        }

        /// <summary>
        /// This was the only way Deadpikle could guarantee that the 
        /// update available window was shown above a main WPF MahApps window.
        /// It's an ugly hack but...oh well. :\
        /// </summary>
        public void EnsureDialogShown()
        {
            _ensureDialogShownTimer = new System.Windows.Forms.Timer();
            _ensureDialogShownTimer.Tick += new EventHandler(EnsureDialogeShown_tick);
            _ensureDialogShownTimer.Interval = 250; // in milliseconds
            _ensureDialogShownTimer.Start();
        }

        private void EnsureDialogeShown_tick(object? sender, EventArgs e)
        {
            // http://stackoverflow.com/a/4831839/3938401 for activating/bringing to front code
            Activate();
            TopMost = true;  // important
            TopMost = false; // important
            Focus();         // important
            if (_ensureDialogShownTimer != null)
            {
                _ensureDialogShownTimer.Enabled = false;
            }
            _ensureDialogShownTimer = null;
        }

        /// <summary>
        /// Hides the remind me later button for the update available window
        /// </summary>
        public void HideRemindMeLaterButton()
        {
            buttonRemind.Visible = false;
        }

        /// <summary>
        /// Hides the skip button for the update available window
        /// </summary>
        public void HideSkipButton()
        {
            skipButton.Visible = false;
        }
    }
}
