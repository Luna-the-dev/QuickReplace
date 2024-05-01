using System.Diagnostics;
using System.Windows;
using TextReplace.Core;

namespace TextReplace.MVVM.ViewModel
{
    class TopBarViewModel : ObservableObject
    {
		private Visibility _wholeWord = Visibility.Hidden;
		public Visibility WholeWord
		{
			get { return _wholeWord; }
			set
			{
				_wholeWord = value;
				OnPropertyChanged();
				Debug.WriteLine(value);
			}
		}

		public RelayCommand ToggleWholeWord => new RelayCommand(o => TglWholeWord());

		public void TglWholeWord()
		{
			if (WholeWord == Visibility.Hidden)
			{
				WholeWord = Visibility.Visible;
			}
			else
			{
				WholeWord = Visibility.Hidden;
			}
		}
	}
}
