using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NetSparkle;
using NetSparkle.Enums;

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
                UIFactory = new NetSparkle.UI.NetFramework.WinForms.UIFactory(),
                UseNotificationToast = true
            };
            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkleUpdateDetector.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
            _sparkleUpdateDetector.CloseApplication += _sparkleUpdateDetector_CloseApplication;
            _sparkleUpdateDetector.StartLoop(true, true);
        }

        private void _sparkleUpdateDetector_CloseApplication()
        {
            Application.Exit();
        }

        private async void AppBackgroundCheckButton_Click(object sender, EventArgs e)
        {
            // Manually check for updates, this will not show a ui
            var result = await _sparkleUpdateDetector.CheckForUpdatesQuietly();
            if (result.Status == UpdateStatus.UpdateAvailable)
            {
                // if update(s) are found, then we have to trigger the UI to show it gracefully
                _sparkleUpdateDetector.ShowUpdateNeededUI();
            }

        }

        private void ExplicitUserRequestCheckButton_Click(object sender, EventArgs e)
        {
            _sparkleUpdateDetector.CheckForUpdatesAtUserRequest();
        }
    }
}
