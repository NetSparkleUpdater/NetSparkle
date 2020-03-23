using System;
using System.Collections.Generic;
using System.Text;
using WPFTemplate.Helpers;

namespace NetSparkle.UI.WPF.ViewModels
{
    public class MessageNotificationWindowViewModel : ChangeNotifier
    {
        private string _message;

        public MessageNotificationWindowViewModel()
        {
            Message = "";
        }

        public MessageNotificationWindowViewModel(string message)
        {
            Message = message;
        }

        public string Message
        {
            get => _message;
            set { _message = value; NotifyPropertyChanged(); }
        }
    }
}
