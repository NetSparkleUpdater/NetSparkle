using NetSparkle.Enums;
using NetSparkle.Interfaces;
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

namespace NetSparkle.UI.NetFramework.WPF
{
    /// <summary>
    /// Interaction logic for UpdateAvailableWindow.xaml
    /// </summary>
    public partial class UpdateAvailableWindow : Window, IUpdateAvailable
    {
        public UpdateAvailableWindow()
        {
            InitializeComponent();
        }

        public Sparkle Sparkle { get; set; }

        public AppCastItem[] Updates { get; set; }
        
        public  bool IsUpdateAlreadyDownloaded { get; set; }

        UpdateAvailableResult IUpdateAvailable.Result => UpdateAvailableResult.None; // TODO: Set actual result 
        // actually the result should be sent back in the UserResponded event! that would simplify the event handling a lot)

        AppCastItem IUpdateAvailable.CurrentItem => null; // TODO: actual value

        public event EventHandler UserResponded;

        void IUpdateAvailable.BringToFront()
        {
            Activate();
        }

        void IUpdateAvailable.Close()
        {
            Close();
        }

        void IUpdateAvailable.HideReleaseNotes()
        {
            // TODO: hide release notes
        }

        void IUpdateAvailable.HideRemindMeLaterButton()
        {
            // TODO: hide remind me later button
        }

        void IUpdateAvailable.HideSkipButton()
        {
            // TODO: hide skip button
        }

        void IUpdateAvailable.Show()
        {
            Show();
        }
    }
}
