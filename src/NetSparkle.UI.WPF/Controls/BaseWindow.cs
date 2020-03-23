using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;

namespace NetSparkle.UI.WPF.Controls
{
    public class BaseWindow : Window
    {
        protected bool _isOnMainThread;
        protected bool _hasInitiatedShutdown = false;

        public BaseWindow()
        {

        }

        public BaseWindow(bool useClosingEvent)
        {
            if (useClosingEvent)
            {
                Closing += BaseWindow_Closing;
            }
        }

        private void BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= BaseWindow_Closing;
            if (!_isOnMainThread && !_hasInitiatedShutdown)
            {
                _hasInitiatedShutdown = true;
                Dispatcher.InvokeShutdown();
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

        protected void BringToFront()
        {
            Topmost = true;
            Activate();
            Topmost = false;
        }
    }
}
