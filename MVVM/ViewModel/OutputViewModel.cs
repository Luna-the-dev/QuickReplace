using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Windows;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    partial class OutputViewModel : ObservableRecipient
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

        public RelayCommand<object> SetSelectedFileCommand => new RelayCommand<object>(SetSelectedFile);

        public OutputViewModel()
        {
            SetIsReplacifyEnabled();
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
            OutputFileWrapper f = (OutputFileWrapper)file;
            OutputData.SelectedFile = OutputFileWrapper.UnwrapOutputFile(f);
        }

        private void SetIsReplacifyEnabled()
        {
            string newFilesNeededText = string.Empty;

            if (ReplaceData.ReplacePhrasesList.Count > 0)
            {
                newFilesNeededText += "Please upload Replacements.";
            }

            if (SourceFilesData.SourceFiles.Count > 0)
            {
                if (newFilesNeededText != string.Empty)
                {
                    newFilesNeededText += "\n";
                }
                newFilesNeededText += "Please upload Source Files.";
            }

            FilesNeededText = newFilesNeededText;

            if (newFilesNeededText != string.Empty)
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
            if (SearchText != string.Empty)
            {
                OutputFiles = new ObservableCollection<OutputFileWrapper>(
                    OutputData.OutputFiles.Select(OutputFileWrapper.WrapOutputFile));
            }
            else
            {
                OutputFiles = new ObservableCollection<OutputFileWrapper>(
                    OutputData.OutputFiles.Select(OutputFileWrapper.WrapOutputFile)
                    .Where(x => x.FileName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

                // if the selected file is not in the search, clear the selected file
                if (OutputFiles.Any(x => x.FileName == selectedFile) == false)
                {
                    SelectedFile = new OutputFileWrapper();
                }
            }
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
                $"{NumOfReplacements} replacements were made.";
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
