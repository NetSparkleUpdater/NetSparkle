using NetSparkle.Enums;
using NetSparkle.Events;
using NetSparkle.Interfaces;
using NetSparkle.UI.WPF.Controls;
using NetSparkle.UI.WPF.Interfaces;
using NetSparkle.UI.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetSparkle.UI.WPF
{
    /// <summary>
    /// Interaction logic for UpdateAvailableWindow.xaml
    /// </summary>
    public partial class UpdateAvailableWindow : BaseWindow, IUpdateAvailable, IReleaseNotesUpdater, IUserRespondedToUpdateCheck
    {

        private UpdateAvailableWindowViewModel _dataContext;

        public UpdateAvailableWindow() : base(true)
        {
            InitializeComponent();
        }

        public UpdateAvailableWindow(UpdateAvailableWindowViewModel viewModel) : base(true)
        {
            InitializeComponent();
            DataContext = _dataContext = viewModel;
            _dataContext.ReleaseNotesUpdater = this;
            _dataContext.UserRespondedHandler = this;
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
            ReleaseNotesRow.Height = new GridLength(10);
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
            UserResponded?.Invoke(this, new UpdateResponseArgs(_dataContext?.UserResponse ?? UpdateAvailableResult.None, CurrentItem));
        }

        public void ShowReleaseNotes(string notes)
        {
            ReleaseNotesBrowser.Dispatcher.Invoke(() =>
            {
                // see https://stackoverflow.com/a/15209861/3938401
                ReleaseNotesBrowser.NavigateToString(notes);
            });
        }
    }
}
