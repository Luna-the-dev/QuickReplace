using System.Diagnostics;
using TextReplace.Core;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    class ReplaceViewModel
    {
        private bool _caseSensitive = false;
        public bool CaseSensitive
        {
            get { return _caseSensitive; }
            private set { _caseSensitive = value; }
        }

        private bool _wholeWord;
        public bool WholeWord
        {
            get { return _wholeWord; }
            private set { _wholeWord = value; }
        }

        // commands
        public RelayCommand Replace => new RelayCommand(o => ReplaceCmd());
        public RelayCommand ToggleCaseSensitive => new RelayCommand(o => ToggleCaseSensitiveCmd(o));
        public RelayCommand ToggleWholeWord => new RelayCommand(o => ToggleWholeWordCmd(o));

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

        private void ToggleCaseSensitiveCmd(object o)
        {
            Debug.WriteLine($"case sensitive = {o}");
            CaseSensitive = Convert.ToBoolean(o);
        }

        private void ToggleWholeWordCmd(object o)
        {
            Debug.WriteLine($"whole word = {o}");
            WholeWord = Convert.ToBoolean(o);
        }
    }
}
