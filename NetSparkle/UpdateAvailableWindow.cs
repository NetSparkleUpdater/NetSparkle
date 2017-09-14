using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using NetSparkle.Interfaces;
using System.Text.RegularExpressions;
using NetSparkle.Enums;
using System.Threading.Tasks;
using System.Threading;

// TODO: Move a bunch of this logic to other objects than the form since it isn't really GUI logic and it could be put elsewhere

namespace NetSparkle
{
    /// <summary>
    /// The main form
    /// </summary>
    public partial class UpdateAvailableWindow : Form, IUpdateAvailable
    {
        private static readonly HashSet<string> MarkDownExtension = new HashSet<string> { ".md", ".mkdn", ".mkd", ".markdown" };

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
        private string _separatorTemplate;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        private string getVersion(Version version)
        {
            if (version.Build != 0)
                return version.ToString();
            if (version.Revision != 0)
                return version.ToString(3);
            return version.ToString(2);
        }

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

            _separatorTemplate = 
                !string.IsNullOrEmpty(separatorTemplate) ? 
                separatorTemplate :
                "<div style=\"border: #ccc 1px solid;\"><div style=\"background: {3}; padding: 5px;\"><span style=\"float: right; display:float;\">" +
                "{1}</span>{0}</div><div style=\"padding: 5px;\">{2}</div></div><br>";

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
                    versionString = getVersion(versionObj);
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

            AppCastItem latestVersion = items.OrderByDescending(p => p.Version).FirstOrDefault();
            string initialHTML = "<html><head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'>" + headAddition + "</head><body>";
            ReleaseNotesBrowser.DocumentText = initialHTML + "<p><em>Loading release notes...</em></p></body></html>";
            bool isUserMissingCriticalUpdate = false;
            foreach (AppCastItem castItem in items)
            {
                isUserMissingCriticalUpdate = isUserMissingCriticalUpdate | castItem.IsCriticalUpdate;
            }
            buttonRemind.Enabled = isUserMissingCriticalUpdate == false;
            skipButton.Enabled = isUserMissingCriticalUpdate == false;
            //if (isUserMissingCriticalUpdate)
            //{
            //    FormClosing += UpdateAvailableWindow_FormClosing; // no closing a critical update!
            //}

            if (applicationIcon != null)
            {
                imgAppIcon.Image = new Icon(applicationIcon, new Size(48, 48)).ToBitmap();
                Icon = applicationIcon;
            }
            EnsureDialogShown();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            downloadAndDisplayAllReleaseNotes(items, latestVersion, initialHTML);
        }

        private async void downloadAndDisplayAllReleaseNotes(AppCastItem[] items, AppCastItem latestVersion, string initialHTML)
        {

            _sparkle.LogWriter.PrintMessage("Preparing to initialize release notes...");
            StringBuilder sb = new StringBuilder(initialHTML);
            foreach (AppCastItem castItem in items)
            {
                _sparkle.LogWriter.PrintMessage("Initializing release notes for {0}", castItem.Version);
                // TODO: could we optimize this by doing multiple downloads at once?
                var releaseNotes = await GetReleaseNotes(castItem);
                sb.Append(string.Format(_separatorTemplate,
                                        castItem.Version,
                                        castItem.PublicationDate.ToString("D"), // was dd MMM yyyy
                                        releaseNotes,
                                        latestVersion.Version.Equals(castItem.Version) ? "#ABFF82" : "#AFD7FF"));
            }
            sb.Append("</body>");
            _sparkle.LogWriter.PrintMessage("Done initializing release notes!");

            string fullHTML = sb.ToString();
            ReleaseNotesBrowser.Invoke((MethodInvoker)delegate
            {
                // see https://stackoverflow.com/a/15209861/3938401
                ReleaseNotesBrowser.Navigate("about:blank");
                ReleaseNotesBrowser.Document.OpenNew(true);
                ReleaseNotesBrowser.Document.Write(fullHTML);
                ReleaseNotesBrowser.DocumentText = fullHTML;
            });
        }

        private void UpdateAvailableWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private async Task<string> GetReleaseNotes(AppCastItem item)
        {
            string criticalUpdate = item.IsCriticalUpdate ? "Critical Update" : "";
            // at first try to use embedded description
            if (!string.IsNullOrEmpty(item.Description))
            {
                // check for markdown
                Regex containsHtmlRegex = new Regex(@"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>");
                if (containsHtmlRegex.IsMatch(item.Description))
                {
                    if (item.IsCriticalUpdate)
                    {
                        item.Description = "<p><em>" + criticalUpdate + "</em></p>" + "<br>" + item.Description;
                    }
                    return item.Description;
                }
                else
                {
                    var md = new MarkdownSharp.Markdown();
                    if (item.IsCriticalUpdate)
                    {
                        item.Description = "*" + criticalUpdate + "*" + "\n\n" + item.Description;
                    }
                    var temp = md.Transform(item.Description);
                    return temp;
                }
            }

            // not embedded so try to release notes from the link
            if (string.IsNullOrEmpty(item.ReleaseNotesLink))
            {
                return null;
            }

            // download release notes
            _sparkle.LogWriter.PrintMessage("Downloading release notes for {0} at {1}", item.Version, item.ReleaseNotesLink);
            string notes = await DownloadReleaseNotes(item.ReleaseNotesLink, _cancellationToken);
            _sparkle.LogWriter.PrintMessage("Downloaded release notes for {0}: {1}", item.Version, notes);
            if (string.IsNullOrEmpty(notes))
            {
                return null;
            }

            // check dsa of release notes
            if (!string.IsNullOrEmpty(item.ReleaseNotesDSASignature))
            {
                if (_sparkle.DSAChecker.VerifyDSASignatureOfString(item.ReleaseNotesDSASignature, notes) == ValidationResult.Invalid)
                    return null;
            }

            // process release notes
            var extension = Path.GetExtension(item.ReleaseNotesLink);
            if (extension != null && MarkDownExtension.Contains(extension.ToLower()))
            {
                try
                {
                    var md = new MarkdownSharp.Markdown();
                    if (item.IsCriticalUpdate)
                    {
                        notes = "*" + criticalUpdate + "*" + "\n\n" + notes;
                    }
                    notes = md.Transform(notes);
                }
                catch (Exception ex)
                {
                    _sparkle.LogWriter.PrintMessage("Error parsing Markdown syntax: {0}", ex.Message);
                }
            }
            return notes;
        }

        private async Task<string> DownloadReleaseNotes(string link, CancellationToken cancellationToken)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                    webClient.Encoding = Encoding.UTF8;
                    if (cancellationToken != null)
                    {
                        using (cancellationToken.Register(() => webClient.CancelAsync()))
                        {
                            return await webClient.DownloadStringTaskAsync(_sparkle.GetAbsoluteUrl(link));
                        }
                    }
                    return await webClient.DownloadStringTaskAsync(_sparkle.GetAbsoluteUrl(link));
                }
            }
            catch (WebException ex)
            {
                _sparkle.LogWriter.PrintMessage("Cannot download release notes from {0} because {1}", link, ex.Message);
                return "";
            }
        }

        /// <summary>
        /// The current item being installed
        /// </summary>
        AppCastItem IUpdateAvailable.CurrentItem
        {
            get { return _updates[0]; }
        }

        /// <summary>
        /// The result of ShowDialog()
        /// </summary>
        DialogResult IUpdateAvailable.Result
        {
            get { return DialogResult; }
        }

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
            Close();
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
            Close();
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
            Close();
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
            Close();
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
    }
}
