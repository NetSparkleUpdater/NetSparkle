using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using AppLimit.NetSparkle;

namespace NetSparkleTestAppWPF
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
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree("Software\\Microsoft\\NetSparkleTestAppWPF");
            }
            catch { }

            _sparkle = new Sparkle("http://update.applimit.com/netsparkle/versioninfo.xml"); //, "NetSparkleTestApp.exe");
            _sparkle.ShowDiagnosticWindow = true;
            _sparkle.StartLoop(true, true);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            _sparkle.StopLoop();
            Close();
        }
    }
}
