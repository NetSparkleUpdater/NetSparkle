using NetSparkleUpdater.SignatureVerifiers;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;


namespace NetSparkleUpdater.Samples.NetFramework.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SparkleUpdater _sparkle;

        public MainWindow()
        {
            InitializeComponent();

            // remove the netsparkle key from registry 
            try
            {
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree("Software\\Microsoft\\NetSparkle.TestAppWPF");
            }
            catch { }

            // set icon in project properties!
            string manifestModuleName = System.Reflection.Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName;
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(manifestModuleName);

            _sparkle = new SparkleUpdater("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/appcast.xml", new DSAChecker(Enums.SecurityMode.Strict))
            {
                UIFactory = new NetSparkleUpdater.UI.WPF.UIFactory(NetSparkleUpdater.UI.WPF.IconUtilities.ToImageSource(icon))
                {
                    ProcessWindowAfterInit = (window, factory) =>
                    {
                        // Example of setting font styles on a window after init: 
                        TextBlock.SetFontStyle(window, FontStyles.Italic);
                    }
                },
                ShowsUIOnMainThread = false,
                //RelaunchAfterUpdate = true,
                //UseNotificationToast = true
            };
            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkle.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
            _sparkle.StartLoop(true, true);
        }

        private void ManualUpdateCheck_Click(object sender, RoutedEventArgs e)
        {
            _sparkle.CheckForUpdatesAtUserRequest();
        }
    }
}
