using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
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
        private Visibility _fileIsValid = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _fileIsInvalid = Visibility.Hidden;

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
                FileIsValid = Visibility.Visible;
                FileIsInvalid = Visibility.Hidden;
                ConfirmIsClickable = true;
            }
            else
            {
                FileIsValid = Visibility.Hidden;
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
            FileIsValid = Visibility.Visible;
            FileIsInvalid = Visibility.Hidden;
            ConfirmIsClickable = false;
        }

        public bool ValidateDelimiter()
        {
            if (ReplaceData.SetNewReplaceFile(FullFileName, newDelimiter: DelimiterInputText, dryRun: true))
            {
                ConfirmIsClickable = true;
                return true;
            }
            ConfirmIsClickable = false;
            return false;
        }
    }
}
