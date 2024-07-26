using NetSparkleUpdater.UI.Avalonia.Helpers;

namespace NetSparkleUpdater.UI.Avalonia.ViewModels
{
    /// <summary>
    /// A view model for showing a single message to the user
    /// </summary>
    public class MessageNotificationWindowViewModel : ChangeNotifier
    {
        private string _message;

        /// <summary>
        /// Initialize the view model with an empty string for its message
        /// </summary>
        public MessageNotificationWindowViewModel()
        {
            _message = "";
        }

        /// <summary>
        /// Initialize the view model with the given message
        /// </summary>
        /// <param name="message">the message to show the user</param>
        public MessageNotificationWindowViewModel(string message)
        {
            _message = message;
        }

        /// <summary>
        /// The message to show to the user
        /// </summary>
        public string Message
        {
            get => _message;
            set { _message = value; NotifyPropertyChanged(); }
        }
    }
}
