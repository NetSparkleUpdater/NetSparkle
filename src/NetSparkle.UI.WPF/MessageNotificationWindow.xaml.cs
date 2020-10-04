using NetSparkleUpdater.UI.WPF.ViewModels;
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

namespace NetSparkleUpdater.UI.WPF
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
            InitializeComponent();
            DataContext = new MessageNotificationWindowViewModel();
        }

        /// <summary>
        /// Construct the notification window for the message notification with the provided
        /// <seealso cref="MessageNotificationWindowViewModel"/>
        /// </summary>
        /// <param name="viewModel">view model that has info on the message to show to the user</param>
        public MessageNotificationWindow(MessageNotificationWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
