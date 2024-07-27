using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetSparkleUpdater.UI.WinForms
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

        private void CheckingForUpdatesWindow_FormClosing(object? sender, FormClosingEventArgs e)
        {
            UpdatesUIClosing?.Invoke(sender, new EventArgs());
            FormClosing -= CheckingForUpdatesWindow_FormClosing;
        }

        /// <summary>
        /// Initializes window and sets the icon to <paramref name="applicationIcon"/>
        /// </summary>
        /// <param name="applicationIcon">The icon to use</param>
        public CheckingForUpdatesWindow(Icon? applicationIcon = null)
        {
            InitializeComponent();
            if (applicationIcon != null)
            {
                Icon = applicationIcon;
                iconImage.Image = new Icon(applicationIcon, new Size(48, 48)).ToBitmap();
            }
            FormBorderStyle = FormBorderStyle.FixedDialog;
        }

        /// <summary>
        /// Event that is called when the UI for the checking for updates window is closing
        /// </summary>
        public event EventHandler? UpdatesUIClosing;

        void ICheckingForUpdates.Close()
        {
            CloseForm();
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
    }
}
