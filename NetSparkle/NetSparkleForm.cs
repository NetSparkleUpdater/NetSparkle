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

namespace NetSparkle
{
    /// <summary>
    /// The main form
    /// </summary>
    public partial class NetSparkleForm : Form, INetSparkleForm
    {
        private static readonly HashSet<string> MarkDownExtension = new HashSet<string> { ".md", ".mkdn", ".mkd", ".markdown" };

        private readonly Sparkle _sparkle;
        private readonly NetSparkleAppCastItem[] _updates;
        private Timer _ensureDialogShownTimer;

        /// <summary>
        /// Event fired when the user has responded to the 
        /// skip, later, install question.
        /// </summary>
        public event EventHandler UserResponded;

        /// <summary>
        /// Template for HTML code drawig release notes separator. {0} used for version number, {1} for publication date
        /// </summary>
        // Useless. Because the form initialize this value it selfes and directlly generates the release notes html. So
        // there is no chance to override the default. Also there isn't any chance to access from the interface
        private string SeparatorTemplate { get; set; }

        private string getVersion(Version version)
        {
            if (version.Build != 0)
                return version.ToString();
            if (version.Revision != 0)
                return version.ToString(3);
            return version.ToString(2);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">List of updates to show</param>
        /// <param name="applicationIcon">The icon</param>
        /// <param name="separatorTemplate">HTML template for every single note. Use {0} = Version. {1} = Date. {2} = Note Body</param>
        /// <param name="headAddition">Additional text they will inserted into HTML Head. For Stylesheets.</param>
        public NetSparkleForm(Sparkle sparkle, NetSparkleAppCastItem[] items, Icon applicationIcon = null, bool isUpdateAlreadyDownloaded = false, 
            string separatorTemplate = "", string headAddition = "")
        {
            _sparkle = sparkle;
            _updates = items;

            SeparatorTemplate = 
                !string.IsNullOrEmpty(separatorTemplate) ? 
                separatorTemplate :
                "<div style=\"border: #ccc 1px solid;\"><div style=\"background: {3}; padding: 5px;\"><span style=\"float: right; display:float;\">" +
                "{1}</span>{0}</div><div style=\"padding: 5px;\">{2}</div></div><br>";

            InitializeComponent();

            // init ui 
            try
            {
                NetSparkleBrowser.AllowWebBrowserDrop = false;
                NetSparkleBrowser.AllowNavigation = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in browser init: " + ex.Message);
            }

            NetSparkleAppCastItem item = items[0];

            lblHeader.Text = lblHeader.Text.Replace("APP", item.AppName);
            lblInfoText.Text = lblInfoText.Text.Replace("APP", item.AppName + " " + item.Version);
            lblInfoText.Text = lblInfoText.Text.Replace("OLDVERSION", getVersion(new Version(item.AppVersionInstalled)));
            lblInfoText.Text = lblInfoText.Text.Replace("[DOWNLOAD]", isUpdateAlreadyDownloaded ? "install" : "download");

            if (items.Length == 0)
            {
                RemoveReleaseNotesControls();
            }
            else
            {
                NetSparkleAppCastItem latestVersion = _updates.OrderByDescending(p => p.Version).FirstOrDefault();

                StringBuilder sb = new StringBuilder("<html><head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'>" + headAddition + "</head><body>");
                foreach (NetSparkleAppCastItem castItem in items)
                {
                    sb.Append(string.Format(SeparatorTemplate, 
                                            castItem.Version,
                                            castItem.PublicationDate.ToString("dd MMM yyyy"),
                                            GetReleaseNotes(castItem),
                                            latestVersion.Version.Equals(castItem.Version) ? "#ABFF82" : "#AFD7FF"));
                }
                sb.Append("</body>");

                string releaseNotes = sb.ToString();
                NetSparkleBrowser.DocumentText = releaseNotes;

                buttonRemind.Enabled = latestVersion.IsCriticalUpdate == false;
                skipButton.Enabled = latestVersion.IsCriticalUpdate == false;
                if (latestVersion.IsCriticalUpdate)
                {
                    //FormClosing += NetSparkleForm_FormClosing; // no closing a critical update!
                }
            }

            if (applicationIcon != null)
            {
                imgAppIcon.Image = new Icon(applicationIcon, new Size(48, 48)).ToBitmap();
                Icon = applicationIcon;
            }
            EnsureDialogShown();
        }

        private void NetSparkleForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private string GetReleaseNotes(NetSparkleAppCastItem item)
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
                        item.Description = "<p>" + criticalUpdate + "</p>" + "<br>" + item.Description;
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

            // no embedded so try to get external
            if (string.IsNullOrEmpty(item.ReleaseNotesLink))
            {
                return null;
            }

            // download release note
            string notes = DownloadReleaseNotes(item.ReleaseNotesLink);
            if (string.IsNullOrEmpty(notes))
            {
                return null;
            }

            // check dsa of release notes
            if (!string.IsNullOrEmpty(item.ReleaseNotesDSASignature))
            {
                if (_sparkle.DSAVerificator.VerifyDSASignatureOfString(item.ReleaseNotesDSASignature, notes) == ValidationResult.Invalid)
                    return null;
            }

            // process release notes
            var extension = Path.GetExtension(item.ReleaseNotesLink);
            if (extension != null && MarkDownExtension.Contains(extension.ToLower()))
            {
                try
                {
                    var md = new MarkdownSharp.Markdown();
                    notes = "*" + criticalUpdate + "*" + "\n\n" + notes;
                    notes = md.Transform(notes);
                }
                catch (Exception ex)
                {
                    _sparkle.ReportDiagnosticMessage("Error parsing MarkDown syntax: " + ex.Message);
                }
            }
            return notes;
        }

        private string DownloadReleaseNotes(string link)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                    webClient.Encoding = Encoding.UTF8;

                    return webClient.DownloadString(_sparkle.GetAbsoluteUrl(link));
                }
            }
            catch (WebException ex)
            {
                _sparkle.ReportDiagnosticMessage("Cannot download release notes from " + link + " because " + ex.Message);
                return "";
            }
        }

        /// <summary>
        /// The current item being installed
        /// </summary>
        NetSparkleAppCastItem INetSparkleForm.CurrentItem
        {
            get { return _updates[0]; }
        }

        /// <summary>
        /// The result of ShowDialog()
        /// </summary>
        DialogResult INetSparkleForm.Result
        {
            get { return DialogResult; }
        }

        /// <summary>
        /// Hides the release notes
        /// </summary>
        void INetSparkleForm.HideReleaseNotes()
        {
            RemoveReleaseNotesControls();
        }

        /// <summary>
        /// Shows the dialog
        /// </summary>
        void INetSparkleForm.Show()
        {
            ShowDialog();
            if (UserResponded != null)
            {
                UserResponded(this, new EventArgs());
            }
        }

        void INetSparkleForm.BringToFront()
        {
            BringToFront();
        }

        void INetSparkleForm.Close()
        {
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
            NetSparkleBrowser.Parent.Controls.Remove(NetSparkleBrowser);
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
            Close();
        }

        /// <summary>
        /// This was the only way Deadpikle could guarantee that the 
        /// update available window was shown above a main WPF MahApps window.
        /// It's an ugly hack but...oh well. :\
        /// </summary>
        public void EnsureDialogShown()
        {
            _ensureDialogShownTimer = new Timer();
            _ensureDialogShownTimer.Tick += new EventHandler(EnsureDialogeShown_tick);
            _ensureDialogShownTimer.Interval = 250; // in miliseconds
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
