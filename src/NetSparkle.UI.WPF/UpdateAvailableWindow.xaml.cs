using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.UI.WPF.Controls;
using NetSparkleUpdater.UI.WPF.Interfaces;
using NetSparkleUpdater.UI.WPF.ViewModels;
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

namespace NetSparkleUpdater.UI.WPF
{
    /// <summary>
    /// Interaction logic for UpdateAvailableWindow.xaml
    /// </summary>
    public partial class UpdateAvailableWindow : BaseWindow, IUpdateAvailable, IReleaseNotesUpdater, IUserRespondedToUpdateCheck
    {
        private UpdateAvailableWindowViewModel _dataContext;
        private bool _hasFinishedNavigatingToAboutBlank = false;
        private string _notes = "";

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
            ReleaseNotesBrowser.Navigated += ReleaseNotesBrowser_Navigated;
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
            ReleaseNotesBrowser.Navigated -= ReleaseNotesBrowser_Navigated;
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
            UserResponded?.Invoke(this, new UpdateResponseEventArgs(_dataContext?.UserResponse ?? UpdateAvailableResult.None, CurrentItem));
        }

        // there is some bizarre thing where the WPF browser doesn't navigate to the release notes unless you successfully navigate to
        // about:blank first. I don't know why. I feel like this is a Terrible Bad Fix, but...it works for now...

        public void ShowReleaseNotes(string notes)
        {
            _notes = notes;
            ReleaseNotesBrowser.Dispatcher.Invoke(() =>
            {

                if (ReleaseNotesBrowser.IsLoaded)
                {
                    if (_hasFinishedNavigatingToAboutBlank)
                    {
                        ReleaseNotesBrowser.NavigateToString(_notes);
                    }
                    // else will catch up when navigating to about:blank is done
                }
                else
                {
                    // don't do anything until the web browser is loaded
                    ReleaseNotesBrowser.Loaded += ReleaseNotesBrowser_Loaded;
                }
            });
        }

        private void ReleaseNotesBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            // see https://stackoverflow.com/a/15209861/3938401
            ReleaseNotesBrowser.Loaded -= ReleaseNotesBrowser_Loaded;
            ReleaseNotesBrowser.Dispatcher.Invoke(() =>
            {
                ReleaseNotesBrowser.NavigateToString("about:blank");
            });
        }

        private void ReleaseNotesBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            if (!_hasFinishedNavigatingToAboutBlank)
            {
                ReleaseNotesBrowser.NavigateToString(_notes);
                _hasFinishedNavigatingToAboutBlank = true;
            }
        }
    }
}
