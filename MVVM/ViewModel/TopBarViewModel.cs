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
        private bool _caseSensitive = false;
        public bool CaseSensitive
        {
            get { return _caseSensitive; }
            set
            {
                _caseSensitive = value;
                OnPropertyChanged();
            }
        }
        private bool _wholeWord = false;
        public bool WholeWord
        {
            get { return _wholeWord; }
            set
            {
                _wholeWord = value;
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
        private Visibility _replaceisClickable = Visibility.Hidden;
        public Visibility ReplaceisClickable
        {
            get { return _replaceisClickable; }
            set
            {
                _replaceisClickable = value;
                OnPropertyChanged();
            }
        }
        private Visibility _replaceisUnclickable = Visibility.Visible;
        public Visibility ReplaceisUnclickable
        {
            get { return _replaceisUnclickable; }
            set
            {
                _replaceisUnclickable = value;
                OnPropertyChanged();
            }
        }

        private const string INVALID_DELIMITER_CHARS = "\n";

        // commands
        public RelayCommand ReplaceFile => new RelayCommand(o => ReplaceFileCmd());
        public RelayCommand SourceFiles => new RelayCommand(o => SourceFilesCmd());
        public RelayCommand ToggleHasHeader => new RelayCommand(o => HasHeaderCmd());
        public RelayCommand Replace => new RelayCommand(o => ReplaceCmd());

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
            }
            else if (result == false) 
            {
                Debug.WriteLine("ReplaceFile either could not be read or parsed.");
                ReplaceFileReadSuccess = Visibility.Hidden;
                ReplaceFileReadFail = Visibility.Visible;
            }

            SetReplaceButtonClickability();
        }

        private void SourceFilesCmd()
        {
            // open a file dialogue for the user and update the source files
            bool? result = SourceFilesData.SetNewSourceFilesFromUser();

            if (result == true)
            {
                Debug.Write("Source file name(s):");
                SourceFilesData.FileNames.ForEach(i => Debug.WriteLine($"\t{i}"));
                SourceFileReadSuccess = Visibility.Visible;
                SourceFileReadFail = Visibility.Hidden;
            }
            else if (result == false)
            {
                Debug.WriteLine("SourceFile could not be read.");
                SourceFileReadSuccess = Visibility.Hidden;
                SourceFileReadFail = Visibility.Visible;
            }

            SetReplaceButtonClickability();
        }

        public void HasHeaderCmd()
		{
			HasHeader = (HasHeader == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
		}

        private void ReplaceCmd()
        {
            if (ReplaceFileData.FileName == string.Empty || SourceFilesData.FileNames.Count == 0)
            {
                Debug.WriteLine("Replace file or source files were empty. This should never be reached...");
                return;
            }

            ReplaceFileData replaceData = new ReplaceFileData(CaseSensitive);
            string suffix = "replacify"; // TODO let the user change this with GUI later

            // create a list of destination file names
            List<string> destFileNames = SourceFilesData.GenerateDestFileNames(suffix);

            // perform the text replacements
            bool result = replaceData.PerformReplacements(SourceFilesData.FileNames, destFileNames, WholeWord);
            SourceFilesData.FileNames.ForEach(o => Debug.WriteLine(o));
            destFileNames.ForEach(o => Debug.WriteLine(o));

            if (result == false)
            {
                Debug.WriteLine("A replacement could not be made.");
            }
            else
            {
                Debug.WriteLine("Youre the greatest programmer to ever live");
            }
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

        /// <summary>
        /// Checks if the delimiter string contains any invalid characters.
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns>True if the string is empty or does not contain any invalid characters.</returns>
        private bool IsDelimiterValid(string delimiter)
        {
            if (delimiter == string.Empty)
            {
                return true;
            }
            return INVALID_DELIMITER_CHARS.Contains(delimiter) ? false : true;
        }

        /// <summary>
        /// Sets the replace button visibility based on whether replace/source files were successfully uploaded.
        /// </summary>
        private void SetReplaceButtonClickability()
        {
            if (ReplaceFileReadSuccess == Visibility.Visible &&
                SourceFileReadSuccess == Visibility.Visible)
            {
                ReplaceisClickable = Visibility.Visible;
                ReplaceisUnclickable = Visibility.Hidden;
            }
            else
            {
                ReplaceisClickable = Visibility.Hidden;
                ReplaceisUnclickable = Visibility.Visible;
            }
        }
    }
}
