using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetSparkle
{
    public partial class CheckingForUpdatesWindow : Form
    {
        public CheckingForUpdatesWindow()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedDialog;
        }

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

        private void Cancel_Click(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
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
                this.Invoke((MethodInvoker)delegate () { Close(); });
               // Close();
            }
        }
    }
}
