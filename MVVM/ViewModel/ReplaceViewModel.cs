using System.Diagnostics;
using TextReplace.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TextReplace.MVVM.ViewModel
{
    partial class ReplaceViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _caseSensitive = false;

        [ObservableProperty]
        private bool _wholeWord = false;
        

        // commands
        public RelayCommand Replace => new RelayCommand(() => ReplaceCmd());

        private void ReplaceCmd()
        {
            ReplaceData replaceData = new ReplaceData(CaseSensitive);

            // create a list of destination file names
            List<string> destFileNames = SourceFilesData.GenerateDestFileNames();

            // perform the text replacements
            bool result = replaceData.PerformReplacements(SourceFilesData.FileNames, destFileNames, WholeWord);

            if (result == false)
            {
                Debug.WriteLine("A replacement could not be made.");
            }
        }
    }
}
