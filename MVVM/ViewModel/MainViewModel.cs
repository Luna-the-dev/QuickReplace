using System;
using TextReplace.Core;

namespace TextReplace.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {

        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand SingleReplaceViewCommand { get; set; }

        public HomeViewModel HomeVm  { get; set; }

        public SingleReplaceViewModel SingleReplaceVm { get; set; }

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
            SingleReplaceVm = new SingleReplaceViewModel();

            // set the home view as default
            CurrentView = HomeVm;


            // change the current view when buttons are clicked
            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVm;
            });

            SingleReplaceViewCommand = new RelayCommand(o =>
            {
                CurrentView = SingleReplaceVm;
            });
        }
    }
}
