using CommunityToolkit.Mvvm.ComponentModel;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    partial class SetOutputStylingViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private OutputFileStyling _outputFilesStyling = OutputData.OutputFilesStyling;
    }
}
