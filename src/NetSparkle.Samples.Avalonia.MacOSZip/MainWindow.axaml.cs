using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using NetSparkleUpdater.SignatureVerifiers;
using NetSparkleUpdater.UI.Avalonia;
using System.IO;

namespace NetSparkleUpdater.Samples.Avalonia
{
    public partial class MainWindow : Window
    {
        private SparkleUpdater _sparkle;

        public MainWindow()
        {
            this.InitializeComponent();
            // set icon in project properties!
            //var zipAppcast = "https://netsparkleupdater.github.io/NetSparkle/files/sample-app-macos-zip/appcast.xml";
            var tarAppcast = "https://netsparkleupdater.github.io/NetSparkle/files/sample-app-macos-tar/appcast.xml";
            var appcastToUse = tarAppcast; // can swap to zipAppcast when testing
            string manifestModuleName = System.Reflection.Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName;
            _sparkle = new CustomSparkleUpdater(appcastToUse, new Ed25519Checker(Enums.SecurityMode.Strict, "8zPswEwycU7XQ7OcGQtI/b22pWo1qM2Ual2OhssaDyI="))
            {
                UIFactory = new NetSparkleUpdater.UI.Avalonia.UIFactory(Icon),
                LogWriter = new LogWriter(),
                RelaunchAfterUpdate = true,
                //UseNotificationToast = false // Avalonia version doesn't yet support notification toast messages

                // if you want to restart your dotnet app without a bundle, you can do something like this:
                //RelaunchAfterUpdateCommandPrefix = "dotnet ",
                //RestartExecutableName = "NetSparkleUpdater.Samples.Avalonia.dll"
                // for this sample app's purposes, since it's downloading a .app that doesn't match this sample binary, we do this:
                // xattr -dr SimpleApp.app ---- this removes the com.apple.quarantine bit which fixes an issue launching the app.
                // Your milage may vary, please test yourself!
                RelaunchAfterUpdateCommandPrefix = "xattr -dr SimpleApp.app; open -n ",
                RestartExecutableName = "SimpleApp.app", // path to binary inside .app
            };
            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkle.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
            _sparkle.StartLoop(true, true);
        }

        public async void ManualUpdateCheck_Click(object sender, RoutedEventArgs e)
        {
            await _sparkle.CheckForUpdatesAtUserRequest();
        }
    }
}
