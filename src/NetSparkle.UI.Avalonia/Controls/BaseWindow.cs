using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;

namespace NetSparkleUpdater.UI.Avalonia.Controls
{
    /// <summary>
    /// Base <see cref="Window"/> class for most WPF update windows.
    /// Provides utilities and common code for windows that are shown.
    /// </summary>
    public class BaseWindow : Window
    {
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
            _hasInitiatedShutdown = false;
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
            _hasInitiatedShutdown = false;
        }

        private void BaseWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= BaseWindow_Closing;
            if (!_hasInitiatedShutdown)
            {
                _hasInitiatedShutdown = true;
            }
        }

        /// <summary>
        /// Show this window to the user.
        /// </summary>
        protected void ShowWindow()
        {
            Show();
        }

        /// <summary>
        /// Close the window and shut down the UI dispatcher if necessary
        /// </summary>
        protected void CloseWindow()
        {
            // make sure to close the window on the thread it has been started on
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Close();
                if (!_hasInitiatedShutdown)
                {
                    _hasInitiatedShutdown = true;
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
