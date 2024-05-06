using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TextReplace.MVVM.ViewModel
{
    partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(HomeViewCommand))]
        [NotifyCanExecuteChangedFor(nameof(ReplaceViewCommand))]
        private object _currentView;

        [ObservableProperty]
        private object _topBarView;

        [ObservableProperty]
        private object _sideBarView;


        public HomeViewModel HomeVm = new HomeViewModel();
        public ReplaceViewModel ReplaceVm = new ReplaceViewModel();
        public TopBarViewModel TopBarVm = new TopBarViewModel();
        public SideBarViewModel SideBarVm = new SideBarViewModel();

        public MainViewModel()
        {
            // set the home view as default
            _currentView = HomeVm;

            _topBarView = TopBarVm;
            _sideBarView = SideBarVm;
        }

        [RelayCommand]
        private void HomeView()
        {
            CurrentView = HomeVm;
        }

        [RelayCommand]
        private void ReplaceView()
        {
            CurrentView = ReplaceVm;
        }
    }
}
