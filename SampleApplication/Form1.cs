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

            var appcastUrl = "https://deadpikle.github.io/NetSparkle/files/sample-app/appcast.xml";
            // set icon in project properties!
            string manifestModuleName = System.Reflection.Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName;
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(manifestModuleName);
            _sparkleUpdateDetector = new Sparkle(appcastUrl, icon)
            {
                UIFactory = new NetSparkle.NetFramework.WinForms.WinFormsUIFactory()
            };
            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkleUpdateDetector.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
            _sparkleUpdateDetector.CheckOnFirstApplicationIdle();
            _sparkleUpdateDetector.CloseApplication += _sparkleUpdateDetector_CloseApplication;
        }

        private void _sparkleUpdateDetector_CloseApplication()
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _sparkleUpdateDetector.CheckForUpdatesQuietly();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _sparkleUpdateDetector.CheckForUpdatesAtUserRequest();
        }
    }
}
