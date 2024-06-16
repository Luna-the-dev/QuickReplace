using CommunityToolkit.Mvvm.ComponentModel;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    partial class SetOutputStylingViewModel : ObservableRecipient
    {
        public readonly OutputFileStyling OutputFilesStyling = OutputData.OutputFilesStyling;

        // implementing the observable propertioes seperately like this due to a bug where the
        // OutputFilesStyling was getting changed even though the OutputData.OutputFilesStyling wasnt being called
        [ObservableProperty]
        private bool _bold;

        [ObservableProperty]
        private bool _italics;

        [ObservableProperty]
        private bool _underline;

        [ObservableProperty]
        private bool _strikethrough;

        public SetOutputStylingViewModel()
        {
            Bold = OutputFilesStyling.Bold;
            Italics = OutputFilesStyling.Italics;
            Underline = OutputFilesStyling.Underline;
            Strikethrough = OutputFilesStyling.Strikethrough;
        }
    }
}
