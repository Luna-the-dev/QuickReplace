using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;
using TextReplace.Core.Enums;
using TextReplace.Messages.Replace;
using TextReplace.Messages.Sources;
using TextReplace.Messages.Output;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    public partial class OutputViewModel : ObservableRecipient,
        IRecipient<OutputFilesMsg>,
        IRecipient<SelectedOutputFileMsg>,
        IRecipient<ReplacePhrasesMsg>,
        IRecipient<SourceFilesMsg>,
        IRecipient<WholeWordMsg>,
        IRecipient<CaseSensitiveMsg>,
        IRecipient<PreserveCaseMsg>,
        IDropTarget
    {   
        [ObservableProperty]
        private ObservableCollection<OutputFileWrapper> _outputFiles =
            new(OutputData.OutputFiles.Select(OutputFileWrapper.WrapOutputFile));

        [ObservableProperty]
        private bool _isReplacifyEnabled = false;

        [ObservableProperty]
        private OutputFileWrapper _selectedFile = new OutputFileWrapper();
        partial void OnSelectedFileChanged(OutputFileWrapper value)
        {
            IsFileSelected = (value.FileName != string.Empty);
        }

        [ObservableProperty]
        private bool _isFileSelected = (OutputData.SelectedFile.FileName != "");

        [ObservableProperty]
        private string _filesNeededText = string.Empty;

        [ObservableProperty]
        private bool _areFilesNeeded = false;
        [ObservableProperty]
        private bool _doOutputFilesExist = false;

        [ObservableProperty]
        private string _searchText = string.Empty;
        partial void OnSearchTextChanged(string value)
        {
            UpdateOutputFilesView(SelectedFile.FileName);
        }

        [ObservableProperty]
        private bool _isWholeWord = OutputData.WholeWord;
        [ObservableProperty]
        private bool _isCaseSensitive = OutputData.CaseSensitive;
        [ObservableProperty]
        private bool _isPreserveCase = OutputData.PreserveCase;

        public static RelayCommand<object> SetSelectedFileCommand => new RelayCommand<object>(SetSelectedFile);
        public static RelayCommand ToggleWholeWordCommand => new RelayCommand(ToggleWholeWord);
        public static RelayCommand ToggleCaseSensitiveCommand => new RelayCommand(ToggleCaseSensitive);
        public static RelayCommand TogglePreserveCaseCommand => new RelayCommand(TogglePreserveCase);

        public OutputViewModel()
        {
            SetIsReplacifyEnabled();
        }

        protected override void OnActivated()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        private static void SetSelectedFile(object? file)
        {
            // for some reason if i pass in the SourceFileWrapper, this doesn't fire
            // on the first click, however if i pass it as a generic obect and
            // cast it to ReplacePhrase, it works. probably some MVVM community toolkit quirk
            if (file == null)
            {
                return;
            }
            OutputFileWrapper f = (OutputFileWrapper)file;
            OutputData.SelectedFile = OutputFileWrapper.UnwrapOutputFile(f);
        }

        private static void ToggleWholeWord()
        {
            OutputData.WholeWord = !OutputData.WholeWord;
        }

        private static void ToggleCaseSensitive()
        {
            OutputData.CaseSensitive = !OutputData.CaseSensitive;
        }

        private static void TogglePreserveCase()
        {
            OutputData.PreserveCase = !OutputData.PreserveCase;
        }

        public static async Task<bool> ReplaceSelected(bool openFileLocation)
        {
            var result = false;
            await Task.Run(() =>
            {
                OutputData.OpenFileLocation = openFileLocation;
                result = PerformReplacements([OutputData.SelectedFile.SourceFileName], [OutputData.SelectedFile.FileName]);
            });
            return result;
        }

        public static async Task<bool> ReplaceAll(bool openFileLocation)
        {
            var result = false;
            await Task.Run(() =>
            {
                OutputData.OpenFileLocation = openFileLocation;
                result = PerformReplacements(
                    OutputData.OutputFiles.Select(x => x.SourceFileName).ToList(),
                    OutputData.OutputFiles.Select(x => x.FileName).ToList());
            });
            return result;
        }

        private static bool PerformReplacements(List<string> sourceFiles, List<string> destFiles)
        {
            if (ReplaceData.FileName == string.Empty || SourceFilesData.SourceFiles.Count == 0)
            {
                Debug.WriteLine("Replace file or source files were empty. This should never be reached...");
                return false;
            }

            // perform the text replacements
            bool result = OutputData.PerformReplacements(
                ReplaceData.ReplacePhrasesDict,
                sourceFiles,
                destFiles,
                OutputData.WholeWord,
                OutputData.CaseSensitive,
                OutputData.PreserveCase);

            if (result == false)
            {
                Debug.WriteLine("A replacement could not be made.");
            }
            else
            {
                Debug.WriteLine("Replacements successfully performed.");
            }
            return result;
        }

        public void SetSelectedOutputFileType(OutputFileTypeEnum fileType)
        {
            OutputData.SetOutputFileType(SelectedFile.SourceFileName, fileType);
        }

        public static void SetAllOutputFileTypes(OutputFileTypeEnum fileType)
        {
            OutputData.SetAllOutputFileTypes(fileType);
        }

        public static void SetOutputFilesStyling(bool bold, bool italics,
            bool underline, bool strikethrough, bool isHighlighted, bool isTextColored,
            Color highlightColor, Color textColor)
        {
            OutputData.OutputFilesStyling = new OutputFileStyling(
                bold, italics, underline, strikethrough,
                isHighlighted, isTextColored, highlightColor, textColor);
        }

        public static void SetOutputFilesStylingDefault()
        {
            OutputData.OutputFilesStyling = new OutputFileStyling();
        }

        private void SetIsReplacifyEnabled()
        {
            bool areFilesNeeded = false;

            if (ReplaceData.ReplacePhrasesList.Count == 0 && SourceFilesData.SourceFiles.Count == 0)
            {
                FilesNeededText = "Please upload Replacements\nand Source Files.";
                areFilesNeeded = true;
            }
            else if (ReplaceData.ReplacePhrasesList.Count == 0)
            {
                FilesNeededText = "Please upload Replacements.";
                areFilesNeeded = true;
            }
            else if (SourceFilesData.SourceFiles.Count == 0)
            {
                FilesNeededText = "Please upload Source Files.";
                areFilesNeeded = true;
            }
            
            if (areFilesNeeded)
            {
                IsReplacifyEnabled = false;
                AreFilesNeeded = true;
                DoOutputFilesExist = false;
                return;
            }
            IsReplacifyEnabled = true;
            AreFilesNeeded = false;
            DoOutputFilesExist = true;
        }

        public static void SetRetryReplacementsOnFile(bool retryReplacementsOnFile)
        {
            OutputData.RetryReplacementsOnFile = retryReplacementsOnFile;
        }

        /// <summary>
        /// Updates the output files view by search term. Pass an empty string to deselect the file.
        /// </summary>
        /// <param name="selectedFile"></param>
        private void UpdateOutputFilesView(string selectedFile)
        {
            if (SearchText == string.Empty)
            {
                OutputFiles = new ObservableCollection<OutputFileWrapper>(
                    OutputData.OutputFiles.Select(OutputFileWrapper.WrapOutputFile));
            }
            else
            {
                OutputFiles = new ObservableCollection<OutputFileWrapper>(
                    OutputData.OutputFiles.Select(OutputFileWrapper.WrapOutputFile)
                    .Where(x => x.FileName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            // if the selected file is not in the search, clear the selected file
            if (OutputFiles.Any(x => x.FileName == selectedFile) == false)
            {
                OutputData.SelectedFile = new OutputFile();
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = System.Windows.DragDropEffects.Copy;
        }

        public void Drop(IDropInfo dropInfo)
        {
            var droppedItem = (OutputFileWrapper)dropInfo.Data;

            // grab the old index of the replace phrase
            int oldIndex = OutputFiles.IndexOf(droppedItem);
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

            OutputData.MoveOutputFile(oldIndex, dropInfo.InsertIndex);
        }

        public void Receive(OutputFilesMsg message)
        {
            UpdateOutputFilesView(SelectedFile.FileName);
        }

        public void Receive(SelectedOutputFileMsg message)
        {
            SelectedFile = OutputFileWrapper.WrapOutputFile(message.Value);
            // UpdateOutputFilesView(SelectedFile.FileName);
        }

        public void Receive(ReplacePhrasesMsg message)
        {
            // check to see if replacements and source files are uploaded
            SetIsReplacifyEnabled();
        }

        public void Receive(SourceFilesMsg message)
        {
            // check to see if replacements and source files are uploaded
            SetIsReplacifyEnabled();
        }

        public void Receive(WholeWordMsg message)
        {
            IsWholeWord = message.Value;
        }

        public void Receive(CaseSensitiveMsg message)
        {
            IsCaseSensitive = message.Value;
        }

        public void Receive(PreserveCaseMsg message)
        {
            IsPreserveCase = message.Value;
        }
    }

    public class OutputFileWrapper
    {
        public string FileName { get; set; }
        public string ShortFileName { get; set; }
        public string SourceFileName { get; set; }
        public int NumOfReplacements { get; set; }
        public string NumOfReplacementsString { get; set; }
        public bool IsSelected { get; set; }

        public OutputFileWrapper()
        {
            FileName = string.Empty;
            ShortFileName = string.Empty;
            SourceFileName = string.Empty;
            NumOfReplacements = -1;
            NumOfReplacementsString = string.Empty;
            SetNumOfReplacementsString();
            IsSelected = false;
        }

        public OutputFileWrapper(OutputFile file, bool isSelected = false)
        {
            FileName = file.FileName;
            ShortFileName = file.ShortFileName;
            SourceFileName = file.SourceFileName;
            NumOfReplacements = file.NumOfReplacements;
            NumOfReplacementsString = string.Empty;
            SetNumOfReplacementsString();
            IsSelected = isSelected;
        }

        public OutputFileWrapper(
            string fileName,
            string shortFileName,
            string sourceFileName,
            int numOfReplacements,
            bool isSelected = false)
        {
            var file = new OutputFile(fileName, shortFileName, sourceFileName, numOfReplacements);

            FileName = file.FileName;
            ShortFileName = file.ShortFileName;
            SourceFileName = file.SourceFileName;
            NumOfReplacements = file.NumOfReplacements;
            NumOfReplacementsString = string.Empty;
            SetNumOfReplacementsString();
            IsSelected = isSelected;
        }

        public void SetNumOfReplacementsString()
        {
            NumOfReplacementsString = (NumOfReplacements < 0) ?
                "Replacements were not yet made." :
                $"{NumOfReplacements:n0} replacements were made.";
        }

        public static OutputFileWrapper WrapOutputFile(OutputFile file)
        {
            if (file.FileName == OutputData.SelectedFile.FileName)
            {
                return new OutputFileWrapper(file, true);
            }
            return new OutputFileWrapper(file);
        }

        public static OutputFile UnwrapOutputFile(OutputFileWrapper file)
        {
            return new OutputFile(file.FileName, file.ShortFileName, file.SourceFileName, file.NumOfReplacements);
        }
    }
}
