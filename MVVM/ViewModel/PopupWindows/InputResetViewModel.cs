using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    partial class InputResetViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string _inputText = string.Empty;
        partial void OnInputTextChanged(string value)
        {
            ConfirmIsClickable = (value != string.Empty);
        }

        [ObservableProperty]
        private bool _confirmIsClickable = false;
    }
}
