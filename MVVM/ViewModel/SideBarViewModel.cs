using TextReplace.Core;
using TextReplace.MVVM.Model;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace TextReplace.MVVM.ViewModel
{
    class SideBarViewModel : ObservableObject
    {
        private object _selectedView;
        public object SelectedView
        {
            get { return _selectedView; }
            set
            {
                _selectedView = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand HomeViewCommand => new RelayCommand(o => { MainContent.ActiveView = HomeVm; });
        public RelayCommand ReplaceViewCommand => new RelayCommand(o => { MainContent.ActiveView = ReplaceVm; });

        public HomeViewModel HomeVm = new HomeViewModel();
        public ReplaceViewModel ReplaceVm = new ReplaceViewModel();

        public SideBarViewModel()
        {
            _selectedView = HomeVm;
        }




    }
}
