using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater.UI.Avalonia.Helpers
{
    /// <summary>
    /// <see cref="INotifyPropertyChanged"/> implementation used for the WPF UI and its view models.
    /// <para>
    /// https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=netframework-4.7.2
    /// </para>
    /// </summary>
    public class ChangeNotifier : INotifyPropertyChanged
    {
        /// <summary>
        /// Event to listen to property changes on some object
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify the listener that a property of the given name has changed
        /// </summary>
        /// <param name="propertyName">string name of the property that was changed</param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
