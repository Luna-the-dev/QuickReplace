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
        private static List<SourceFile> _sourceFiles = [];
        public static List<SourceFile> SourceFiles
        {
            get { return _sourceFiles; }
            set
            {
                _sourceFiles = value;
                WeakReferenceMessenger.Default.Send(new SourceFilesMsg(value));
            }
        }
        private static SourceFile _defaultSourceFileOptions = new SourceFile();
        public static SourceFile DefaultSourceFileOptions
        {
            get { return _defaultSourceFileOptions; }
            set
            {
                _defaultSourceFileOptions = value;
                WeakReferenceMessenger.Default.Send(new DefaultSourceFileOptionsMsg(value));
            }
        }
        private static SourceFile _selectedFile = new SourceFile();
        public static SourceFile SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                _selectedFile = value;
            }
        }

        /// <summary>
        /// Opens a file dialogue and replaces the SourceFilesData list with whatever the user selects (if valid)
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns>
        /// False if one of the files was invalid, null user closed the window without selecting a file.
        /// </returns>
        public static bool SetNewSourceFiles(List<string> fileNames)
        {
            // set the SourceFilesData names
            if (FileValidation.AreFileNamesValid(fileNames) == false)
            {
                Debug.WriteLine("An input file is not readable. SourceFilesData was not updated.");
                return false;
            }

            foreach (string fileName in fileNames)
            {
                // only add the file to the list/dict if it wasnt already
                // in there to prevefnt duplicates/overwrites
                if (SourceFiles.Any(x => x.FileName == fileName) == false)
                {
                    SourceFiles.Add(new SourceFile(
                        fileName,
                        DefaultSourceFileOptions.OutputDirectory,
                        DefaultSourceFileOptions.Suffix));
                }
            }

            WeakReferenceMessenger.Default.Send(new SourceFilesMsg(SourceFiles));
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

            foreach (var file in SourceFiles)
            {
                string? directory = (file.OutputDirectory == string.Empty) ?
                    Path.GetDirectoryName(file.FileName) :
                    file.OutputDirectory;

                string suffix = (file.Suffix == string.Empty) ?
                    "-replacify" :
                    file.Suffix;

                destFileNames.Add(string.Format(@"{0}\{1}{2}{3}",
                                                directory,
                                                Path.GetFileNameWithoutExtension(file.FileName),
                                                suffix,
                                                Path.GetExtension(file.FileName)
                                                ));
            }

            return destFileNames;
        }

        /// <summary>
        /// Sets one or multiple of the DefaultSourceFileOptions values
        /// </summary>
        /// <param name="caseSensitive"></param>
        /// <param name="wholeWord"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="suffix"></param>
        public static void SetDefaultSourceFileOptions(
            string? outputDirectory = null,
            string? suffix = null)
        {
            // create a local variable so that you only have to change the default options one time
            SourceFile newDefaultOptions = DefaultSourceFileOptions;

            if (outputDirectory != null)
            {
                newDefaultOptions.OutputDirectory = outputDirectory;
            }
            if (suffix != null)
            {
                newDefaultOptions.Suffix = suffix;
            }

            DefaultSourceFileOptions = newDefaultOptions;
        }

        /// <summary>
        /// Updates the output directory and/or the suffix of all files, as well as the default option for the files.
        /// </summary>
        /// <param name="outputDirectory"></param>
        /// <param name="suffix"></param>
        public static void UpdateAllSourceFileOptions(string? outputDirectory = null, string? suffix = null)
        {
            var newSourceFiles = SourceFiles;
            var newDefaultSfo = DefaultSourceFileOptions;

            if (outputDirectory != null)
            {
                foreach (var file in SourceFiles)
                {
                    file.OutputDirectory = outputDirectory;
                }

                newDefaultSfo.OutputDirectory = outputDirectory;
            }

            if (suffix != null)
            {
                foreach (var file in SourceFiles)
                {
                    file.Suffix = suffix;
                }

                newDefaultSfo.Suffix = suffix;
            }

            SourceFiles = newSourceFiles;
            DefaultSourceFileOptions = newDefaultSfo;
        }

        public static bool RemoveSourceFile(string fileName)
        {
            var index = SourceFiles.FindIndex(x => x.FileName == fileName);
            if (index == -1)
            {
                Debug.WriteLine($"{fileName} was not found in SourceFiles and was not removed.");
                return false;
            }

            SourceFiles.RemoveAt(index);
            WeakReferenceMessenger.Default.Send(new SourceFilesMsg(SourceFiles));
            return true;
        }
    }

    class SourceFile
    {
        public string FileName { get; set; }
        public string ShortFileName { get; set; }
        public string OutputDirectory { get; set; }
        public string Suffix { get; set; }

        public SourceFile()
        {
            FileName = "";
            ShortFileName = "";
            OutputDirectory = "";
            Suffix = "";
        }

        public SourceFile(
            string fileName,
            string outputDirectory,
            string suffix)
        {
            FileName = fileName;
            ShortFileName = Path.GetFileName(fileName);
            OutputDirectory = outputDirectory;
            Suffix = suffix;
        }
    }
}
