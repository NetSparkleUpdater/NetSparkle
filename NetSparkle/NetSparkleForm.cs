using System;
using System.Drawing;
using System.Windows.Forms;
using AppLimit.NetSparkle.Interfaces;

namespace AppLimit.NetSparkle
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
        /// <param name="appIcon"></param>
        /// <param name="windowIcon"></param>
        public NetSparkleForm(NetSparkleAppCastItem item, Image appIcon, Icon windowIcon)
        {            
            InitializeComponent();
            
            // init ui 
            try
            {
                NetSparkleBrowser.AllowWebBrowserDrop = false;
                NetSparkleBrowser.AllowNavigation = false;
            }
            catch (Exception)
            { }
            
            _currentItem = item;

            lblHeader.Text = lblHeader.Text.Replace("APP", item.AppName);
            lblInfoText.Text = lblInfoText.Text.Replace("APP", item.AppName + " " + item.Version);
            lblInfoText.Text = lblInfoText.Text.Replace("OLDVERSION", item.AppVersionInstalled);

            if (item.ReleaseNotesLink != null && item.ReleaseNotesLink.Length > 0 )
                NetSparkleBrowser.Navigate(item.ReleaseNotesLink);
            else            
                RemoveReleaseNotesControls();            

            if (appIcon != null)
                imgAppIcon.Image = appIcon;

            if (windowIcon != null)
                Icon = windowIcon;

            this.TopMost = true;
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
