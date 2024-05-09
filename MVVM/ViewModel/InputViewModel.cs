using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace TextReplace.MVVM.ViewModel
{
    partial class InputViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string _inputText = string.Empty;
        partial void OnInputTextChanged(string value)
        {
            ConfirmIsClickable = (value == string.Empty) ? Visibility.Hidden : Visibility.Visible;
            ConfirmIsUnclickable = (value == string.Empty) ? Visibility.Visible : Visibility.Hidden;
        }

        [ObservableProperty]
        private Visibility _confirmIsClickable = Visibility.Hidden;
        [ObservableProperty]
        private Visibility _confirmIsUnclickable = Visibility.Visible;
    }
}
