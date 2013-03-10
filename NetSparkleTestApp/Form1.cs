using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NetSparkle;

namespace NetSparkleTestApp
{
    public partial class Form1 : Form
    {
        private readonly Sparkle _sparkle;

        public Form1()
        {
            InitializeComponent();

            _sparkle = new Sparkle("file://" + DirectoryOfTheApplicationExecutable + "../../../../Extras/Sample Appcast.xml", SystemIcons.Application)
            //_sparkle = new Sparkle("https://update.applimit.com/netsparkle/versioninfo.xml")
            {
                TrustEverySSLConnection = true,
                //EnableSystemProfiling = true,
                //SystemProfileUrl = new Uri("http://update.applimit.com/netsparkle/stat/profileInfo.php")
            };

            //_sparkle.UpdateDetected += new UpdateDetected(_sparkle_updateDetected);
            //_sparkle.EnableSilentMode = true;
            //_sparkle.HideReleaseNotes = true;

            _sparkle.StartLoop(true);
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


        void _sparkle_updateDetected(object sender, UpdateDetectedEventArgs e)
        {
            DialogResult res = MessageBox.Show("Update detected, perform unattended", "Update", MessageBoxButtons.YesNoCancel);

            if (res == System.Windows.Forms.DialogResult.Yes)
                e.NextAction = NextUpdateAction.PerformUpdateUnattended;
            else if (res == System.Windows.Forms.DialogResult.Cancel)
                e.NextAction = NextUpdateAction.ProhibitUpdate;
            else
                e.NextAction = NextUpdateAction.ShowStandardUserInterface;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _sparkle.StopLoop();
        }

        private void btnStopLoop_Click(object sender, EventArgs e)
        {
            _sparkle.StopLoop();
        }

        private void btnTestLoop_Click(object sender, EventArgs e)
        {
            if (_sparkle.IsUpdateLoopRunning)
                MessageBox.Show("Loop is running");
            else
                MessageBox.Show("Loop is not running");
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            
        }
    }
}
