using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;

namespace NetSparkleUpdater.Samples.NetCore.WinForms
{
    public partial class Form1 : Form
    {
        private SparkleUpdater _sparkleUpdateDetector;

        public Form1()
        {
            InitializeComponent();

            var appcastUrl = "https://netsparkleupdater.github.io/NetSparkle/files/sample-app/appcast.xml";
            // set icon in project properties!
            string manifestModuleName = System.Reflection.Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName;
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(manifestModuleName);
            _sparkleUpdateDetector = new SparkleUpdater(appcastUrl, new DSAChecker(Enums.SecurityMode.Strict))
            {
                UIFactory = new NetSparkleUpdater.UI.WinForms.UIFactory(icon),
                //RelaunchAfterUpdate = true,
                //ShowsUIOnMainThread = true,
                //UseNotificationToast = true
            };
            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkleUpdateDetector.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
            //_sparkleUpdateDetector.CloseApplication += _sparkleUpdateDetector_CloseApplication;
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
