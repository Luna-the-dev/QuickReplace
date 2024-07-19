using CommunityToolkit.Mvvm.ComponentModel;
using TextReplace.Core.Validation;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    public partial class SetOutputDirectoryViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string _directoryName = SourceFilesData.DefaultSourceFileOptions.OutputDirectory;
        partial void OnDirectoryNameChanged(string value)
        {
            ShowDirectoryName = (value != string.Empty);
            ConfirmIsClickable = (value != string.Empty);
        }

        [ObservableProperty]
        private bool _showDirectoryName = false;

        [ObservableProperty]
        private bool _confirmIsClickable = false;
    }
}
