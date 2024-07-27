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
        /// Whether or not this Window is on the main thread or not
        /// </summary>
        protected bool _isOnMainThread;
        /// <summary>
        /// Whether or not the close window code has been called yet
        /// (so that it is not called more than one time).
        /// </summary>
        protected bool _hasInitiatedShutdown = false;
        
        /// <summary>
        /// Cancellation token used when showing this window on the main UI dispatcher
        /// </summary>
        protected CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Public, default construtor for BaseWindow objects
        /// </summary>
        public BaseWindow()
        {
            _cancellationTokenSource = new CancellationTokenSource();
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
            _cancellationTokenSource = new CancellationTokenSource();
            _hasInitiatedShutdown = false;
        }

        private void BaseWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= BaseWindow_Closing;
            if (!_isOnMainThread && !_hasInitiatedShutdown)
            {
                _hasInitiatedShutdown = true;
                _cancellationTokenSource.Cancel();
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
                    Dispatcher.UIThread.MainLoop(_cancellationTokenSource.Token);
                }
            }
            catch (ThreadAbortException)
            {
                Close();
                if (!isOnMainThread)
                {
                    _cancellationTokenSource.Cancel();
                }
            }
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
                if (!_isOnMainThread && !_hasInitiatedShutdown)
                {
                    _hasInitiatedShutdown = true;
                    _cancellationTokenSource.Cancel();
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
