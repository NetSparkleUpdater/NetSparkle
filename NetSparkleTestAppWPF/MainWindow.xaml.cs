using System.Drawing;
using System.Windows;


namespace NetSparkle.TestAppWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Sparkle _sparkle;

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
            //_sparkle = new Sparkle("https://deadpikle.github.io/NetSparkle/files/sample-app/appcast.xml", icon); //, "NetSparkleTestApp.exe");

            _sparkle = new Sparkle(
                "https://api.appcenter.ms/v0.1/public/sparkle/apps/c27e6512-ceae-4665-82c5-c0d7f6a27f30",
                icon,
                NetSparkle.Enums.SecurityMode.Unsafe); //, "NetSparkleTestApp.exe");


            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkle.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
            _sparkle.StartLoop(true, true);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            _sparkle.CheckForUpdatesAtUserRequest();
        }
    }
}
