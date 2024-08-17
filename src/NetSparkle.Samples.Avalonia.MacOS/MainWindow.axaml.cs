using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using System.IO;
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
            _sparkle = new CustomSparkleUpdater("https://netsparkleupdater.github.io/NetSparkle/files/sample-app-macos/appcast.xml", new Ed25519Checker(Enums.SecurityMode.Strict, "8zPswEwycU7XQ7OcGQtI/b22pWo1qM2Ual2OhssaDyI="))
            {
                UIFactory = new NetSparkleUpdater.UI.Avalonia.UIFactory(Icon),
                LogWriter = new LogWriter(LogWriterOutputMode.Console)
                //UseNotificationToast = false // Avalonia version doesn't yet support notification toast messages
            };
            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkle.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
            StartSparkle();
        }

        private async void StartSparkle()
        {
            await _sparkle.StartLoop(true, true);
        }

        public async Task ManualUpdateCheck_Click(object sender, RoutedEventArgs e)
        {
            await _sparkle.CheckForUpdatesAtUserRequest();
        }
    }
}
