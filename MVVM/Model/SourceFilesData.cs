using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using TextReplace.Core.Validation;

namespace TextReplace.MVVM.Model
{
    class SourceFilesData
    {
        private static List<string> _fileNames = new List<string>();
        public static List<string> FileNames
        {
            get { return _fileNames; }
            set
            {
                if (FileValidation.AreFileNamesValid(value))
                {
                    _fileNames = value;
                }
                else
                {
                    throw new Exception("An input file is not readable. SourceFilesData was not updated.");
                }
            }
        }
        // optional user specified file path for the output file
        private static string _outputDirectory = string.Empty;
        public static string OutputDirectory
        {
            get { return _outputDirectory; }
            set { _outputDirectory = value; }
        }

        /// <summary>
        /// Opens a file dialogue and replaces the SourceFilesData list with whatever the user selects (if valid)
        /// </summary>
        /// <returns>
        /// False if one of the files was invalid, null user closed the window without selecting a file.
        /// </returns>
        public static bool? SetNewSourceFilesFromUser()
        {
            // configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "Open Text Files";
            dialog.FileName = "Document"; // Default file name
            dialog.DefaultExt = ".txt"; // Default file extension
            dialog.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension
            dialog.Multiselect = true;

            // open file dialog box
            if (dialog.ShowDialog() != true)
            {
                Debug.WriteLine("Replace file upload window was closed.");
                return null;
            }

            // set the SourceFilesData names
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
                string? directory = (OutputDirectory == string.Empty) ? Path.GetDirectoryName(name) : OutputDirectory;
                destFileNames.Add(string.Format(@"{0}\{1}-{2}{3}",
                                                directory,
                                                Path.GetFileNameWithoutExtension(name),
                                                suffix,
                                                Path.GetExtension(name)
                                                ));
            }

            return destFileNames;
        }

    }
}
