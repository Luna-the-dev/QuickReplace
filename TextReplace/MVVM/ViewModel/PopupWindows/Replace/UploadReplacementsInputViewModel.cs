using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    public partial class UploadReplacementsInputViewModel : ObservableRecipient
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
        private bool _showFileName = false;
        [ObservableProperty]
        private bool _fileIsValid = false;
        [ObservableProperty]
        private bool _fileIsInvalid = false;

        [ObservableProperty]
        private bool _confirmIsClickable = false;

        public void ValidateFile(string fileName)
        {
            bool result = ReplaceData.SetNewReplacePhrasesFromFile(fileName, dryRun: true);
            FullFileName = fileName;
            if (result)
            {
                ShowFileName = true;
                FileIsValid = true;
                FileIsInvalid = false;
                ConfirmIsClickable = true;
            }
            else
            {
                ShowFileName = true;
                FileIsValid = false;
                FileIsInvalid = true;
                ConfirmIsClickable = false;
            }
        }
    }
}
