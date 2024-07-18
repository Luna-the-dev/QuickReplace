using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using TextReplace.Messages.Replace;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel.PopupWindows
{
    public partial class EditPhraseDoubleInputViewModel : ObservableRecipient,
        IRecipient<SelectedReplacePhraseMsg>
    {
        [ObservableProperty]
        private string _topInputText = string.Empty;
        partial void OnTopInputTextChanged(string value)
        {
            // if user did not change the Item1
            if (value == SelectedPhraseItem1)
            {
                ConfirmIsClickable = true;
            }
            else if (value == string.Empty || ReplaceData.ReplacePhrasesDict.ContainsKey(value))
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

        private string _selectedPhraseItem1 = ReplaceData.SelectedPhrase.Item1;
        public string SelectedPhraseItem1
        {
            get { return _selectedPhraseItem1; }
            set { _selectedPhraseItem1 = value; }
        }

        public void Receive(SelectedReplacePhraseMsg message)
        {
            SelectedPhraseItem1 = message.Value.Item1;
        }

    }
}
