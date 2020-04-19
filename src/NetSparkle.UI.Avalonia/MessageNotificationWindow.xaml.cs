using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using NetSparkleUpdater.UI.Avalonia.ViewModels;

namespace NetSparkleUpdater.UI.Avalonia
{
    public class MessageNotificationWindow : Window
    {
        public MessageNotificationWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = new MessageNotificationWindowViewModel();
        }

        public MessageNotificationWindow(MessageNotificationWindowViewModel viewModel, IBitmap iconBitmap)
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = viewModel;
            /*var imageControl = this.FindControl<Image>("AppIcon");
            if (imageControl != null)
            {
                imageControl.Source = iconBitmap;
            }*/
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void CloseMessage()
        {
            Close();
        }
    }
}
