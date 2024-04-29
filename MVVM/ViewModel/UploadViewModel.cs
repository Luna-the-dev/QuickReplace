using System.Diagnostics;
using TextReplace.Core;

namespace TextReplace.MVVM.ViewModel
{
    class UploadViewModel : ObservableObject
    {
        private string _delimiter = "";
        public string Delimiter
        {
            get { return _delimiter; }
            set
            {
                if (IsDelimiterValid(value))
                {
                    _delimiter = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _hasHeader = false;
        public bool HasHeader
        {
            get { return _hasHeader; }
            set
            {
                _hasHeader = value;
                OnPropertyChanged();
            }
        }

        private const string INVALID_DELIMITER_CHARS = "\n";

        // Commands
        public RelayCommand ReplaceFile => new RelayCommand(o => ReplaceFileCmd());
        public RelayCommand SourceFiles => new RelayCommand(o => SourceFilesCmd());

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

        private bool IsDelimiterValid(string delimiter)
        {
            if (INVALID_DELIMITER_CHARS.Contains(delimiter))
            {
                return false;
            }
            return true;
        }
    }
}
