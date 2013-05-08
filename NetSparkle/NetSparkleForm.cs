using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using NetSparkle.Interfaces;

namespace NetSparkle
{
    /// <summary>
    /// The main form
    /// </summary>
    public partial class NetSparkleForm : Form, INetSparkleForm
    {
        NetSparkleAppCastItem _currentItem;
        private TempFile _htmlTempFile;

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in browser init: " + ex.Message);
            }
            
            _currentItem = item;
           

            lblHeader.Text = lblHeader.Text.Replace("APP", item.AppName);
            lblInfoText.Text = lblInfoText.Text.Replace("APP", item.AppName + " " + item.Version);
            lblInfoText.Text = lblInfoText.Text.Replace("OLDVERSION", item.AppVersionInstalled);

            if (!string.IsNullOrEmpty(item.ReleaseNotesLink))
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

            TopMost = true;
        }

        /// <summary>
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _htmlTempFile.Dispose();//will try to delete it
            }
            catch
            {
                //not worth complaining about, just leaks to temp folder
            }
            base.OnClosing(e);
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
            _htmlTempFile = TempFile.WithExtension("htm");
            File.WriteAllText(_htmlTempFile.Path, md.Transform(contents));
            NetSparkleBrowser.Navigate(_htmlTempFile.Path);
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
    }
}
