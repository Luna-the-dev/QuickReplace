using System.ComponentModel;
using System.Diagnostics;
using TextReplace.Core;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    class ReplaceViewModel : ObservableObject
    {
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

        // commands
        public RelayCommand Replace => new RelayCommand(o => ReplaceCmd());

        private void ReplaceCmd()
        {
            ReplaceData replaceData = new ReplaceData(CaseSensitive);
            string suffix = "replacify"; // TODO let the user change this with GUI later

            // create a list of destination file names
            List<string> destFileNames = SourceFiles.GenerateDestFileNames(suffix);

            // perform the text replacements
            bool result = replaceData.PerformReplacements(SourceFiles.FileNames, destFileNames, WholeWord);

            if (result == false)
            {
                Debug.WriteLine("A replacement could not be made.");
            }
        }
    }
}
