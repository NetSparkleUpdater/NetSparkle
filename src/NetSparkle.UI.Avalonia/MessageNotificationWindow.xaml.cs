using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using NetSparkleUpdater.UI.Avalonia.ViewModels;

namespace NetSparkleUpdater.UI.Avalonia
{
    /// <summary>
    /// Interaction logic for MessageNotificationWindow.xaml.
    /// 
    /// Window that shows a single message to the user (usually an error) regarding
    /// a software update.
    /// </summary>
    public partial class MessageNotificationWindow : Window
    {
        /// <summary>
        /// Construct the notification window for the message notification with the default
        /// <seealso cref="MessageNotificationWindowViewModel"/>.
        /// </summary>
        public MessageNotificationWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = new MessageNotificationWindowViewModel();
        }

        /// <summary>
        /// Construct the notification window for the message notification with the provided
        /// <seealso cref="MessageNotificationWindowViewModel"/>
        /// </summary>
        /// <param name="viewModel">view model that has info on the message to show to the user</param>
        /// <param name="iconBitmap">Bitmap to use for the app's icon/graphic. Not currently used.</param>
        public MessageNotificationWindow(MessageNotificationWindowViewModel viewModel, Bitmap iconBitmap)
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

        /// <summary>
        /// Close the message window
        /// </summary>
        public void CloseMessage()
        {
            Close();
        }
    }
}
