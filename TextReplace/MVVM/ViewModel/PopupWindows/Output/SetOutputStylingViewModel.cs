using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    partial class SetOutputStylingViewModel : ObservableRecipient
    {
        // implementing the observable propertioes seperately like this due to a bug where the
        // OutputFilesStyling was getting changed even though the OutputData.OutputFilesStyling wasnt being called
        [ObservableProperty]
        private bool _bold = OutputData.OutputFilesStyling.Bold;

        [ObservableProperty]
        private bool _italics = OutputData.OutputFilesStyling.Italics;

        [ObservableProperty]
        private bool _underline = OutputData.OutputFilesStyling.Underline;

        [ObservableProperty]
        private bool _strikethrough = OutputData.OutputFilesStyling.Strikethrough;

        [ObservableProperty]
        private bool _isHighlighted = OutputData.OutputFilesStyling.IsHighlighted;

        [ObservableProperty]
        private bool _isTextColored = OutputData.OutputFilesStyling.IsTextColored;

        [ObservableProperty]
        private Color _highlightColor = OutputData.OutputFilesStyling.HighlightColor;

        [ObservableProperty]
        private Color _textColor = OutputData.OutputFilesStyling.TextColor;
    }
}
