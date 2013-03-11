using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using NetSparkle.Interfaces;

namespace NetSparkle
{
    /// <summary>
    /// The main form
    /// </summary>
    public partial class NetSparkleForm : Form, INetSparkleForm
    {
        NetSparkleAppCastItem _currentItem;

        /// <summary>
        /// Event fired when the user has responded to the 
        /// skip, later, install question.
        /// </summary>
        public event EventHandler UserResponded;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="applicationIcon"></param>
        public NetSparkleForm(NetSparkleAppCastItem item, Icon applicationIcon)
        {            
            InitializeComponent();
            
            // init ui 
            try
            {
                NetSparkleBrowser.AllowWebBrowserDrop = false;
                NetSparkleBrowser.AllowNavigation = false;
                NetSparkleBrowser.Navigated += new WebBrowserNavigatedEventHandler(NetSparkleBrowser_Navigated);
            }
            catch (Exception)
            { }
            
            _currentItem = item;
           

            lblHeader.Text = lblHeader.Text.Replace("APP", item.AppName);
            lblInfoText.Text = lblInfoText.Text.Replace("APP", item.AppName + " " + item.Version);
            lblInfoText.Text = lblInfoText.Text.Replace("OLDVERSION", item.AppVersionInstalled);

            if (item.ReleaseNotesLink != null && item.ReleaseNotesLink.Length > 0 )
            {

                if (new List<string>(new[]{".md",".mkdn",".mkd",".markdown"}).Contains(Path.GetExtension(item.ReleaseNotesLink).ToLower()))
                {
                    try
                    {
                        ShowMarkdownReleaseNotes(item);
                    }
                    catch (Exception)
                    {
#if DEBUG
                        throw;
#else
                        NetSparkleBrowser.Navigate(item.ReleaseNotesLink); //just show it raw
#endif
                    }
                    
                }
                else
                {
                    NetSparkleBrowser.Navigate(item.ReleaseNotesLink);
                }
            }
            else            
                RemoveReleaseNotesControls();

            imgAppIcon.Image = applicationIcon.ToBitmap();
            Icon = applicationIcon;

            this.TopMost = true;
        }

        private void ShowMarkdownReleaseNotes(NetSparkleAppCastItem item)
        {
            string contents;
            if (item.ReleaseNotesLink.StartsWith("file://")) //handy for testing
            {
                contents = File.ReadAllText(item.ReleaseNotesLink.Replace("file://", ""));
            }
            else
            {
                using (var webClient = new WebClient())
                {
                    contents = webClient.DownloadString(item.ReleaseNotesLink);
                }
            }
            var md = new MarkdownSharp.Markdown();
            var htmlTempFile = TempFile.WithExtension("htm"); //enhance: will leek a file to temp
            File.WriteAllText(htmlTempFile.Path, md.Transform(contents));
            NetSparkleBrowser.Navigate(htmlTempFile.Path);
        }

        void NetSparkleBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if(!e.Url.OriginalString.StartsWith("http"))
            {
                try
                {
                    File.Delete(e.Url.OriginalString);
                }
                catch (Exception error)
                {
                    Debug.Fail(error.Message);
                }
            }
        }

        /// <summary>
        /// The current item being installed
        /// </summary>
        NetSparkleAppCastItem INetSparkleForm.CurrentItem
        {
            get { return _currentItem; }
            set { _currentItem = value; }
        }

        /// <summary>
        /// The result of ShowDialog()
        /// </summary>
        DialogResult INetSparkleForm.Result
        {
            get { return this.DialogResult; }
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
            base.ShowDialog();
            if (UserResponded != null)
            {
                UserResponded(this, new EventArgs());
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
            Size newSize = new Size(this.Size.Width, this.Size.Height - label3.Height - panel1.Height);

            // remove the no more needed controls            
            label3.Parent.Controls.Remove(label3);
            NetSparkleBrowser.Parent.Controls.Remove(NetSparkleBrowser);
            panel1.Parent.Controls.Remove(panel1);

            // resize the window
            /*this.MinimumSize = newSize;
            this.Size = this.MinimumSize;
            this.MaximumSize = this.MinimumSize;*/
            this.Size = newSize;
        }

        /// <summary>
        /// Event called when the skip button is clicked
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private void OnSkipButtonClick(object sender, EventArgs e)
        {
            // set the dialog result to no
            this.DialogResult = DialogResult.No;

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
            this.DialogResult = DialogResult.Retry;

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
    }
}
