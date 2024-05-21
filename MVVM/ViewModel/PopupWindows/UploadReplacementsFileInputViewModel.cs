using CommunityToolkit.Mvvm.ComponentModel;
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
    }
}
