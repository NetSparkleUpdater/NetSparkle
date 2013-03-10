using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetSparkle;

namespace NetSparkle.TestAppWPF
{
    public partial class Form1 : Form
    {
        private Sparkle _sparkle = new Sparkle("http://update.applimit.com/netsparkle/versioninfo.xml", SystemIcons.Application); 

        public Form1()
        {
            InitializeComponent();

            // remove the netsparkle key from registry 
            try
            {
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree("Software\\Microsoft\\NetSparkleTestApp");
            }
            catch { }

            _sparkle.StartLoop(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _sparkle.StopLoop();
            Close();
        }
    }
}
