using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetSparkle.Samples.NetCore.WPF
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
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree("Software\\Microsoft\\NetSparkle.TestAppNetCoreWPF");
            }
            catch { }

            // set icon in project properties!
            string manifestModuleName = System.Reflection.Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName;
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(manifestModuleName);
            _sparkle = new Sparkle("https://deadpikle.github.io/NetSparkle/files/sample-app/appcast.xml")
            {
                UIFactory = new NetSparkle.UI.WPF.UIFactory(NetSparkle.UI.WPF.IconUtilities.ToImageSource(icon)),
                ShowsUIOnMainThread = false
                //UseNotificationToast = true
            };
            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkle.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
            _sparkle.StartLoop(true, true);
        }

        /// <summary>
        /// Convert System.Drawing.Icon to System.Windows.Media.ImageSource.
        ///  From: https://stackoverflow.com/a/6580799/3938401
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static ImageSource ToImageSource(System.Drawing.Icon icon)
        {
            if (icon == null)
            {
                return null;
            }

            return imageSource;
        }

        private void ManualUpdateCheck_Click(object sender, RoutedEventArgs e)
        {
            _sparkle.CheckForUpdatesAtUserRequest();
        }
    }
}
