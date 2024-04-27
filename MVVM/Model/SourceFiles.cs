using System.IO;
using TextReplace.Core;

namespace TextReplace.MVVM.Model
{
    class SourceFiles
    {
        private static List<string> _fileNames = new List<string>();

        public static List<string> FileNames
        {
            get => _fileNames;
            set
            {
                if (FileValidation.AreFileNamesValid(value))
                {
                    _fileNames = value;
                }
                else
                {
                    throw new Exception("An input file is not readable. SourceFiles was not updated.");
                }
            }
        }

        /// <summary>
        /// Opens a file dialogue and replaces the SourceFiles list with whatever the user selects (if valid)
        /// </summary>
        /// <returns> False if the user didn't select a file or if one of the files was invalid. True otherwise
        /// </returns>
        public static bool SetNewSourceFilesFromUser()
        {
            List<string> filenames = new List<string>();

            // configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "Open Text Files";
            dialog.FileName = "Document"; // Default file name
            dialog.DefaultExt = ".txt"; // Default file extension
            dialog.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension
            dialog.Multiselect = true;

            // show open file dialog box
            bool? result = dialog.ShowDialog();

            // process open file dialog box results
            if (result != true)
            {
                return false;
            }

            // set the SourceFiles names
            try
            {
                FileNames = dialog.FileNames.ToList();
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates a list of destination file names by adding a suffix onto the SrouceFile file names
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns>A list of destination file names</returns>
        public static List<string> GenerateDestFileNames(string suffix)
        {
            List<string> destFileNames = new List<string>();
            foreach (var name in FileNames)
            {
                destFileNames.Add(string.Format(@"{0}\{1}-{2}{3}",
                                                Path.GetDirectoryName(name),
                                                Path.GetFileNameWithoutExtension(name),
                                                suffix,
                                                Path.GetExtension(name)
                                                ));
            }

            return destFileNames;
        }

    }
}
