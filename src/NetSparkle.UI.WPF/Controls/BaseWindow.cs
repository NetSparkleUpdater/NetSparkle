using System.Threading;
using System.Windows;

namespace NetSparkleUpdater.UI.WPF.Controls
{
    /// <summary>
    /// Base <see cref="Window"/> class for most WPF update windows.
    /// Provides utilities and common code for windows that are shown.
    /// </summary>
    public class BaseWindow : Window
    {
        /// <summary>
        /// Whether or not this Window is on the main thread or not
        /// </summary>
        protected bool _isOnMainThread;
        /// <summary>
        /// Whether or not the close window code has been called yet
        /// (so that it is not called more than one time).
        /// </summary>
        protected bool _hasInitiatedShutdown = false;

        /// <summary>
        /// Public, default construtor for BaseWindow objects
        /// </summary>
        public BaseWindow()
        {

        }

        /// <summary>
        /// Create a BaseWindow and set up the closing event handler
        /// if requested
        /// </summary>
        /// <param name="useClosingEvent">true to use the <see cref="Window.Closing"/>
        /// event handler; false otherwise</param>
        public BaseWindow(bool useClosingEvent)
        {
            if (useClosingEvent)
            {
                Closing += BaseWindow_Closing;
            }
        }

        private void BaseWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= BaseWindow_Closing;
            if (!_isOnMainThread && !_hasInitiatedShutdown)
            {
                _hasInitiatedShutdown = true;
                Dispatcher.InvokeShutdown();
            }
        }

        /// <summary>
        /// Show this window to the user.
        /// </summary>
        /// <param name="isOnMainThread">true if the window is being shown while
        /// on the main thread; false otherwise</param>
        protected void ShowWindow(bool isOnMainThread)
        {
            try
            {
                Show();
                _isOnMainThread = isOnMainThread;
                if (!isOnMainThread)
                {
                    // https://stackoverflow.com/questions/1111369/how-do-i-create-and-show-wpf-windows-on-separate-threads
                    System.Windows.Threading.Dispatcher.Run();
                }
            }
            catch (ThreadAbortException)
            {
                Close();
                if (!isOnMainThread)
                {
                    Dispatcher.InvokeShutdown();
                }
            }
        }

        /// <summary>
        /// Close the window and shut down the UI dispatcher if necessary
        /// </summary>
        protected void CloseWindow()
        {
            // make sure to close the window on the thread it has been started on
            Dispatcher.InvokeAsync(() =>
            {
                Close();
                if (!_isOnMainThread && !_hasInitiatedShutdown)
                {
                    _hasInitiatedShutdown = true;
                    Dispatcher.InvokeShutdown();
                }
            });
        }

        /// <summary>
        /// Bring this window to the front of all other windows by temporarily
        /// setting Topmost to true.
        /// </summary>
        protected void BringToFront()
        {
            Topmost = true;
            Activate();
            Topmost = false;
        }
    }
}
