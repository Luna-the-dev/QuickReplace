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
                bool result = Model.ReplaceFile.SetNewReplaceFileFromUser();

                if (result == false)
                {
                    Debug.WriteLine("ReplaceFile could not be read.");
                }

                Debug.WriteLine(Model.ReplaceFile.FileName);
            });

            SourceFiles = new RelayCommand(o =>
            {
                // open a file dialogue for the user and update the replace file
                bool result = Model.SourceFiles.SetNewSourceFilesFromUser();

                if (result == false)
                {
                    Debug.WriteLine("SourceFile could not be read.");
                }

                Model.SourceFiles.FileNames.ForEach(i => Debug.WriteLine(i));
            });
        }

    }
}
