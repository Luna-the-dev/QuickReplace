using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using TextReplace.Messages.Replace;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    partial class OutputViewModel : ObservableRecipient,
        IRecipient<OutputFilesMsg>,
        IRecipient<SelectedOutputFileMsg>,
        IRecipient<ReplacePhrasesMsg>,
        IRecipient<SourceFilesMsg>,
        IRecipient<WholeWordMsg>,
        IRecipient<CaseSensitiveMsg>
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
        private Visibility _isFilesNeededVisible = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _areOutputFilesVisible = Visibility.Hidden;

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

        public RelayCommand ReplaceAllCommand => new RelayCommand(ReplaceAll);
        public RelayCommand ReplaceSelectedCommand => new RelayCommand(ReplaceSelected);
        public RelayCommand<object> SetSelectedFileCommand => new RelayCommand<object>(SetSelectedFile);
        public RelayCommand ToggleWholeWordCommand => new RelayCommand(ToggleWholeWord);
        public RelayCommand ToggleCaseSensitiveCommand => new RelayCommand(ToggleCaseSensitive);

        public OutputViewModel()
        {
            SetIsReplacifyEnabled();
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        private void ReplaceAll()
        {
            PerformReplacements(
                SourceFilesData.SourceFiles.Select(x => x.FileName).ToList(),
                OutputData.OutputFiles.Select(x => x.FileName).ToList());
        }

        private void ReplaceSelected()
        {
            PerformReplacements([OutputData.SelectedFile.SourceFileName], [OutputData.SelectedFile.FileName]);
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
            OutputFileWrapper f = (OutputFileWrapper)file;
            OutputData.SelectedFile = OutputFileWrapper.UnwrapOutputFile(f);
        }

        private void ToggleWholeWord()
        {
            OutputData.WholeWord = !OutputData.WholeWord;
        }

        private void ToggleCaseSensitive()
        {
            OutputData.CaseSensitive = !OutputData.CaseSensitive;
        }

        private static void PerformReplacements(List<string> sourceFiles, List<string> destFiles)
        {
            if (ReplaceData.FileName == string.Empty || SourceFilesData.SourceFiles.Count == 0)
            {
                Debug.WriteLine("Replace file or source files were empty. This should never be reached...");
                return;
            }

            // perform the text replacements
            bool result = OutputData.PerformReplacements(
                ReplaceData.ReplacePhrasesDict,
                sourceFiles,
                destFiles,
                OutputData.WholeWord,
                OutputData.CaseSensitive);

            Debug.WriteLine("Output file names:");
            destFiles.ForEach(o => Debug.WriteLine($"\t{o}"));

            if (result == false)
            {
                Debug.WriteLine("A replacement could not be made.");
            }
            else
            {
                Debug.WriteLine("Replacements successfully performed.");
            }
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
                IsFilesNeededVisible = Visibility.Visible;
                AreOutputFilesVisible = Visibility.Hidden;
                return;
            }
            IsReplacifyEnabled = true;
            IsFilesNeededVisible = Visibility.Hidden;
            AreOutputFilesVisible = Visibility.Visible;
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

        public void Receive(OutputFilesMsg message)
        {
            UpdateOutputFilesView(SelectedFile.FileName);
        }

        public void Receive(SelectedOutputFileMsg message)
        {
            SelectedFile = OutputFileWrapper.WrapOutputFile(message.Value);
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
    }

    class OutputFileWrapper
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
