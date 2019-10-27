using NetSparkle.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetSparkle.NetFramework.WinForms
{
    /// <summary>
    /// The checking for updates window
    /// </summary>
    public partial class CheckingForUpdatesWindow : Form, ICheckingForUpdates
    {
        /// <summary>
        /// Default constructor for CheckingForUpdatesWindow
        /// </summary>
        public CheckingForUpdatesWindow()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedDialog;
            FormClosing += CheckingForUpdatesWindow_FormClosing;
        }

        private void CheckingForUpdatesWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            UpdatesUIClosing?.Invoke(sender, new EventArgs());
            FormClosing -= CheckingForUpdatesWindow_FormClosing;
        }

        /// <summary>
        /// Initializes window and sets the icon to <paramref name="applicationIcon"/>
        /// </summary>
        /// <param name="applicationIcon">The icon to use</param>
        public CheckingForUpdatesWindow(Icon applicationIcon = null)
        {
            InitializeComponent();
            if (applicationIcon != null)
            {
                Icon = applicationIcon;
                iconImage.Image = new Icon(applicationIcon, new Size(48, 48)).ToBitmap();
            }
            FormBorderStyle = FormBorderStyle.FixedDialog;
        }


        public event EventHandler UpdatesUIClosing;
        //public event EventHandler ICheckingForUpdates.Closing;

        void ICheckingForUpdates.Close()
        {
            Close();
        }
        void ICheckingForUpdates.Show()
        {
            Show();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
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

    }
}
