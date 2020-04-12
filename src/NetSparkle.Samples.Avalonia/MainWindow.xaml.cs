using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using NetSparkle.UI.Avalonia;
using System.IO;

namespace NetSparkle.Samples.Avalonia
{
    public class MainWindow : Window
    {
        private Sparkle _sparkle;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            // set icon in project properties!
            string manifestModuleName = System.Reflection.Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName;
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(manifestModuleName); 
            MemoryStream iconStream = new MemoryStream();
            icon.Save(iconStream);
            iconStream.Seek(0, SeekOrigin.Begin); // TODO: this icon does not look right
            _sparkle = new SparkleAvalonia("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/appcast.xml")
            {
                UIFactory = new NetSparkle.UI.Avalonia.UIFactory(new WindowIcon(iconStream)),
                // Avalonia version doesn't support separate threads: https://github.com/AvaloniaUI/Avalonia/issues/3434#issuecomment-573446972
                ShowsUIOnMainThread = true,
                //UseNotificationToast = true // Avalonia version doesn't yet support notification toast messages
            };
            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkle.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
            _sparkle.StartLoop(true, true);
        }

        public async void ManualUpdateCheck_Click(object sender, RoutedEventArgs e)
        {
            await _sparkle.CheckForUpdatesAtUserRequest();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
