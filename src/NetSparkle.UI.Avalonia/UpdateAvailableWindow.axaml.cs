using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.UI.Avalonia.Controls;
using NetSparkleUpdater.UI.Avalonia.Interfaces;
using NetSparkleUpdater.UI.Avalonia.ViewModels;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace NetSparkleUpdater.UI.Avalonia
{
    /// <summary>
    /// Interaction logic for UpdateAvailableWindow.xaml.
    /// 
    /// Window that shows the list of available updates to the user
    /// </summary>
    public partial class UpdateAvailableWindow : BaseWindow, IUpdateAvailable, IReleaseNotesDisplayer, IUserRespondedToUpdateCheck
    {
        private UpdateAvailableWindowViewModel? _dataContext;
        private RowDefinition? _releaseNotesRow;
        private ScrollViewer? _htmlLabelContainer;
        private HtmlLabel? _htmlLabel;
        private bool _wasResponseSent = false;

        /// <summary>
        /// Initialize the available update window with no initial date context
        /// (and thus no initial information on downloadable releases to show
        /// to the user)
        /// </summary>
        public UpdateAvailableWindow() : base(true)
        {
            this.InitializeComponent();
            InitViews();
        }

        /// <summary>
        /// Initialize the available update window with the given view model,
        /// which contains the information on the updates that are available to the
        /// end user
        /// </summary>
        /// <param name="viewModel">View model with info on the updates that are available
        /// to the user</param>
        /// <param name="iconBitmap">Bitmap to use for the app's logo/graphic</param>
        public UpdateAvailableWindow(UpdateAvailableWindowViewModel viewModel, Bitmap? iconBitmap) : base(true)
        {
            this.InitializeComponent();
            InitViews();
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

        private void InitViews()
        {
            var grid = this.FindControl<Grid>("MainGrid");
            _releaseNotesRow = grid?.RowDefinitions[2];
            _htmlLabel = this.FindControl<HtmlLabel>("ChangeNotesHTMLLabel");
            _htmlLabelContainer = this.FindControl<ScrollViewer>("ChangeNotesScrollViewer");
            //_htmlLabel.SetValue(HtmlLabel.AutoSizeHeightOnlyProperty, true); // throws on 0.10.0 for some reason?
        }

        /// <summary>
        /// Change the main grid's background color. Use new SolidColorBrush(Colors.Transparent) or null to clear.
        /// </summary>
        /// <param name="solidColorBrush"></param>
        public void ChangeMainGridBackgroundColor(IBrush solidColorBrush)
        {
            var grid = this.FindControl<Grid>("MainGrid");
            if (grid != null)
            {
                grid.Background = solidColorBrush;
            }
        }

        private void UpdateAvailableWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            UserRespondedToUpdateCheck(UpdateAvailableResult.None); // just in case
            Closing -= UpdateAvailableWindow_Closing;
        }

        UpdateAvailableResult IUpdateAvailable.Result => _dataContext?.UserResponse ?? UpdateAvailableResult.None;

        AppCastItem IUpdateAvailable.CurrentItem => CurrentItem;

        /// <summary>
        /// The item that the user is being asked about updating to
        /// </summary>
        public AppCastItem CurrentItem
        {
            // I don't really like the creating of a new app cast item, but we'll revisit
            // some of this when we refactor the UI stuff, so it's OK for now. Not really used
            // anywhere in our lib, anyway.
            get { return _dataContext?.Updates?.FirstOrDefault() ?? new AppCastItem(); }
        }

        /// <summary>
        /// An event that informs its listeners how the user responded to the
        /// software update request
        /// </summary>
        public event UserRespondedToUpdate? UserResponded;

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
            if (_releaseNotesRow != null)
            {
                _releaseNotesRow.Height = new GridLength(0);
            }
            if (_htmlLabelContainer != null)
            {
                _htmlLabelContainer.IsVisible = false;
            }
            Height = 225;
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

        void IUpdateAvailable.Show()
        {
            ShowWindow();
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
        /// <param name="htmlNotes">The HTML notes to show to the end user</param>
        public void ShowReleaseNotes(string htmlNotes)
        {
            if (_htmlLabel != null)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _htmlLabel.Text = htmlNotes;
                });
            }
        }
    }
}
