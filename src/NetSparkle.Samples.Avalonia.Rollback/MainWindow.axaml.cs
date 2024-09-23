using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using NetSparkleUpdater.SignatureVerifiers;
using NetSparkleUpdater.UI.Avalonia;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NetSparkleUpdater.Samples.Avalonia
{
    public partial class MainWindow : Window
    {
        private SparkleUpdater _sparkle;

        public MainWindow()
        {
            InitializeComponent();
            // set icon in project properties!
            string manifestModuleName = System.Reflection.Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName;
            _sparkle = new CustomSparkleUpdater("https://netsparkleupdater.github.io/NetSparkle/files/console-sample-app/appcast.xml", new Ed25519Checker(Enums.SecurityMode.Strict,"B7B7xMPTz7+q4FGiiFFpO+bnOygXBt4FZSOGJXrBX4U="))
            {
                UIFactory = new NetSparkleUpdater.UI.Avalonia.UIFactory(Icon),
                RelaunchAfterUpdate = false
                //UseNotificationToast = false // Avalonia version doesn't yet support notification toast messages
            };
            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkle.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
            var filter = new OSAppCastFilter(_sparkle.LogWriter);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                filter.OSName = "windows-x64";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64)
                {
                    Console.WriteLine("Using macos-arm64");
                    filter.OSName = "macos-arm64";
                }
                else
                {
                    Console.WriteLine("Using macos-x64");
                    filter.OSName = "macos-x64";
                }
            }
            else
            {
                filter.OSName = "linux-x64";
            }
            _sparkle.AppCastHelper.AppCastFilter = filter;
            StartSparkle();
        }

        private async void StartSparkle()
        {
            await _sparkle.StartLoop(true, true);
        }

        public async Task ManualUpdateCheck_Click()
        {
            await _sparkle.CheckForUpdatesAtUserRequest();
        }
    }
}
