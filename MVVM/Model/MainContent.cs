using TextReplace.MVVM.ViewModel;

namespace TextReplace.MVVM.Model
{
    class MainContent
    {
        private static object _activeView = new HomeViewModel();
        public static object ActiveView
        {
            get { return _activeView; }
            set
            {
                _activeView = value;
            }
        }
    }
}
