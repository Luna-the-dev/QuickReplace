using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    partial class DoubleInputViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string _topInputText = string.Empty;
        partial void OnTopInputTextChanged(string value)
        {
            if (value == string.Empty || ReplaceData.ReplacePhrasesDict.ContainsKey(value))
            {
                ConfirmIsClickable = false;
            }
            else
            {
                ConfirmIsClickable = true;
            }
        }

        [ObservableProperty]
        private bool _confirmIsClickable = false;
    }
}
