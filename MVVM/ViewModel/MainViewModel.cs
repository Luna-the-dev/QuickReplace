using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using TextReplace.Messages;

namespace TextReplace.MVVM.ViewModel
{
    partial class MainViewModel : ObservableRecipient, IRecipient<ActiveContentViewMsg>
    {
        [ObservableProperty]
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

            WeakReferenceMessenger.Default.Register(this);
        }

        public void Receive(ActiveContentViewMsg message)
        {
            CurrentView = message.Value;
        }
    }
}
