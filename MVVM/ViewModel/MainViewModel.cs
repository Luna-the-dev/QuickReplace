﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TextReplace.MVVM.ViewModel
{
    partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private object _currentView;

        [ObservableProperty]
        private object _topBarView;

        [ObservableProperty]
        private object _sideBarView;
        

        public RelayCommand HomeViewCommand => new RelayCommand(() => { CurrentView = HomeVm; });
        public RelayCommand ReplaceViewCommand => new RelayCommand(() => { CurrentView = ReplaceVm; });

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

        private void temp()
        {

        }
    }
}
