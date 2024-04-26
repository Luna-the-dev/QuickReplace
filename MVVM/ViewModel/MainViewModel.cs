using System;
using TextReplace.Core;

namespace TextReplace.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {

        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand UploadViewCommand { get; set; }
        public RelayCommand SingleReplaceViewCommand { get; set; }

        public HomeViewModel HomeVm { get; set; }
        public UploadViewModel UploadVm { get; set; }
        public SingleReplaceViewModel SingleReplaceVm { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
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
            SingleReplaceVm = new SingleReplaceViewModel();

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

            SingleReplaceViewCommand = new RelayCommand(o =>
            {
                CurrentView = SingleReplaceVm;
            });
        }
    }
}
