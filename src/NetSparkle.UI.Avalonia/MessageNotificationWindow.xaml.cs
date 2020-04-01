using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NetSparkle.UI.Avalonia.ViewModels;

namespace NetSparkle.UI.Avalonia
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

        public MessageNotificationWindow(MessageNotificationWindowViewModel viewModel)
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = viewModel;
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
