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
    public partial class MessageNotificationWindow : Form
    {
        public MessageNotificationWindow()
        {
            InitializeComponent();
        }

        public MessageNotificationWindow(string title, string message, Icon applicationIcon = null)
        {
            InitializeComponent();
            Text = title;
            if (applicationIcon != null)
            {
                Icon = applicationIcon;
                iconImage.Image = new Icon(applicationIcon, new Size(48, 48)).ToBitmap();
            }
            lblMessage.Text = message;
            FormBorderStyle = FormBorderStyle.FixedDialog;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
