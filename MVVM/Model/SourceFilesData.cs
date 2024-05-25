using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.IO;
using TextReplace.Core.Validation;
using TextReplace.Messages.Replace;
using TextReplace.Messages.Sources;

namespace TextReplace.MVVM.Model
{
    class SourceFilesData
    {
        private static List<string> _fileNames = new List<string>();
        public static List<string> FileNames
        {
            get { return _fileNames; }
            set { _fileNames = value; }
        }
        // optional user specified file path for the output files
        private static string _outputDirectory = string.Empty;
        public static string OutputDirectory
        {
            get { return _outputDirectory; }
            set { _outputDirectory = value; }
        }
        // optional user specified suffix for the output files
        private static string _suffix = string.Empty;
        public static string Suffix
        {
            get { return _suffix; }
            set
            {
                _suffix = value;
                WeakReferenceMessenger.Default.Send(new SuffixMsg(value));
            }
        }

        /// <summary>
        /// Opens a file dialogue and replaces the SourceFilesData list with whatever the user selects (if valid)
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns>
        /// False if one of the files was invalid, null user closed the window without selecting a file.
        /// </returns>
        public static bool SetNewSourceFilesFromUser(string[] fileNames)
        {
            // set the SourceFilesData names
            if (FileValidation.AreFileNamesValid(fileNames.ToList()))
            {
                Debug.WriteLine("An input file is not readable. SourceFilesData was not updated.");
                return false;
            }

            FileNames = fileNames.ToList();
            return true;
        }

        /// <summary>
        /// Generates a list of destination file names by adding a suffix onto the SrouceFile file names
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns>A list of destination file names</returns>
        public static List<string> GenerateDestFileNames()
        {
            List<string> destFileNames = new List<string>();
            foreach (var name in FileNames)
            {
                string? directory = (OutputDirectory == string.Empty) ? Path.GetDirectoryName(name) : OutputDirectory;
                string suffix = (Suffix == string.Empty) ? "-replacify" : Suffix;
                destFileNames.Add(string.Format(@"{0}\{1}{2}{3}",
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
