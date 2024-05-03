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
                ReplaceData.HasHeader = (value == Visibility.Visible) ? true : false;
                Debug.WriteLine(ReplaceData.HasHeader);
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
                ReplaceData.Delimiter = value;
                Debug.WriteLine(ReplaceData.Delimiter);
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
            bool result = Model.ReplaceData.SetNewReplaceFileFromUser();

            if (result == false)
            {
                Debug.WriteLine("ReplaceFile either could not be read or parsed.");
            }

            Debug.Write("Replace file name: ");
            Debug.WriteLine(Model.ReplaceData.FileName);

            /*foreach (var kvp in Model.ReplaceData.ReplacePhrases)
            {
                Debug.WriteLine($"key: {kvp.Key}\tvalue: {kvp.Value}");
            }*/
        }

        private void SourceFilesCmd()
        {
            // open a file dialogue for the user and update the source files
            bool result = Model.SourceFiles.SetNewSourceFilesFromUser();

            if (result == false)
            {
                Debug.WriteLine("SourceFile could not be read.");
            }

            Debug.Write("Source file name(s): ");
            Model.SourceFiles.FileNames.ForEach(i => Debug.WriteLine(i));
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
