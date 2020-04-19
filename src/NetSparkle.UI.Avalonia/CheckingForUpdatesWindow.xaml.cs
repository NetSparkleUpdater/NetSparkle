using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using NetSparkleUpdater.Interfaces;
using System;

namespace NetSparkleUpdater.UI.Avalonia
{
    public class CheckingForUpdatesWindow : Window, ICheckingForUpdates
    {
        public event EventHandler UpdatesUIClosing;

        public CheckingForUpdatesWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Closing += CheckingForUpdatesWindow_Closing;
        }

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

        public void Cancel()
        {
            Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
