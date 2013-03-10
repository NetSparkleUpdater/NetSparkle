using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NetSparkle;

namespace SampleApplication
{
    public partial class Form1 : Form
    {
        private Sparkle _sparkleUpdateDetector;

        public Form1()
        {
            InitializeComponent();

            var appcastUrl = "file://" + DirectoryOfTheApplicationExecutable + "../../../../Extras/Sample Appcast.xml";
            _sparkleUpdateDetector = new Sparkle(appcastUrl, SystemIcons.Application);
            _sparkleUpdateDetector.CheckOnFirstApplicationIdle();
        }


        public static string DirectoryOfTheApplicationExecutable
        {
            get
            { 
                string path;
                path = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
                path = Uri.UnescapeDataString(path);
                return Directory.GetParent(path).FullName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _sparkleUpdateDetector.CheckForUpdates(false);
        }
    }
}
