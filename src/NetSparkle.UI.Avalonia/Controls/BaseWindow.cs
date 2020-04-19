using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;

namespace NetSparkle.UI.Avalonia.Controls
{
    public class BaseWindow : Window
    {
        protected bool _isOnMainThread;
        protected bool _hasInitiatedShutdown;

        protected CancellationTokenSource _cancellationTokenSource;

        public BaseWindow()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _hasInitiatedShutdown = false;
        }

        public BaseWindow(bool useClosingEvent)
        {
            if (useClosingEvent)
            {
                Closing += BaseWindow_Closing;
            }
            _cancellationTokenSource = new CancellationTokenSource();
            _hasInitiatedShutdown = false;
        }

        private void BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= BaseWindow_Closing;
            if (!_isOnMainThread && !_hasInitiatedShutdown)
            {
                _hasInitiatedShutdown = true;
                _cancellationTokenSource.Cancel();
            }
        }

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

        protected void BringToFront()
        {
            Topmost = true;
            Activate();
            Topmost = false;
        }
    }
}
