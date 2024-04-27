using System;
using TextReplace.Core;

namespace TextReplace.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {

        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand UploadViewCommand { get; set; }
        public RelayCommand ReplaceViewCommand { get; set; }

        public HomeViewModel HomeVm { get; set; }
        public UploadViewModel UploadVm { get; set; }
        public ReplaceViewModel ReplaceVm { get; set; }

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


        public MainViewModel()
        {
            // declare view models
            HomeVm = new HomeViewModel();
            UploadVm = new UploadViewModel();
            ReplaceVm = new ReplaceViewModel();

            // set the home view as default
            CurrentView = HomeVm;


            // change the current view when buttons are clicked
            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVm;
            });

            UploadViewCommand = new RelayCommand(o =>
            {
                CurrentView = UploadVm;
            });

            ReplaceViewCommand = new RelayCommand(o =>
            {
                CurrentView = ReplaceVm;
            });
        }
    }
}
