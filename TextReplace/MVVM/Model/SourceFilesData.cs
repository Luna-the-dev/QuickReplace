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
                WeakReferenceMessenger.Default.Send(new SelectedSourceFileMsg(value));
            }
        }

        /// <summary>
        /// Moves a source file in the SourceFiles list from its current position to a new index
        /// Updates the Output Files to reflect this as well
        /// </summary>
        /// <param name="oldIndex"></param>
        /// <param name="newIndex"></param>
        public static void MoveSourceFile(int oldIndex, int newIndex)
        {
            try
            {
                var sourceFile = SourceFiles[oldIndex];

                SourceFiles.RemoveAt(oldIndex);

                // shift the new index due to the removal if needed
                if (newIndex > oldIndex)
                {
                    newIndex--;
                }

                SourceFiles.Insert(newIndex, sourceFile);
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.WriteLine(e.Message);
            }

            WeakReferenceMessenger.Default.Send(new SourceFilesMsg(SourceFiles));
            OutputData.UpdateOutputFiles(SourceFiles);
        }

        /// <summary>
        /// Opens a file dialogue and replaces the SourceFilesData list with whatever the user selects (if valid)
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns>
        /// False if one of the files was invalid, null user closed the window without selecting a file.
        /// </returns>
        public static bool AddNewSourceFiles(List<string> fileNames)
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
            OutputData.UpdateOutputFiles(SourceFiles);
            return true;
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
        public static void UpdateSourceFileOptions(string fileName, string? outputDirectory = null, string? suffix = null)
        {
            int index = SourceFiles.FindIndex(x => x.FileName == fileName);

            if (index < 0)
            {
                Debug.WriteLine("Filename could not be found, source file option not updated.");
                return;
            }

            if (outputDirectory != null)
            {
                SourceFiles[index].OutputDirectory = outputDirectory;
            }

            if (suffix != null)
            {
                SourceFiles[index].Suffix = suffix;
            }

            WeakReferenceMessenger.Default.Send(new SourceFilesMsg(SourceFiles));
            OutputData.UpdateOutputFiles(SourceFiles);
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
            OutputData.UpdateOutputFiles(newSourceFiles);
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

            if (SourceFiles[index].FileName == SelectedFile.FileName)
            {
                SelectedFile = new SourceFile();
            }

            SourceFiles.RemoveAt(index);
            WeakReferenceMessenger.Default.Send(new SourceFilesMsg(SourceFiles));
            OutputData.UpdateOutputFiles(SourceFiles);
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
            FileName = string.Empty;
            ShortFileName = string.Empty;
            OutputDirectory = string.Empty;
            Suffix = string.Empty;
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
