using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using TextReplace.Core.Validation;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    partial class UploadSourceFilesInputViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private List<string> _fullFileNames = new List<string>();
        partial void OnFullFileNamesChanged(List<string> value)
        {
            FileNameOrCount = (value.Count == 1) ? Path.GetFileName(value[0]) : $"{value.Count} files uploaded.";
        }

        [ObservableProperty]
        private string _fileNameOrCount = string.Empty;

        [ObservableProperty]
        private Visibility _showFileNameOrCount = Visibility.Collapsed;
        [ObservableProperty]
        private Visibility _fileIsValid = Visibility.Collapsed;
        [ObservableProperty]
        private Visibility _fileIsInvalid = Visibility.Collapsed;

        [ObservableProperty]
        private bool _confirmIsClickable = false;

        public bool ValidateFiles(List<string> fileNames)
        {
            string invalidFileName = "";
            foreach (var file in fileNames)
            {
                if (FileValidation.IsSourceFileTypeValid(file) == false || 
                    FileValidation.IsInputFileReadable(file) == false)
                {
                    invalidFileName = file;
                    break;
                }
            }

            // if one of the files is invalid
            if (invalidFileName != string.Empty)
            {
                FullFileNames = new List<string>(){ invalidFileName };
                ShowFileNameOrCount = Visibility.Visible;
                FileIsValid = Visibility.Collapsed;
                FileIsInvalid = Visibility.Visible;
                ConfirmIsClickable = false;
                return false;
            }

            // if only one file was selected
            if (fileNames.Count <= 1)
            {
                FullFileNames = fileNames;
                ShowFileNameOrCount = Visibility.Visible;
                FileIsValid = Visibility.Visible;
                FileIsInvalid = Visibility.Collapsed;
                ConfirmIsClickable = true;
                return true;
            }

            // if multiple files were selected
            FullFileNames = fileNames;
            ShowFileNameOrCount = Visibility.Visible;
            FileIsValid = Visibility.Visible;
            FileIsInvalid = Visibility.Collapsed;
            ConfirmIsClickable = true;
            return true;
        }
    }
}
