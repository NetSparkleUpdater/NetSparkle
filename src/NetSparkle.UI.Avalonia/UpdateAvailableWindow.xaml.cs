using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Html;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.UI.Avalonia.Controls;
using NetSparkleUpdater.UI.Avalonia.Interfaces;
using NetSparkleUpdater.UI.Avalonia.ViewModels;
using System.Linq;

namespace NetSparkleUpdater.UI.Avalonia
{
    /// <summary>
    /// Interaction logic for UpdateAvailableWindow.xaml.
    /// 
    /// Window that shows the list of available updates to the user
    /// </summary>
    public class UpdateAvailableWindow : BaseWindow, IUpdateAvailable, IReleaseNotesDisplayer, IUserRespondedToUpdateCheck
    {
        private UpdateAvailableWindowViewModel _dataContext;
        private RowDefinition _releaseNotesRow;

        private HtmlLabel _htmlLabel;
        private bool _wasResponseSent = false;

        /// <summary>
        /// Initialize the available update window with no initial date context
        /// (and thus no initial information on downloadable releases to show
        /// to the user)
        /// </summary>
        public UpdateAvailableWindow() : base(true)
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        /// <summary>
        /// Initialize the available update window with the given view model,
        /// which contains the information on the updates that are available to the
        /// end user
        /// </summary>
        /// <param name="viewModel">View model with info on the updates that are available
        /// to the user</param>
        /// <param name="iconBitmap">Bitmap to use for the app's logo/graphic</param>
        public UpdateAvailableWindow(UpdateAvailableWindowViewModel viewModel, IBitmap iconBitmap) : base(true)
        {
            this.InitializeComponent();
            var imageControl = this.FindControl<Image>("AppIcon");
            if (imageControl != null)
            {
                imageControl.Source = iconBitmap;
            }
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = _dataContext = viewModel;
            _dataContext.ReleaseNotesDisplayer = this;
            _dataContext.UserRespondedHandler = this;
            Closing += UpdateAvailableWindow_Closing;
        }

        private void UpdateAvailableWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserRespondedToUpdateCheck(UpdateAvailableResult.None); // just in case
            Closing -= UpdateAvailableWindow_Closing;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            var grid = this.FindControl<Grid>("MainGrid");
            _releaseNotesRow = grid.RowDefinitions[2];
            _htmlLabel = this.FindControl<HtmlLabel>("ChangeNotesHTMLLabel");
            //_htmlLabel.SetValue(HtmlLabel.AutoSizeHeightOnlyProperty, true); // throws on 0.10.0 for some reason?
        }

        UpdateAvailableResult IUpdateAvailable.Result => _dataContext?.UserResponse ?? UpdateAvailableResult.None;

        AppCastItem IUpdateAvailable.CurrentItem => CurrentItem;

        /// <summary>
        /// The item that the user is being asked about updating to
        /// </summary>
        public AppCastItem CurrentItem
        {
            get { return _dataContext?.Updates?.FirstOrDefault(); }
        }

        /// <summary>
        /// An event that informs its listeners how the user responded to the
        /// software update request
        /// </summary>
        public event UserRespondedToUpdate UserResponded;

        void IUpdateAvailable.BringToFront()
        {
            BringToFront();
        }

        void IUpdateAvailable.Close()
        {
            UserRespondedToUpdateCheck(UpdateAvailableResult.None); // just in case
            Closing -= UpdateAvailableWindow_Closing;
            CloseWindow();
        }

        void IUpdateAvailable.HideReleaseNotes()
        {
            if (_dataContext != null)
            {
                _dataContext.AreReleaseNotesVisible = false;
            }
            _releaseNotesRow.Height = new GridLength(10);
        }

        void IUpdateAvailable.HideRemindMeLaterButton()
        {
            if (_dataContext != null)
            {
                _dataContext.IsRemindMeLaterVisible = false;
            }
        }

        void IUpdateAvailable.HideSkipButton()
        {
            if (_dataContext != null)
            {
                _dataContext.IsSkipVisible = false;
            }
        }

        void IUpdateAvailable.Show(bool isOnMainThread)
        {
            ShowWindow(isOnMainThread);
        }

        /// <summary>
        /// The user responded to the update check with a given response
        /// </summary>
        /// <param name="response">How the user responded to the update (e.g. install the update, remind me later)</param>
        public void UserRespondedToUpdateCheck(UpdateAvailableResult response)
        {
            if (!_wasResponseSent)
            {
                _wasResponseSent = true;
                UserResponded?.Invoke(this, new UpdateResponseEventArgs(_dataContext?.UserResponse ?? UpdateAvailableResult.None, CurrentItem));
            }
        }

        /// <summary>
        /// Show the HTML release notes to the user via the UI dispatcher
        /// </summary>
        /// <param name="notes">The HTML notes to show to the end user</param>
        public void ShowReleaseNotes(string notes)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _htmlLabel.Text = notes;
            });
        }
    }
}
