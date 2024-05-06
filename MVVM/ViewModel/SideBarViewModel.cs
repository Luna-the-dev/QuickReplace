using TextReplace.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace TextReplace.MVVM.ViewModel
{
    partial class SideBarViewModel : ObservableObject
    {
        [ObservableProperty]
        private object _selectedView;

        public RelayCommand HomeViewCommand => new RelayCommand(() => { MainContent.ActiveView = HomeVm; });
        public RelayCommand ReplaceViewCommand => new RelayCommand(() => { MainContent.ActiveView = ReplaceVm; });

        public HomeViewModel HomeVm = new HomeViewModel();
        public ReplaceViewModel ReplaceVm = new ReplaceViewModel();

        public SideBarViewModel()
        {
            _selectedView = HomeVm;
        }




    }
}
