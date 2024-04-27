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
        public RelayCommand Replace { get; set; }
        public RelayCommand ToggleCaseSensitive { get; set; }
        public RelayCommand ToggleWholeWord { get; set; }

        public ReplaceViewModel()
        {
            Replace = new RelayCommand(o =>
            {
                if (DoReplace() == false)
                {
                    Debug.WriteLine("Something went wrong in the replace command.");
                }
            });

            ToggleCaseSensitive = new RelayCommand(o =>
            {
                Debug.WriteLine($"case sensitive = {o}");
                CaseSensitive = Convert.ToBoolean(o);
            });

            ToggleWholeWord = new RelayCommand(o =>
            {
                Debug.WriteLine($"whole word = {o}");
                WholeWord = Convert.ToBoolean(o);
            });
        }

        /// <summary>
        /// Utility function used by the Replace command. This performs the actual replace function.
        /// </summary>
        /// <returns>Returns false if something went wrong.</returns>
        private bool DoReplace()
        {
            ReplaceData replaceData = new ReplaceData(CaseSensitive);
            string suffix = "replacify"; // TODO let the user change this with GUI later

            // parse the replace phrases and save them in the object
            bool result = replaceData.ParseReplacePhrases();

            if (result == false)
            {
                Debug.WriteLine("Replace phrases could not be parsed.");
                return false;
            }

            // create a list of destination file names
            List<string> destFileNames = SourceFiles.GenerateDestFileNames(suffix);

            // perform the text replacements
            result = replaceData.PerformReplacements(SourceFiles.FileNames, destFileNames, WholeWord);

            if (result == false)
            {
                Debug.WriteLine("A replacement could not be made.");
                return false;
            }

            return true;
        }
    }
}
