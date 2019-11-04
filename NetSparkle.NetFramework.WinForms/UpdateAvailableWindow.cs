using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetSparkle.Interfaces;
using NetSparkle.Enums;
using System.Threading;

namespace NetSparkle.UI.NetFramework.WinForms
{
    /// <summary>
    /// The main form
    /// </summary>
    public partial class UpdateAvailableWindow : Form, IUpdateAvailable
    {
        private readonly Sparkle _sparkle;
        private readonly AppCastItem[] _updates;
        private System.Windows.Forms.Timer _ensureDialogShownTimer;

        /// <summary>
        /// Event fired when the user has responded to the 
        /// skip, later, install question.
        /// </summary>
        public event EventHandler UserResponded;

        /// <summary>
        /// Template for HTML code drawing release notes separator. {0} used for version number, {1} for publication date
        /// </summary>
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        private ReleaseNotesGrabber _releaseNotesGrabber;

        /// <summary>
        /// Form constructor for showing release notes.
        /// </summary>
        /// <param name="sparkle">The <see cref="Sparkle"/> instance to use</param>
        /// <param name="items">List of updates to show. Should contain at least one item.</param>
        /// <param name="applicationIcon">The icon to display</param>
        /// <param name="isUpdateAlreadyDownloaded">If true, make sure UI text shows that the user is about to install the file instead of download it.</param>
        /// <param name="separatorTemplate">HTML template for every single note. Use {0} = Version. {1} = Date. {2} = Note Body</param>
        /// <param name="headAddition">Additional text they will inserted into HTML Head. For Stylesheets.</param>
        public UpdateAvailableWindow(Sparkle sparkle, AppCastItem[] items, Icon applicationIcon = null, bool isUpdateAlreadyDownloaded = false, 
            string separatorTemplate = "", string headAddition = "")
        {
            _sparkle = sparkle;
            _updates = items;
            _releaseNotesGrabber = new ReleaseNotesGrabber(separatorTemplate, headAddition, sparkle);

            InitializeComponent();

            // init ui 
            try
            {
                ReleaseNotesBrowser.AllowWebBrowserDrop = false;
                ReleaseNotesBrowser.AllowNavigation = false;
            }
            catch (Exception ex)
            {
                _sparkle.LogWriter.PrintMessage("Error in browser init: {0}", ex.Message);
            }

            AppCastItem item = items.FirstOrDefault();

            lblHeader.Text = lblHeader.Text.Replace("APP", item != null ? item.AppName : "the application");
            if (item != null)
            {
                lblInfoText.Text = lblInfoText.Text.Replace("APP", item.AppName + " " + item.Version);
                var versionString = "";
                try
                {
                    // Use try/catch since Version constructor can throw an exception and we don't want to
                    // die just because the user has a malformed version string
                    Version versionObj = new Version(item.AppVersionInstalled);
                    versionString = NetSparkle.Utilities.GetVersionString(versionObj);
                }
                catch
                {
                    versionString = "";
                }
                lblInfoText.Text = lblInfoText.Text.Replace("OLDVERSION", versionString);
            }
            else
            {
                // TODO: string translations (even though I guess this window should never be called with 0 app cast items...)
                lblInfoText.Text = "Would you like to [DOWNLOAD] it now?"; 
            }
            lblInfoText.Text = lblInfoText.Text.Replace("[DOWNLOAD]", isUpdateAlreadyDownloaded ? "install" : "download");

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

            ReleaseNotesBrowser.DocumentText = _releaseNotesGrabber.GetLoadingText();
            EnsureDialogShown();
            LoadReleaseNotes(items);
        }

        private async void LoadReleaseNotes(AppCastItem[] items)
        {
            AppCastItem latestVersion = items.OrderByDescending(p => p.Version).FirstOrDefault();
            string releaseNotes = await _releaseNotesGrabber.DownloadAllReleaseNotesAsHTML(items, latestVersion, _cancellationToken);
            ReleaseNotesBrowser.Invoke((MethodInvoker)delegate
            {
                // see https://stackoverflow.com/a/15209861/3938401
                ReleaseNotesBrowser.Navigate("about:blank");
                ReleaseNotesBrowser.Document.OpenNew(true);
                ReleaseNotesBrowser.Document.Write(releaseNotes);
                ReleaseNotesBrowser.DocumentText = releaseNotes;
            });
        }

        private void UpdateAvailableWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// The current item being installed
        /// </summary>
        AppCastItem IUpdateAvailable.CurrentItem
        {
            get { return _updates.Count() > 0 ? _updates[0] : null; }
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
            ShowDialog();
            UserResponded?.Invoke(this, new EventArgs());
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
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate () { Close(); });
            }
            else
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
            Size newSize = new Size(Size.Width, Size.Height - label3.Height - panel1.Height);

            // remove the no more needed controls            
            label3.Parent.Controls.Remove(label3);
            ReleaseNotesBrowser.Parent.Controls.Remove(ReleaseNotesBrowser);
            panel1.Parent.Controls.Remove(panel1);

            // resize the window
            /*this.MinimumSize = newSize;
            this.Size = this.MinimumSize;
            this.MaximumSize = this.MinimumSize;*/
            Size = newSize;
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
            _cancellationTokenSource?.Cancel();
            CloseForm();
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
            _cancellationTokenSource?.Cancel();
            CloseForm();
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
            _cancellationTokenSource?.Cancel();
            CloseForm();
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

        private void EnsureDialogeShown_tick(object sender, EventArgs e)
        {
            // http://stackoverflow.com/a/4831839/3938401 for activating/bringing to front code
            Activate();
            TopMost = true;  // important
            TopMost = false; // important
            Focus();         // important
            _ensureDialogShownTimer.Enabled = false;
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
