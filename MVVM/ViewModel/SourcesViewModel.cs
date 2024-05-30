using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using TextReplace.Messages.Replace;
using TextReplace.MVVM.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace TextReplace.MVVM.ViewModel
{
    partial class SourcesViewModel : ObservableRecipient,
        IRecipient<SourceFilesMsg>,
        IRecipient<SelectedSourceFileMsg>
    {
        [ObservableProperty]
        private ObservableCollection<SourceFileWrapper> _sourceFiles =
            new ObservableCollection<SourceFileWrapper>(SourceFilesData.SourceFiles.Select(SourceFileWrapper.WrapSourceFile));

        [ObservableProperty]
        private bool _isSourceFileUploaded = (SourceFilesData.SourceFiles.Count > 0);

        [ObservableProperty]
        private SourceFileWrapper _selectedFile = new SourceFileWrapper();
        partial void OnSelectedFileChanged(SourceFileWrapper value)
        {
            IsFileSelected = value.FileName != string.Empty;
        }

        [ObservableProperty]
        private bool _isFileSelected = (SourceFilesData.SelectedFile.FileName != "");

        [ObservableProperty]
        private string _searchText = string.Empty;
        partial void OnSearchTextChanged(string value)
        {
            UpdateSourceFilesView(SelectedFile.FileName);
        }

        public RelayCommand<object> SetSelectedFileCommand => new RelayCommand<object>(SetSelectedFile);

        public SourcesViewModel()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        private void SetSelectedFile(object? file)
        {
            // for some reason if i pass in the SourceFileWrapper, this doesn't fire
            // on the first click, however if i pass it as a generic obect and
            // cast it to ReplacePhrase, it works. probably some MVVM community toolkit quirk
            if (file == null)
            {
                return;
            }
            SourceFileWrapper f = (SourceFileWrapper)file;
            SourceFilesData.SelectedFile = SourceFileWrapper.UnwrapSourceFile(f);
        }

        /// <summary>
        /// Removes a source file based on the index of the file.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveSourceFile(int index)
        {
            if (index > SourceFiles.Count - 1)
            {
                Debug.WriteLine("Source file index is invalid");
                return;
            }

            bool res = SourceFilesData.RemoveSourceFile(SourceFiles[index].FileName);
            if (res == false)
            {
                Debug.WriteLine("Source file could not be removed.");
                return;
            }
            UpdateSourceFilesView(SelectedFile.FileName);
        }

        /// <summary>
        /// Removes all source files.
        /// </summary>
        public static void RemoveAllSourceFiles()
        {
            SourceFilesData.SourceFiles = [];
        }

        public void UpdateSourceFileOutputDirectory(string directoryName)
        {
            SourceFilesData.UpdateSourceFileOptions(SelectedFile.FileName, outputDirectory: directoryName);
        }

        public static void UpdateAllSourceFileOutputDirectories(string directoryName)
        {
            SourceFilesData.UpdateAllSourceFileOptions(outputDirectory: directoryName);
        }

        /// <summary>
        /// Updates the source files view by search term. Pass an empty string to deselect the file.
        /// </summary>
        /// <param name="selectedFile"></param>
        private void UpdateSourceFilesView(string selectedFile)
        {
            if (SearchText == string.Empty)
            {
                SourceFiles = new ObservableCollection<SourceFileWrapper>(
                    SourceFilesData.SourceFiles.Select(SourceFileWrapper.WrapSourceFile));
            }
            else
            {
                SourceFiles = new ObservableCollection<SourceFileWrapper>(
                    SourceFilesData.SourceFiles.Select(SourceFileWrapper.WrapSourceFile)
                    .Where(x => x.FileName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
                
                // if the selected file is not in the search, clear the selected file
                if (SourceFiles.Any(x => x.FileName == selectedFile) == false)
                {
                    SelectedFile = new SourceFileWrapper();
                }
            }
        }

        /// <summary>
        /// Wrapper for SourceFilesData.SetNewSourceFiles
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public static bool SetNewSourceFiles(List<string> fileNames)
        {
            return SourceFilesData.SetNewSourceFiles(fileNames);
        }

        public void Receive(SourceFilesMsg message)
        {
            // if the source file has no custom output directory or suffix, replace the empty string with "Default"
            SourceFiles = new ObservableCollection<SourceFileWrapper>(message.Value.Select(x => {
                x.OutputDirectory = (x.OutputDirectory == string.Empty) ? "Default" : x.OutputDirectory;
                x.Suffix = (x.Suffix == string.Empty) ? "Default" : x.Suffix;
                return SourceFileWrapper.WrapSourceFile(x);
            }));

            IsSourceFileUploaded = (message.Value.Count > 0);
        }

        public void Receive(SelectedSourceFileMsg message)
        {
            SelectedFile = SourceFileWrapper.WrapSourceFile(message.Value);
        }
    }

    class SourceFileWrapper
    {
        public string FileName { get; set; }
        public string ShortFileName { get; set; }
        public string OutputDirectory { get; set; }
        public string Suffix { get; set; }
        public bool IsSelected { get; set; }

        public SourceFileWrapper()
        {
            FileName = "";
            ShortFileName = "";
            OutputDirectory = "";
            Suffix = "";
            IsSelected = false;
        }

        public SourceFileWrapper(
            string fileName,
            string outputDirectory,
            string suffix,
            bool isSelected = false)
        {
            FileName = fileName;
            ShortFileName = Path.GetFileName(fileName);
            OutputDirectory = outputDirectory;
            Suffix = suffix;
            IsSelected = isSelected;
        }

        public static SourceFileWrapper WrapSourceFile(SourceFile file)
        {
            if (file.FileName == SourceFilesData.SelectedFile.FileName)
            {
                return new SourceFileWrapper(file.FileName, file.OutputDirectory, file.Suffix, true);
            }
            return new SourceFileWrapper(file.FileName, file.OutputDirectory, file.Suffix);
        }

        public static SourceFile UnwrapSourceFile(SourceFileWrapper file)
        {
            return new SourceFile(file.FileName, file.OutputDirectory, file.Suffix);
        }
    }
}
