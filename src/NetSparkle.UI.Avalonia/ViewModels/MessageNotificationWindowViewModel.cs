using System;
using System.Collections.Generic;
using System.Text;
using NetSparkleUpdater.UI.Avalonia.Helpers;
using NetSparkleUpdater.UI.Avalonia.Interfaces;

namespace NetSparkleUpdater.UI.Avalonia.ViewModels
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
