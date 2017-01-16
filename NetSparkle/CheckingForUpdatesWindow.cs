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
    }
}
