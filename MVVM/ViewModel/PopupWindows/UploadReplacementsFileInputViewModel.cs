using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using TextReplace.Core.Validation;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    partial class UploadReplacementsInputViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string _fullFileName = string.Empty;
        partial void OnFullFileNameChanged(string value)
        {
            FileName = Path.GetFileName(value);
        }

        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private Visibility _showFileName = Visibility.Collapsed;
        [ObservableProperty]
        private Visibility _fileIsValid = Visibility.Collapsed;
        [ObservableProperty]
        private Visibility _fileIsInvalid = Visibility.Collapsed;

        [ObservableProperty]
        private string _delimiterInputText = string.Empty;
        partial void OnDelimiterInputTextChanged(string value)
        {
            // disable confirm button when user changes the delimiter
            // button is enabled on clicking the enter delimiter button
            ConfirmIsClickable = false;
            EnterDelimiterIsClickable = (value != "");
        }
        [ObservableProperty]
        private Visibility _delimiterVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private bool _enterDelimiterIsClickable = false;

        [ObservableProperty]
        private bool _confirmIsClickable = false;

        public void ValidateFile(string fileName)
        {
            bool result = ReplaceData.SetNewReplaceFile(fileName, dryRun: true);
            FullFileName = fileName;
            if (result)
            {
                ShowFileName = Visibility.Visible;
                FileIsValid = Visibility.Visible;
                FileIsInvalid = Visibility.Collapsed;
                ConfirmIsClickable = true;
            }
            else
            {
                ShowFileName = Visibility.Visible;
                FileIsValid = Visibility.Collapsed;
                FileIsInvalid = Visibility.Visible;
                ConfirmIsClickable = false;
            }
        }

        public void HideDelimiter()
        {
            DelimiterVisibility = Visibility.Collapsed;
        }

        public void ShowDelimiter(string fileName)
        {
            DelimiterVisibility = Visibility.Visible;
            FullFileName = fileName;
            ShowFileName = Visibility.Visible;
            FileIsValid = Visibility.Visible;
            FileIsInvalid = Visibility.Hidden;
            ConfirmIsClickable = false;
        }

        public bool ValidateDelimiter()
        {
            if (DataValidation.IsDelimiterValid(DelimiterInputText) == false)
            {
                return false;
            }

            if (ReplaceData.SetNewReplaceFile(FullFileName, newDelimiter: DelimiterInputText, dryRun: true) == false)
            {
                ConfirmIsClickable = false;
                return false;
            }

            ConfirmIsClickable = true;
            return true;
        }
    }
}
