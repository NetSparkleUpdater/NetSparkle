using NetSparkle.UI.WPF.ViewModels;
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

namespace NetSparkle.UI.WPF
{
    /// <summary>
    /// Interaction logic for MessageNotificationWindow.xaml
    /// </summary>
    public partial class MessageNotificationWindow : Window
    {
        public MessageNotificationWindow()
        {
            InitializeComponent();
            DataContext = new MessageNotificationWindowViewModel();
        }

        public MessageNotificationWindow(MessageNotificationWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
