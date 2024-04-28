using System.Diagnostics;
using TextReplace.Core;

namespace TextReplace.MVVM.ViewModel
{
    class UploadViewModel
    {
        // Commands
        public RelayCommand ReplaceFile { get; set; }
        public RelayCommand SourceFiles { get; set; }

        public UploadViewModel()
        {
            ReplaceFile = new RelayCommand(o =>
            {
                // open a file dialogue for the user and update the replace file
                bool result = Model.ReplaceData.SetNewReplaceFileFromUser();

                if (result == false)
                {
                    Debug.WriteLine("ReplaceFile either could not be read or parsed.");
                }

                /*Debug.WriteLine(Model.ReplaceData.FileName);

                foreach (var kvp in Model.ReplaceData.ReplacePhrases)
                {
                    Debug.WriteLine($"key: {kvp.Key}\tvalue: {kvp.Value}");
                }*/
            });

            SourceFiles = new RelayCommand(o =>
            {
                // open a file dialogue for the user and update the source files
                bool result = Model.SourceFiles.SetNewSourceFilesFromUser();

                if (result == false)
                {
                    Debug.WriteLine("SourceFile could not be read.");
                }

                /*Model.SourceFiles.FileNames.ForEach(i => Debug.WriteLine(i));*/
            });
        }

    }
}
