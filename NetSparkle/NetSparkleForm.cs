using System;
using System.Drawing;
using System.Windows.Forms;

namespace AppLimit.NetSparkle
{
    public partial class NetSparkleForm : Form
    {
        NetSparkleAppCastItem _currentItem;
        
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
        }

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

        private void skipButton_Click(object sender, EventArgs e)
        {
            // set the dialog result to no
            this.DialogResult = DialogResult.No;

            // close the windows
            Close();
        }

        private void buttonRemind_Click(object sender, EventArgs e)
        {
            // set the dialog result ot retry
            this.DialogResult = DialogResult.Retry;

            // close the window
            Close();
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            // set the result to yes
            DialogResult = DialogResult.Yes;

            // close the dialog
            Close();
        }
    }
}
