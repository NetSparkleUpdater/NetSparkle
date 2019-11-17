using NetSparkle.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NetSparkle
{
    /// <summary>
    /// Like a notification ballon, but more reliable "toast" because it slowly goes up, then down.
    /// Subscribe to the Click even to know if the user clicked on it.
    /// </summary>
    public partial class ToastNotifier : Form
    {
        private Timer _goUpTimer;
        private Timer _goDownTimer;
        private Timer _pauseTimer;
        private int startPosX;
        private int startPosY;

        /// <summary>
        /// constructor
        /// </summary>
        public ToastNotifier(Icon applicationIcon = null)
        {
            InitializeComponent();
            // We want our window to be the top most
            TopMost = true;
            // Pop doesn't need to be shown in task bar
            ShowInTaskbar = false;
            // Create and run timer for animation
            _goUpTimer = new Timer();
            _goUpTimer.Interval = 25;
            _goUpTimer.Tick += GoUpTimerTick;
            _goDownTimer = new Timer();
            _goDownTimer.Interval = 25;
            _goDownTimer.Tick += GoDownTimerTick;
            _pauseTimer = new Timer();
            _pauseTimer.Interval = 15000;
            _pauseTimer.Tick += PauseTimerTick;

            if (applicationIcon != null)
            {
                Icon = applicationIcon;
                this.Image.Image = new Icon(applicationIcon, new Size(48, 48)).ToBitmap();
            }
        }

        public Action<AppCastItem[]> ClickAction { get; set; }

        public AppCastItem[] Updates { get; set; }

        private void PauseTimerTick(object sender, EventArgs e)
        {
            _pauseTimer.Stop();
            _goDownTimer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            // Move window out of screen
            startPosX = Screen.PrimaryScreen.WorkingArea.Width - Width;
            startPosY = Screen.PrimaryScreen.WorkingArea.Height;
            SetDesktopLocation(startPosX, startPosY);
            base.OnLoad(e);
            // Begin animation
            _goUpTimer.Start();
        }

        void GoUpTimerTick(object sender, EventArgs e)
        {
            //Lift window by 5 pixels
            startPosY -= 5;
            //If window is fully visible stop the timer
            if (startPosY < Screen.PrimaryScreen.WorkingArea.Height - Height)
            {
                _goUpTimer.Stop();
                _pauseTimer.Start();
            }
            else
            {
                SetDesktopLocation(startPosX, startPosY);
            }
        }

        private void GoDownTimerTick(object sender, EventArgs e)
        {
            //Lower window by 5 pixels
            startPosY += 5;
            //If window is fully visible stop the timer
            if (startPosY > Screen.PrimaryScreen.WorkingArea.Height)
            {
                _goDownTimer.Stop();
                Close();
            }
            else
            {
                SetDesktopLocation(startPosX, startPosY);
            }
        }

        private void ToastNotifier_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            ClickAction?.Invoke(Updates);
            Close();
        }

        /// <summary>
        /// Show the toast
        /// </summary>
        /// <param name="message">Main message of the toast</param>
        /// <param name="callToAction">Text of the hyperlink</param>
        /// <param name="seconds">How long to show before it goes back down</param>
        public void Show(string message, string callToAction, int seconds)
        {
            _message.Text = message;
            _callToAction.Text = callToAction;
            _pauseTimer.Interval = 1000 * seconds;
            Show();
        }

        private void callToAction_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.ToastNotifier_Click(null, null);
        }
    }
}
