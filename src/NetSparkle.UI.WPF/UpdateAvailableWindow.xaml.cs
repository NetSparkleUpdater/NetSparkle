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
    /// Interaction logic for UpdateAvailableWindow.xaml.
    /// 
    /// Window that shows the list of available updates to the user
    /// </summary>
    public partial class UpdateAvailableWindow : BaseWindow, IUpdateAvailable, IReleaseNotesUpdater, IUserRespondedToUpdateCheck
    {
        private UpdateAvailableWindowViewModel _dataContext;
        private bool _hasFinishedNavigatingToAboutBlank = false;
        private string _notes = "";
        private bool _wasResponseSent = false;

        /// <summary>
        /// Initialize the available update window with no initial date context
        /// (and thus no initial information on downloadable releases to show
        /// to the user)
        /// </summary>
        public UpdateAvailableWindow() : base(true)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize the available update window with the given view model,
        /// which contains the information on the updates that are available to the
        /// end user
        /// </summary>
        /// <param name="viewModel">View model with info on the updates that are available
        /// to the user</param>
        public UpdateAvailableWindow(UpdateAvailableWindowViewModel viewModel) : base(true)
        {
            InitializeComponent();
            DataContext = _dataContext = viewModel;
            _dataContext.ReleaseNotesUpdater = this;
            _dataContext.UserRespondedHandler = this;
            ReleaseNotesBrowser.Navigated += ReleaseNotesBrowser_Navigated;
            Closing += UpdateAvailableWindow_Closing;
        }

        private void UpdateAvailableWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserRespondedToUpdateCheck(UpdateAvailableResult.None); // just in case response not sent
            ReleaseNotesBrowser.Navigated -= ReleaseNotesBrowser_Navigated;
            Closing -= UpdateAvailableWindow_Closing;
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
            UserRespondedToUpdateCheck(UpdateAvailableResult.None);
            ReleaseNotesBrowser.Navigated -= ReleaseNotesBrowser_Navigated;
            Closing -= UpdateAvailableWindow_Closing;
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
        /// Show the release notes to the end user. Release notes should be in HTML.
        /// 
        /// There is some bizarre thing where the WPF browser doesn't navigate to the release notes unless you successfully navigate to
        /// about:blank first. I don't know why. I feel like this is a Terrible Bad Fix, but...it works for now...
        /// </summary>
        /// <param name="notes">The HTML notes to show to the end user</param>
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
