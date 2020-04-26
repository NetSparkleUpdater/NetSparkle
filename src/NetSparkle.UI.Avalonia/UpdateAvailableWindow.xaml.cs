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
    public class UpdateAvailableWindow : BaseWindow, IUpdateAvailable, IReleaseNotesUpdater, IUserRespondedToUpdateCheck
    {
        private UpdateAvailableWindowViewModel _dataContext;
        private RowDefinition _releaseNotesRow;

        private HtmlLabel _htmlLabel;

        public UpdateAvailableWindow() : base(true)
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

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
            _dataContext.ReleaseNotesUpdater = this;
            _dataContext.UserRespondedHandler = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            var grid = this.FindControl<Grid>("MainGrid");
            _releaseNotesRow = grid.RowDefinitions[2];
            _htmlLabel = this.FindControl<HtmlLabel>("ChangeNotesHTMLLabel");
            _htmlLabel.SetValue(HtmlLabel.AutoSizeHeightOnlyProperty, true);
        }

        UpdateAvailableResult IUpdateAvailable.Result => _dataContext?.UserResponse ?? UpdateAvailableResult.None;

        AppCastItem IUpdateAvailable.CurrentItem => CurrentItem;

        public AppCastItem CurrentItem
        {
            get { return _dataContext?.Updates?.FirstOrDefault(); }
        }

        public event UserRespondedToUpdate UserResponded;

        void IUpdateAvailable.BringToFront()
        {
            BringToFront();
        }

        void IUpdateAvailable.Close()
        {
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

        public void UserRespondedToUpdateCheck(UpdateAvailableResult response)
        {
            UserResponded?.Invoke(this, new UpdateResponseEventArgs(_dataContext?.UserResponse ?? UpdateAvailableResult.None, CurrentItem));
        }

        public void ShowReleaseNotes(string notes)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _htmlLabel.Text = notes;
            });
        }
    }
}
