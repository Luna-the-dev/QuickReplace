using System.Diagnostics;
using TextReplace.Core;

namespace TextReplace.MVVM.Model
{
    class ReplaceFile
    {
        private static string _fileName = string.Empty;

        public static string FileName
        {
            get => _fileName;
            set
            {
                if (FileValidation.IsInputFileReadable(value))
                {
                    _fileName = value;
                }
                else
                {
                    throw new Exception("Input file is not readable. ReplaceFile was not updated.");
                }
            }
        }

        /// <summary>
        /// Opens a file dialogue and replaces the ReplaceFile with whatever the user selects (if valid)
        /// </summary>
        /// <returns>False if the user didn't select a file or if the file was invalid. True otherwise</returns>
        public static bool SetNewReplaceFileFromUser()
        {
            // configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "Open Text File";
            dialog.FileName = "Document"; // Default file name
            dialog.DefaultExt = ".txt"; // Default file extension
            dialog.Filter = "All files (*.*)|*.*"; // Filter files by extension

            // show open file dialog box
            bool? result = dialog.ShowDialog();

            // process open file dialog box results
            if (result != true)
            {
                return false;
            }

            // set the ReplaceFile name
            try
            {
                FileName = dialog.FileName;
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
