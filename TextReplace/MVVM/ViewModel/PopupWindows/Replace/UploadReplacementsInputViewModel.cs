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
        private Visibility _showFileName = Visibility.Collapsed;
        [ObservableProperty]
        private Visibility _fileIsValid = Visibility.Collapsed;
        [ObservableProperty]
        private Visibility _fileIsInvalid = Visibility.Collapsed;

        [ObservableProperty]
        private bool _confirmIsClickable = false;

        public void ValidateFile(string fileName)
        {
            bool result = ReplaceData.SetNewReplacePhrasesFromFile(fileName, dryRun: true);
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
    }
}
