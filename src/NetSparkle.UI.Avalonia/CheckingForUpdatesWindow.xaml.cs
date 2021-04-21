using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using NetSparkleUpdater.Interfaces;
using System;

namespace NetSparkleUpdater.UI.Avalonia
{
    /// <summary>
    /// Interaction logic for CheckingForUpdatesWindow.xaml.
    /// 
    /// Window that shows while NetSparkle is checking for updates.
    /// </summary>
    public class CheckingForUpdatesWindow : Window, ICheckingForUpdates
    {
        /// <inheritdoc/>
        public event EventHandler UpdatesUIClosing;

        /// <summary>
        /// Create the window that tells the user that SparkleUpdater is checking
        /// for updates
        /// </summary>
        public CheckingForUpdatesWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Closing += CheckingForUpdatesWindow_Closing;
        }

        /// <summary>
        /// Create the window that tells the user that SparkleUpdater is checking
        /// for updates with a given bitmap to use as an icon/graphic
        /// </summary>
        /// <param name="iconBitmap">The bitmap to use for the application logo/graphic</param>
        public CheckingForUpdatesWindow(IBitmap iconBitmap)
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Closing += CheckingForUpdatesWindow_Closing;
            var imageControl = this.FindControl<Image>("AppIcon");
            if (imageControl != null)
            {
                imageControl.Source = iconBitmap;
            }
        }

        private void CheckingForUpdatesWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= CheckingForUpdatesWindow_Closing;
            UpdatesUIClosing?.Invoke(sender, new EventArgs());
        }

        void ICheckingForUpdates.Close()
        {
            Close();
        }

        void ICheckingForUpdates.Show()
        {
            Show();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
