﻿using TextReplace.Core;

namespace TextReplace.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        private object _topBarView;
        public object TopBarView
        {
            get { return _topBarView; }
            set
            {
                _topBarView = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand HomeViewCommand => new RelayCommand(o => { CurrentView = HomeVm; });
        public RelayCommand UploadViewCommand => new RelayCommand(o => { CurrentView = UploadVm; });
        public RelayCommand ReplaceViewCommand => new RelayCommand(o => { CurrentView = ReplaceVm; });

        public HomeViewModel HomeVm = new HomeViewModel();
        public UploadViewModel UploadVm = new UploadViewModel();
        public ReplaceViewModel ReplaceVm = new ReplaceViewModel();
        public TopBarViewModel TopBarVm = new TopBarViewModel();
        
        public MainViewModel()
        {
            // set the home view as default
            _currentView = HomeVm;

            _topBarView = TopBarVm;
        }
    }
}
