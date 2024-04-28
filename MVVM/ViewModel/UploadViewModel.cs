using System.CodeDom.Compiler;
using System.Diagnostics;
using TextReplace.Core;

namespace TextReplace.MVVM.ViewModel
{
    class UploadViewModel
    {
        private static string _delimiter = "hi";
        public static string Delimiter
        {
            get { return _delimiter; }
            private set { _delimiter = value; }
        }

        // Commands
        public RelayCommand ReplaceFile => new RelayCommand(o => ReplaceFileCmd());
        public RelayCommand SourceFiles => new RelayCommand(o => SourceFilesCmd());
        public RelayCommand SetDelimiter => new RelayCommand(o => SetDelimiterCmd());

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

        private void SetDelimiterCmd()
        {
            Debug.WriteLine("hey!");
            //Delimiter = o.ToString() ?? "";
            //Debug.WriteLine(Delimiter);
        }

    }
}
