using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using TextReplace.Core.Validation;
using TextReplace.MVVM.Model;
using System.Windows;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    partial class SetOutputDirectoryViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string _directoryName;
        partial void OnDirectoryNameChanged(string value)
        {
            ShowDirectoryName = (value == string.Empty) ?
                Visibility.Collapsed :
                Visibility.Visible;
        }

        [ObservableProperty]
        private Visibility _showDirectoryName = Visibility.Collapsed;

        [ObservableProperty]
        private bool _confirmIsClickable = false;

        public SetOutputDirectoryViewModel()
        {
            DirectoryName = SourceFilesData.DefaultSourceFileOptions.OutputDirectory;
        }

        public void ValidateOutputDirectory(string directory)
        {
            DirectoryName = directory;
            ConfirmIsClickable = true;
        }
    }
}
