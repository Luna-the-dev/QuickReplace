using CommunityToolkit.Mvvm.ComponentModel;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    partial class ReplaceFilesViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private bool _openFileLocation = OutputData.OpenFileLocation;
    }
}
