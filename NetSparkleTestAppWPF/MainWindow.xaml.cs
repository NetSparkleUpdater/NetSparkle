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
            _sparkle = new Sparkle("https://deadpikle.github.io/NetSparkle/files/sample-app/appcast.xml", icon); //, "NetSparkleTestApp.exe");
            _sparkle.StartLoop(true, true);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            _sparkle.CheckForUpdatesAtUserRequest();
        }
    }
}
