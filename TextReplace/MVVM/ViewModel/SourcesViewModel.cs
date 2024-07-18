using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using TextReplace.Messages.Replace;
using TextReplace.Messages.Sources;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    public partial class SourcesViewModel : ObservableRecipient,
        IRecipient<SourceFilesMsg>,
        IRecipient<SelectedSourceFileMsg>,
        IRecipient<DefaultSourceFileOptionsMsg>,
        IDropTarget
    {
        [ObservableProperty]
        private ObservableCollection<SourceFileWrapper> _sourceFiles =
            new(SourceFilesData.SourceFiles.Select(SourceFileWrapper.WrapSourceFile));
        partial void OnSourceFilesChanged(ObservableCollection<SourceFileWrapper> value)
        {
            IsSourceFileUploaded = (value.Count > 0);
        }

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

        [ObservableProperty]
        private SourceFile _defaultSourceFileOptions = SourceFilesData.DefaultSourceFileOptions;

        public RelayCommand<object> SetSelectedFileCommand => new RelayCommand<object>(SetSelectedFile);

        protected override void OnActivated()
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

        /// <summary>
        /// Updates the output directory for the selected source file
        /// </summary>
        /// <param name="directoryName"></param>
        public void UpdateSourceFileOutputDirectory(string directoryName)
        {
            SourceFilesData.UpdateSourceFileOptions(SelectedFile.FileName, outputDirectory: directoryName);
        }

        /// <summary>
        /// Updates the output directory for all source files
        /// </summary>
        /// <param name="directoryName"></param>
        public static void UpdateAllSourceFileOutputDirectories(string directoryName)
        {
            SourceFilesData.UpdateAllSourceFileOptions(outputDirectory: directoryName);
        }

        /// <summary>
        /// Updates the suffix for the selected source file
        /// </summary>
        /// <param name="suffix"></param>
        public void UpdateSourceFileSuffixes(string suffix)
        {
            SourceFilesData.UpdateSourceFileOptions(SelectedFile.FileName, suffix: suffix);
        }

        /// <summary>
        /// Updates the suffix for all source files
        /// </summary>
        /// <param name="suffix"></param>
        public static void UpdateAllSourceFileSuffixes(string suffix)
        {
            SourceFilesData.UpdateAllSourceFileOptions(suffix: suffix);
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
            }

            // if the selected file is not in the search, clear the selected file
            if (SourceFiles.Any(x => x.FileName == selectedFile) == false)
            {
                SourceFilesData.SelectedFile = new SourceFile();
            }
        }

        /// <summary>
        /// Wrapper for SourceFilesData.SetNewSourceFiles
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public static bool AddNewSourceFiles(List<string> fileNames)
        {
            return SourceFilesData.AddNewSourceFiles(fileNames);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = System.Windows.DragDropEffects.Copy;
        }

        public void Drop(IDropInfo dropInfo)
        {
            var droppedItem = (SourceFileWrapper)dropInfo.Data;

            // grab the old index of the replace phrase
            int oldIndex = SourceFiles.IndexOf(droppedItem);
            if (oldIndex < 0)
            {
                Debug.WriteLine("Dropped item does not exist.");
                return;
            }

            // the item was dropped into the same position was it was in before. do nothing
            if (dropInfo.InsertIndex == oldIndex || dropInfo.InsertIndex == oldIndex + 1)
            {
                return;
            }

            SourceFilesData.MoveSourceFile(oldIndex, dropInfo.InsertIndex);
        }

        public void Receive(SourceFilesMsg message)
        {
            SourceFiles = new ObservableCollection<SourceFileWrapper>(message.Value.Select(SourceFileWrapper.WrapSourceFile));
        }

        public void Receive(SelectedSourceFileMsg message)
        {
            SelectedFile = SourceFileWrapper.WrapSourceFile(message.Value);
        }

        public void Receive(DefaultSourceFileOptionsMsg message)
        {
            DefaultSourceFileOptions = message.Value;
        }
    }

    public class SourceFileWrapper
    {
        public string FileName { get; set; }
        public string ShortFileName { get; set; }
        public string OutputDirectory { get; set; }
        public string OutputDirectoryText { get; set; }
        public string Suffix { get; set; }
        public string SuffixText { get; set; }
        public bool IsSelected { get; set; }

        public SourceFileWrapper()
        {
            FileName = string.Empty;
            ShortFileName = string.Empty;
            OutputDirectory = string.Empty;
            OutputDirectoryText = string.Empty;
            Suffix = string.Empty;
            SuffixText = string.Empty;
            IsSelected = false;
        }

        public SourceFileWrapper(SourceFile file, bool isSelected = false)
        {
            FileName = file.FileName;
            ShortFileName = Path.GetFileName(file.FileName);
            OutputDirectory = file.OutputDirectory;
            OutputDirectoryText = (OutputDirectory == string.Empty) ? "Default" : OutputDirectory;
            Suffix = file.Suffix;
            SuffixText = (Suffix == string.Empty) ? "Default" : Suffix;
            IsSelected = isSelected;
        }

        public static SourceFileWrapper WrapSourceFile(SourceFile file)
        {
            if (file.FileName == SourceFilesData.SelectedFile.FileName)
            {
                return new SourceFileWrapper(file, true);
            }
            return new SourceFileWrapper(file);
        }

        public static SourceFile UnwrapSourceFile(SourceFileWrapper file)
        {
            return new SourceFile(file.FileName, file.OutputDirectory, file.Suffix);
        }
    }
}
