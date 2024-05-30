using CommunityToolkit.Mvvm.ComponentModel;
using TextReplace.Core.Validation;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    partial class SetSuffixInputResetViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string _inputText = string.Empty;
        partial void OnInputTextChanged(string value)
        {
            ConfirmIsClickable = (value != string.Empty && IsSuffixValid(value));
        }

        [ObservableProperty]
        private bool _confirmIsClickable = false;

        /// <summary>
        /// Wrapper for DataValidation.IsSuffixValid()
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static bool IsSuffixValid(string suffix)
        {
            return DataValidation.IsSuffixValid(suffix);
        }
    }
}
