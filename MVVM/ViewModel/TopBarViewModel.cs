using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TextReplace.Core;
using TextReplace.MVVM.Model;
using TextReplace.MVVM.View;

namespace TextReplace.MVVM.ViewModel
{
    class TopBarViewModel : ObservableObject
    {
		private Visibility _hasHeader = Visibility.Hidden;
		public Visibility HasHeader
		{
			get { return _hasHeader; }
			set
			{
				_hasHeader = value;
                ReplaceFileData.HasHeader = (value == Visibility.Visible) ? true : false;
                Debug.WriteLine(ReplaceFileData.HasHeader);
                OnPropertyChanged();
			}
		}
        private string _delimiter = string.Empty;
        public string Delimiter
        {
            get { return _delimiter; }
            set
            {
                _delimiter = value;
                ReplaceFileData.Delimiter = value;
                Debug.WriteLine(ReplaceFileData.Delimiter);
                OnPropertyChanged();
            }
        }

        private Visibility _replaceFileReadSuccess = Visibility.Hidden;
        public Visibility ReplaceFileReadSuccess
        {
            get { return _replaceFileReadSuccess; }
            set
            {
                _replaceFileReadSuccess = value;
                OnPropertyChanged();
            }
        }
        private Visibility _replaceFileReadFail = Visibility.Hidden;
        public Visibility ReplaceFileReadFail
        {
            get { return _replaceFileReadFail; }
            set
            {
                _replaceFileReadFail = value;
                OnPropertyChanged();
            }
        }
        private Visibility _sourceFileReadSuccess = Visibility.Hidden;
        public Visibility SourceFileReadSuccess
        {
            get { return _sourceFileReadSuccess; }
            set
            {
                _sourceFileReadSuccess = value;
                OnPropertyChanged();
            }
        }
        private Visibility _sourceFileReadFail = Visibility.Hidden;
        public Visibility SourceFileReadFail
        {
            get { return _sourceFileReadFail; }
            set 
            {
                _sourceFileReadFail = value;
                OnPropertyChanged();
            }
        }

        private const string INVALID_DELIMITER_CHARS = "\n";

        // commands
        public RelayCommand ReplaceFile => new RelayCommand(o => ReplaceFileCmd());
        public RelayCommand SourceFiles => new RelayCommand(o => SourceFilesCmd());
        public RelayCommand ToggleHasHeader => new RelayCommand(o => HasHeaderCmd());

        private void ReplaceFileCmd()
        {
            // open a file dialogue for the user and update the replace file
            bool? result = ReplaceFileData.SetNewReplaceFileFromUser();

            if (result == true)
            {
                Debug.Write("Replace file name:\t");
                Debug.WriteLine(ReplaceFileData.FileName);
                ReplaceFileReadSuccess = Visibility.Visible;
                ReplaceFileReadFail = Visibility.Hidden;
                Debug.WriteLine(ReplaceFileReadSuccess);
            }
            else if (result == false) 
            {
                Debug.WriteLine("ReplaceFile either could not be read or parsed.");
                ReplaceFileReadSuccess = Visibility.Hidden;
                ReplaceFileReadFail = Visibility.Visible;
            }
        }

        private void SourceFilesCmd()
        {
            // open a file dialogue for the user and update the source files
            bool? result = Model.SourceFilesData.SetNewSourceFilesFromUser();

            if (result == true)
            {
                Debug.Write("Source file name(s):");
                Model.SourceFilesData.FileNames.ForEach(i => Debug.WriteLine($"\t{i}"));
                SourceFileReadSuccess = Visibility.Visible;
                SourceFileReadFail = Visibility.Hidden;
            }
            else if (result == false)
            {
                Debug.WriteLine("SourceFile could not be read.");
                SourceFileReadSuccess = Visibility.Hidden;
                SourceFileReadFail = Visibility.Visible;
            }
        }

        public void HasHeaderCmd()
		{
			HasHeader = (HasHeader == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
		}

        /// <summary>
        /// Checks to see if the delimiter is valid, and then sets the delimiter
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public bool SetDelimiter(string delimiter)
        {
            if (IsDelimiterValid(delimiter))
            {
                Delimiter = delimiter;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsDelimiterValid(string delimiter)
        {
            if (delimiter == string.Empty)
            {
                return true;
            }
            return INVALID_DELIMITER_CHARS.Contains(delimiter) ? false : true;
        }
    }
}
