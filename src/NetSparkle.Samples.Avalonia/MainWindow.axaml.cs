using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
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
            InitializeComponent();
            // set icon in project properties!
            string manifestModuleName = System.Reflection.Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName;
            _sparkle = new SparkleUpdater("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/appcast.xml", new DSAChecker(Enums.SecurityMode.Strict))
            {
                UIFactory = new NetSparkleUpdater.UI.Avalonia.UIFactory(Icon)
                {
                    // use the following property to change the main grid background on the update window. nullable.
                    //UpdateWindowGridBackgroundBrush = new SolidColorBrush(Colors.Purple) 
                },
                //UseNotificationToast = false // Avalonia version doesn't yet support notification toast messages
            };
            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkle.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
            _sparkle.StartLoop(true, true);
        }

        public async void ManualUpdateCheck_Click()
        {
            await _sparkle.CheckForUpdatesAtUserRequest();
        }
    }
}
