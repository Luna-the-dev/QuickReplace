using CommunityToolkit.Mvvm.ComponentModel;
using TextReplace.Core.Validation;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    partial class SetDelimiterInputViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string _inputText = string.Empty;
        partial void OnInputTextChanged(string value)
        {
            ConfirmIsClickable = DataValidation.IsDelimiterValid(value);
        }

        [ObservableProperty]
        private bool _confirmIsClickable = false;
    }
}