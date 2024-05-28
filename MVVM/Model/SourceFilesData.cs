using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.IO;
using TextReplace.Core.Validation;
using TextReplace.Messages.Replace;
using TextReplace.Messages.Sources;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace TextReplace.MVVM.Model
{
    class SourceFilesData
    {
        private static List<string> _fileNames = [];
        public static List<string> FileNames
        {
            get { return _fileNames; }
            set
            {
                _fileNames = value;
                WeakReferenceMessenger.Default.Send(new SourceFileNamesMsg(value));
            }
        }
        private static Dictionary<string, SourceFileOptions> _sourceFileOptionsDict = [];
        public static Dictionary<string, SourceFileOptions> SourceFileOptionsDict
        {
            get { return _sourceFileOptionsDict; }
            set
            {
                _sourceFileOptionsDict = value;
                WeakReferenceMessenger.Default.Send(new SourceFileOptionsMsg(value));
            }
        }
        private static SourceFileOptions _defaultSourceFileOptions = new SourceFileOptions("", "");
        public static SourceFileOptions DefaultSourceFileOptions
        {
            get { return _defaultSourceFileOptions; }
            set
            {
                _defaultSourceFileOptions = value;
                WeakReferenceMessenger.Default.Send(new DefaultSourceFileOptionsMsg(value));
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
                if (SourceFileOptionsDict.ContainsKey(fileName) == false)
                {
                    FileNames.Add(fileName);
                    SourceFileOptionsDict.Add(fileName, DefaultSourceFileOptions);
                }
            }
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
                string? directory = (SourceFileOptionsDict[name].OutputDirectory == string.Empty) ?
                    Path.GetDirectoryName(name) :
                    SourceFileOptionsDict[name].OutputDirectory;

                string suffix = (SourceFileOptionsDict[name].Suffix == string.Empty) ?
                    "-replacify" :
                    SourceFileOptionsDict[name].Suffix;

                destFileNames.Add(string.Format(@"{0}\{1}{2}{3}",
                                                directory,
                                                Path.GetFileNameWithoutExtension(name),
                                                suffix,
                                                Path.GetExtension(name)
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
            SourceFileOptions newDefaultOptions = DefaultSourceFileOptions;

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
        public static void UpdateSourceFileOptions(string? outputDirectory = null, string? suffix = null)
        {
            if (outputDirectory != null)
            {
                var newSfoDict = SourceFileOptionsDict;
                foreach (var sfo in newSfoDict.Values)
                {
                    sfo.OutputDirectory = outputDirectory;
                }
                SourceFileOptionsDict = newSfoDict;

                DefaultSourceFileOptions.OutputDirectory = outputDirectory;
            }

            if (suffix != null)
            {
                var newSfoDict = SourceFileOptionsDict;
                foreach (var sfo in newSfoDict.Values)
                {
                    sfo.Suffix = suffix;
                }
                SourceFileOptionsDict = newSfoDict;

                DefaultSourceFileOptions.Suffix = suffix;
            }
        }
    }

    class SourceFileOptions(string outputDirectory, string suffix)
    {
        public string OutputDirectory { get; set; } = outputDirectory;
        public string Suffix { get; set; } = suffix;
    }
}
