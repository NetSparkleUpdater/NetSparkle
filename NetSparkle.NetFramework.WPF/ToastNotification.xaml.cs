using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;

namespace NetSparkle.UI.NetFramework.WPF
{
    /// <summary>
    /// Interaction logic for ToastNotification.xaml
    /// </summary>
    public partial class ToastNotification : Window
    {
        private Timer _goUpTimer;
        private Timer _goDownTimer;
        private Timer _pauseTimer;
        private double _startPosX;
        private double _startPosY;
        private bool _hasInitiatedShutdown = false;

        private double _workAreaHeight;
        private double _workAreaWidth;

        public ToastNotification()
        {
            InitializeComponent();
            Topmost = true;
            // Toast doesn't need to be shown in task bar
            ShowInTaskbar = false;
            // Create and run timer for animation

            _goUpTimer = new Timer();
            // AutoReset = false so that timer stops when we want it to: https://stackoverflow.com/a/18280560/3938401
            _goUpTimer.AutoReset = false;
            _goUpTimer.Interval = 25;
            _goUpTimer.Elapsed += GoUpTimerTick;
            _goUpTimer.Start();

            _goDownTimer = new Timer();
            _goDownTimer.AutoReset = false;
            _goDownTimer.Interval = 25;
            _goDownTimer.Elapsed += GoDownTimerTick;

            _pauseTimer = new Timer();
            _pauseTimer.AutoReset = false;
            _pauseTimer.Interval = 15000;
            _pauseTimer.Elapsed += PauseTimerTick;

            _workAreaHeight = System.Windows.SystemParameters.WorkArea.Height;
            _workAreaWidth = System.Windows.SystemParameters.WorkArea.Width;
            Left = _startPosX = _workAreaWidth - Width;
            Top = _startPosY = _workAreaHeight + 10;
            Loaded += ToastNotification_Loaded;
        }

        private void ToastNotification_Loaded(object sender, RoutedEventArgs e)
        {
            // Begin animation
            _goUpTimer.Start();
        }

        public Action<List<AppCastItem>> ClickAction { get; set; }

        public List<AppCastItem> Updates { get; set; }
        
        private void PauseTimerTick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _pauseTimer.Stop();
                _pauseTimer.Dispose();
                _goDownTimer.Start();
            });
        }

        void GoUpTimerTick(object sender, EventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                // If window is fully visible stop the timer
                if (_startPosY < _workAreaHeight - Height)
                {
                    _goUpTimer.Stop();
                    _goUpTimer.Dispose();
                    _pauseTimer.Start();
                }
                else
                {
                    Left = _startPosX;
                    Top = _startPosY;
                    _goUpTimer.Enabled = true;
                }
                // Lift window by 5 pixels
                _startPosY -= 5;
            });
        }

        private void GoDownTimerTick(object sender, EventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                // If window is fully visible stop the timer
                if (_startPosY > _workAreaHeight + Height)
                {
                    _goDownTimer.Stop();
                    CloseToastMessage();
                }
                else
                {
                    Left = _startPosX;
                    Top = _startPosY;
                    _goDownTimer.Enabled = true;
                }
                // Lower window by 5 pixels
                _startPosY += 5;
            });
        }

        private void ToastNotifier_Click(object sender, EventArgs e)
        {
            ClickAction?.Invoke(Updates);
            CloseToastMessage();
        }

        private void CloseToastMessage()
        {
            Dispatcher.InvokeAsync(() =>
            {
                // make sure all the timers are stopped
                _pauseTimer.Stop();
                _pauseTimer.Dispose();
                _goUpTimer.Stop();
                _goUpTimer.Dispose();
                _goUpTimer.Stop();
                _goUpTimer.Dispose();
                Close();
                if (!_hasInitiatedShutdown)
                {
                    _hasInitiatedShutdown = true;
                    Dispatcher.InvokeShutdown();
                }
            });
        }

        /// <summary>
        /// Show the toast
        /// </summary>
        /// <param name="message">Main message of the toast</param>
        /// <param name="callToAction">Text of the hyperlink</param>
        /// <param name="seconds">How long to show before it goes back down</param>
        public void Show(string message, string callToAction, int seconds)
        {
            NotificationTitle.Text = message;
            NotificationLink.Inlines.Clear();
            NotificationLink.Inlines.Add(callToAction);
            _pauseTimer.Interval = 1000 * seconds;
            Show();
        }

        private void NotificationLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            ClickAction?.Invoke(Updates);
            CloseToastMessage();
        }
    }
}
