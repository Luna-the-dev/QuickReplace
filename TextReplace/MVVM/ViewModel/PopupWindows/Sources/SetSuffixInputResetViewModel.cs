using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using TextReplace.Core.Validation;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    public partial class SetSuffixInputResetViewModel : ObservableRecipient
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
