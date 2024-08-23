using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.IO;
using TextReplace.Core.Validation;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    public partial class UploadSourceFilesInputViewModel : ObservableRecipient
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
        private bool _showFileNameOrCount = false;
        [ObservableProperty]
        private bool _fileIsValid = false;
        [ObservableProperty]
        private bool _fileIsInvalid = false;

        [ObservableProperty]
        private bool _confirmIsClickable = false;

        public bool ValidateFiles(List<string> fileNames)
        {
            string invalidFileName = "";
            foreach (var file in fileNames)
            {
                if (FileValidation.IsInputFileReadable(file) == false ||
                    FileValidation.IsSourceFileTypeValid(file) == false)
                {
                    invalidFileName = file;
                    break;
                }
            }

            // if one of the files is invalid
            if (invalidFileName != string.Empty)
            {
                FullFileNames = new List<string>(){ invalidFileName };
                ShowFileNameOrCount = true;
                FileIsValid = false;
                FileIsInvalid = true;
                ConfirmIsClickable = false;
                return false;
            }

            // if multiple files were selected
            FullFileNames = fileNames;
            ShowFileNameOrCount = true;
            FileIsValid = true;
            FileIsInvalid = false;
            ConfirmIsClickable = true;
            return true;
        }
    }
}
